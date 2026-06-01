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
    public Dictionary<int, List<QuestInstData>> CityQuest = new Dictionary<int, List<QuestInstData>>();
    public int curMkUid = 0; //마커 퀘스트를 수락하면 마커가 생성되는데 그 마커의 uid를 여기에 저장
    public void LoadQuestManager()
    {
        LoadQuestData();
    }
    private void Start()
    {
        if (GsManager.gameState == GameState.Battle)
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
        CityQuest[cityID] = new List<QuestInstData>();
        int showGrade = PlayerManager.I.pData.Grade + 1;
        int curG = 1;
        for (int i = 0; i < showGrade; i++)
        {
            List<int> gQst = GetQstGradeList(curG);
            int cnt = Random.Range(5, 6);
            // Debug.Log("cityID: " + cityID + " curG: " + curG + " cnt: " + cnt);
            for (int j = 0; j < cnt; j++)
            {
                int type = gQst[Random.Range(0, gQst.Count)];
                var qst = new QuestInstData(type, cityID, QuestData[type].Type, curG, QuestData[type].Name, QuestData[type].IsTrace);
                CityQuest[cityID].Add(qst);
                ///
                string desc; //퀘스트 설명
                int tg, tgCnt, exp, crown, gradeExp; //타겟 ID, 목표 횟수, 경험치, 왕관, 등급 경험치
                ///
                switch (type)
                {
                    case 1:
                        //추후 tgCity는 관문 통행증에 따라 조정
                        tg = GetQstTgCity(cityID); //목표 도시
                        desc = string.Format(LocalizationManager.GetValue("QstG_Delivery_Desc"), PlaceManager.I.GetCityName(tg));
                        exp = 200; crown = 500; gradeExp = 100; //당장은 냅두고 추후에 해당 수치들은 거리에 따라 조정(거리는 도로 타일로 계산할것)
                        qst.SetQuestBase(desc, exp, crown, gradeExp);
                        qst.CityId = tg;
                        //튜토리얼 지역 3 도시는 관문 통행 완료 전까지 3곳에서 서로 주고받기
                        break; //다른 도시에 편지 전달 퀘스트
                    case 11://특정 몬스터 처치 퀘스트
                        tg = GetQstTgMon(cityID, curG, type)[0];
                        tgCnt = Random.Range(10, 30);
                        desc = string.Format(LocalizationManager.GetValue("QstG_KillMon_Desc"), tgCnt, LocalizationManager.GetValue(MonManager.I.MonDataList[tg].Name));
                        exp = curG * tgCnt * 50; crown = curG * tgCnt * 40; gradeExp = curG * tgCnt * 10;
                        qst.SetQuestBase(desc, exp, crown, gradeExp);
                        qst.MonId = tg;
                        qst.TgCnt = tgCnt;
                        break;
                    case 21://특정 몬스터 드랍 아이템 제출 퀘스트
                        tg = GetQstTgMon(cityID, curG, type)[0];
                        tgCnt = Random.Range(4, 20);
                        exp = curG * tgCnt * 60; crown = curG * tgCnt * 55; gradeExp = curG * tgCnt * 12;
                        var item = ItemManager.I.ItemDataList[MonManager.I.MonDataList[tg].DropList[0].ItemId];
                        desc = string.Format(LocalizationManager.GetValue("QstG_Supply_Desc"), tgCnt, LocalizationManager.GetValue(item.Name));
                        qst.SetQuestBase(desc, exp, crown, gradeExp);
                        qst.ItemId = item.ItemId;
                        qst.TgCnt = tgCnt;
                        break;
                    case 31://길드_특정 장소의 몬스터 무리 토벌
                        List<int> list = GetQstTgMon(cityID, curG, type);
                        desc = LocalizationManager.GetValue("QstG_FindMonGrp_Desc");
                        tgCnt = WorldObjManager.I.monGrpData[list[0]].Max * 2;
                        exp = curG * tgCnt * 50; crown = curG * tgCnt * 40; gradeExp = curG * tgCnt * 10;
                        qst.SetQuestBase(desc, exp, crown, gradeExp);
                        qst.TgCnt = 1;
                        qst.TgPos = WorldObjManager.I.GetSpawnPos(list[1]);
                        break;
                    case 41://상단 호위
                        // tg = GetQstTgCity(cityID); //목표 도시
                        break;
                }
            }
            curG++;
        }
    }
    public void ResetCityQuest()
    {
        CityQuest.Clear();
        for (int i = 1; i <= 3; i++)
            SetQuestsInCity(i);
    }
    private List<int> GetQstGradeList(int grade)
    {
        return new List<int> { 1, 11, 21 }; //테스트
        // //등급에 따라 퀘스트 타입 리스트 반환
        // switch (grade)
        // {
        //     case 1:
        //         return new List<int> { 1, 11, 21, 31, 41 };
        //     case 2:
        //         return new List<int> { 1, 11, 21, 31, 41, 51, 61 };
        //     case 3:
        //         return new List<int> { 1, 11, 21, 31, 41, 51, 61, 71, 101 };
        //     default:
        //         return new List<int> { 1, 11, 21, 31, 41, 51, 61, 71 };
        // }
    }
    private int GetQstTgCity(int cityId)
    {
        List<int> tgCity = new List<int> { 1, 2, 3 };
        tgCity.Remove(cityId);
        return tgCity[Random.Range(0, tgCity.Count)];
    }
    private List<int> GetQstTgMon(int cityId, int grade, int type)
    {
        List<int> Area = PlaceManager.I.CityDic[cityId].Area;
        List<int> GrpList = new List<int>();
        List<int> AreaList = new List<int>();
        foreach (var aa in Area)
        {
            foreach (var grp in WorldObjManager.I.areaDataList[aa].grpByGrade[grade])
            {
                if (!GrpList.Contains(grp))
                {
                    GrpList.Add(grp);
                    AreaList.Add(aa);
                }
            }
        }
        switch (type)
        {
            case 11:
            case 21:
                List<int> MonList = new List<int>();
                for (int j = 0; j < GrpList.Count; j++)
                {
                    if (!MonList.Contains(WorldObjManager.I.monGrpData[GrpList[j]].LeaderID))
                        MonList.Add(WorldObjManager.I.monGrpData[GrpList[j]].LeaderID);
                }
                return new List<int> { MonList[Random.Range(0, MonList.Count)] }; //몬스터 ID
            case 31:
                int ran = Random.Range(0, GrpList.Count);
                return new List<int> { GrpList[ran], AreaList[ran] }; //몬스터 그룹 ID, 구역 ID
            default:
                return new List<int>();
        }
    }
    private int GetQstTgItem(int mId)
    {
        return 65001;
    }
    public int GetQstUid()
    {
        return 10000000 + Random.Range(0, 89999999);
    }
    #endregion
    #region 마커 관련

    #endregion
}
