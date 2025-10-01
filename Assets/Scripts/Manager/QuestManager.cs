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
        for (int i = 1; i <= 5; i++)
        {
            CityQuest[cityID][i] = new QuestInstData(i, cityID);
            string desc = ""; //퀘스트 설명
            int tg = 0; //타겟 ID
            int days = 0, star = 0, exp = 0, crown = 0, gradeExp = 0; //별, 경험치, 왕관, 등급 경험치

            switch (i)
            {
                case 1:
                    List<int> tgCity = new List<int> { 1, 2, 3, 4, 5 };
                    tgCity.Remove(cityID);
                    tg = tgCity[Random.Range(0, tgCity.Count)];
                    desc = string.Format(LocalizationManager.GetValue("Quest_1"), CityManager.I.GetCityName(tg));
                    CityQuest[cityID][i].SetQuestBase(desc, days, star, exp, crown, gradeExp);
                    CityQuest[cityID][i].CityId = tg;
                    break; //다른 도시에 편지 전달 퀘스트
                case 2:
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
}
