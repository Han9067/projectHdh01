using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class QuestManager : AutoSingleton<QuestManager>
{
    private QuestTable _questTable;
    public QuestTable QuestTable => _questTable ?? (_questTable = GameDataManager.GetTable<QuestTable>());
    public Dictionary<int, QuestData> QuestData = new Dictionary<int, QuestData>();
    public Dictionary<int, Dictionary<int, QuestInstData>> CityQuest = new Dictionary<int, Dictionary<int, QuestInstData>>();
    private void Awake()
    {
        LoadQuestData();
    }
    private void Start()
    {
        for (int i = 0; i < 5; i++)
            SetQuestsInCity(i);
        // foreach (var city in CityManager.I.CityDataList)
        //     SetQuestsInCity(city.Key);
    }
    private void LoadQuestData()
    {
        foreach (var quest in QuestTable.Datas)
            QuestData[quest.QuestID] = new QuestData(quest.QuestID, quest.Name, quest.Days);
    }
    private void SetQuestsInCity(int cityID)
    {
        CityQuest[cityID] = new Dictionary<int, QuestInstData>();
        for (int i = 1; i <= 2; i++)
        {
            CityQuest[cityID][i] = new QuestInstData(i, cityID, QuestTable.Datas[i].Name);
            string desc = ""; //퀘스트 설명
            int tg = 0; //타겟 ID
            int days = 1, star = 1, exp = 0, crown = 0, gradeExp = 0; //별, 경험치, 왕관, 등급 경험치

            switch (i)
            {
                case 1:
                    List<int> tgCity = new List<int> { 1, 2, 3, 4, 5 };
                    tgCity.Remove(cityID);
                    tg = tgCity[Random.Range(0, tgCity.Count)];
                    desc = string.Format(LocalizationManager.GetValue("Quest_1"), CityManager.I.GetCityName(tg));
                    star = 1;
                    days = 30; exp = 600; crown = 2000; gradeExp = 100;
                    CityQuest[cityID][i].SetQuestBase(desc, days, star, exp, crown, gradeExp);
                    CityQuest[cityID][i].CityId = tg;
                    break; //다른 도시에 편지 전달 퀘스트
                case 2:
                    //유저의 등급 수준에 맞춰 최저점과 최고점을 결정(현재 등급에서 1단계 아래가 최저점, 1단계 위로 최고점)
                    //현재 퀘스트 신청하는 도시의 주변 스폰 지역 검색
                    //등급에 따라 도시 주변 스폰하는 몬스터들을 한곳으로 저장
                    //저장된 리스트에서 랜덤으로 몬스터 하나 선택
                    int min = GetMinGrade(PlayerManager.I.pData.Grade), max = GetMaxGrade(PlayerManager.I.pData.Grade);
                    int[] Area = CityManager.I.CityDataList[cityID].Area;
                    List<int> GrpList = new List<int>();
                    foreach (var area in Area)
                    {
                        for (int j = min; j <= max; j++)
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
                    int cnt = Random.Range(10, 30);
                    desc = string.Format(LocalizationManager.GetValue("Quest_2"), MonManager.I.MonDataList[tg].Name, cnt);
                    star = GetStarToMon(tg);
                    days = 30; exp = star + (cnt * 100); crown = star + (cnt * 200); gradeExp = star + (cnt * 20);
                    CityQuest[cityID][i].SetQuestBase(desc, days, star, exp, crown, gradeExp);
                    CityQuest[cityID][i].MonId = tg;
                    CityQuest[cityID][i].Cnt = cnt;
                    break; //특정 몬스터 처치 퀘스트
                case 3:
                    break; //특정 아이템 획득 퀘스트
                case 4:
                    break; //특정 장소에 발견된 몬스터 무리를 토벌 퀘스트
                case 5:
                    break; //상인 호위 퀘스트
            }
        }
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
}
