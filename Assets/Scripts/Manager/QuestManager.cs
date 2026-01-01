using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class QuestManager : AutoSingleton<QuestManager>
{
    #region 퀘스트 관련
    private QuestTable _questTable;
    public QuestTable QuestTable => _questTable ?? (_questTable = GameDataManager.GetTable<QuestTable>());
    public Dictionary<int, QuestData> QuestData = new Dictionary<int, QuestData>();
    public Dictionary<int, Dictionary<int, QuestInstData>> CityQuest = new Dictionary<int, Dictionary<int, QuestInstData>>();
    public void LoadQuestManager()
    {
        LoadQuestData();
    }
    private void Start()
    {
        if (GsManager.I.gameState == GameState.Battle)
        {
            // LoadQst();
            return;
        }
        //최초 시작시 로드된 CityQuest가 없을 경우 새로 만들어줌
        LoadQst();
    }
    public void LoadQst()
    {
        // isLoad = true;
        if (CityQuest.Count == 0)
        {
            // int cnt = PlayerManager.I.isGate1Open ? 3 : 5;
            for (int i = 1; i <= 3; i++)
                SetQuestsInCity(i);
        }
    }
    private void LoadQuestData()
    {
        foreach (var quest in QuestTable.Datas)
            QuestData[quest.QuestID] = new QuestData(quest.QuestID, quest.Name, quest.Type, quest.Trace == 0);
    }
    private void SetQuestsInCity(int cityID)
    {
        //예외처리: 극단적으로 최하등급 모험가때 받은 퀘스트를 B등급 될때까지 냅둔다면 그것도 문제이기에 모험가 등급이 상승될때, 현재 받고 있는 길드 퀘스트들은 전부 초기화 됨
        CityQuest[cityID] = new Dictionary<int, QuestInstData>();
        for (int i = 1; i <= 2; i++)
        {
            CityQuest[cityID][i] = new QuestInstData(i, cityID, QuestData[i].Type, QuestData[i].Name, QuestData[i].IsTrace);
            string desc; //퀘스트 설명
            int tg; //타겟 ID
            int star, exp, crown, gradeExp; //별, 경험치, 왕관, 등급 경험치
            int minG, maxG; //최저 등급, 최고 등급
            switch (i)
            {
                case 1:
                    //추후 tgCity는 관문 통행증에 따라 조정
                    List<int> tgCity = new List<int> { 1, 2, 3 };
                    tgCity.Remove(cityID);
                    tg = tgCity[Random.Range(0, tgCity.Count)]; //목표 도시
                    desc = string.Format(LocalizationManager.GetValue("QstG_Delivery_Desc"), PlaceManager.I.GetCityName(tg));
                    star = 1;
                    exp = 200; crown = 500; gradeExp = 100; //당장은 냅두고 추후에 해당 수치들은 거리에 따라 조정(거리는 도로 타일로 계산할것)
                    CityQuest[cityID][i].SetQuestBase(desc, star, exp, crown, gradeExp);
                    CityQuest[cityID][i].CityId = tg;
                    //튜토리얼 지역 3 도시는 관문 통행 완료 전까지 3곳에서 서로 주고받기
                    break; //다른 도시에 편지 전달 퀘스트
                case 2:
                case 3:
                    //유저의 등급 수준에 맞춰 최저점과 최고점을 결정(현재 등급에서 1단계 아래가 최저점, 1단계 위로 최고점)
                    //현재 퀘스트 신청하는 도시의 주변 스폰 지역 검색
                    //등급에 따라 도시 주변 스폰하는 몬스터들을 한곳으로 저장
                    //저장된 리스트에서 랜덤으로 몬스터 하나 선택
                    //만약 몬스터 처치면 그대로 몬스터를 저장하고, 전리품 제출이면 몬스터의 드랍 아이템(68000번 이상)을 찾아서 저장
                    minG = GetMinGrade(PlayerManager.I.pData.Grade); maxG = GetMaxGrade(PlayerManager.I.pData.Grade);
                    List<int> Area = PlaceManager.I.CityDic[cityID].Area;
                    List<int> GrpList = new List<int>();
                    foreach (var area in Area)
                    {
                        for (int j = minG; j <= maxG; j++)
                        {
                            foreach (var grp in WorldObjManager.I.areaDataList[area].grpByGrade[j])
                            {
                                if (!GrpList.Contains(grp))
                                    GrpList.Add(grp);
                            }
                        }
                    }
                    List<int> MonList = new List<int>();
                    for (int j = 0; j < GrpList.Count; j++)
                    {
                        if (!MonList.Contains(WorldObjManager.I.monGrpData[GrpList[j]].LeaderID))
                            MonList.Add(WorldObjManager.I.monGrpData[GrpList[j]].LeaderID);
                    }
                    tg = MonList[Random.Range(0, MonList.Count)];
                    int cnt;
                    star = GetStarToMon(tg);
                    CityQuest[cityID][i].CurCnt = 0;
                    switch (i)
                    {
                        case 2:
                            cnt = Random.Range(10, 30);
                            desc = string.Format(LocalizationManager.GetValue("QstG_KillMon_Desc"), cnt, LocalizationManager.GetValue(MonManager.I.MonDataList[tg].Name));
                            exp = star * cnt * 50; crown = star * cnt * 40; gradeExp = star * cnt * 10;
                            CityQuest[cityID][i].SetQuestBase(desc, star, exp, crown, gradeExp);
                            CityQuest[cityID][i].MonId = tg;
                            CityQuest[cityID][i].TgCnt = cnt;
                            break;
                        case 3:
                            cnt = Random.Range(4, 20);
                            exp = star * cnt * 60; crown = star * cnt * 55; gradeExp = star * cnt * 12;
                            var item = ItemManager.I.ItemDataList[MonManager.I.MonDataList[tg].DropList[0].ItemId];
                            desc = string.Format(LocalizationManager.GetValue("QstG_KillMon_Desc"), cnt, LocalizationManager.GetValue(item.Name));
                            CityQuest[cityID][i].SetQuestBase(desc, star, exp, crown, gradeExp);
                            CityQuest[cityID][i].ItemId = item.ItemId;
                            CityQuest[cityID][i].TgCnt = cnt;
                            break;
                    }
                    break; //특정 몬스터 처치 퀘스트
                case 4:
                    break; //특정 장소에 발견된 몬스터 무리를 토벌 퀘스트
                case 5:
                    break; //상인 호위 퀘스트
            }
        }
    }
    public void ResetCityQuest()
    {
        CityQuest.Clear();
        for (int i = 1; i <= 5; i++)
            SetQuestsInCity(i);
    }
    int GetMinGrade(int grade)
    {
        if (grade == 0)
            return 0;
        else return grade - 1;
    }
    int GetMaxGrade(int grade)
    {
        if (grade >= 8)
            return 10;
        else
            return grade + 1;
    }
    int GetStarToMon(int monId)
    {
        int val = 1 + (MonManager.I.MonDataList[monId].Lv / 20);
        if (val > 10)
            val = 10;
        return val;
    }
    #endregion
    #region 마커 관련

    #endregion
}
