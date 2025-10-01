using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class QuestManager : AutoSingleton<QuestManager>
{
    private QuestTable _questTable;
    public QuestTable QuestTable => _questTable ?? (_questTable = GameDataManager.GetTable<QuestTable>());
    public Dictionary<int, QuestData> QuestDataList = new Dictionary<int, QuestData>();

    private void Awake()
    {
        // LoadQuestData();
    }
    private void Start()
    {
        // foreach (var city in CityManager.I.CityDataList)
        // {
        //     SetQuestsInCity(city.Key);
        // }
        //CityGradeQuests
    }
    private void LoadQuestData()
    {
        // foreach (var quest in QuestTable.Datas)
        //     QuestDataList[quest.QuestID] = new QuestData(quest.QuestID, quest.Name, quest.Type, quest.Days, quest.Cnt);
    }
    private void SetQuestsInCity(int cityID)
    {
        //도시마다 특정 기간이 지나면 퀘스트가 갱신되는데 현재 퀘스트의 갯수는 등급별 5개씩 생성하도록 해야함.
        //일단 H,G,F,E,D 까지만 구현 준비 -> 떄문에 반복문을 5번만 돌림
        // for (int i = 1; i <= 5; i++)
        // {
        //     CityGradeQuests[cityID][i] = new List<QuestInstData>();
        //     for (int j = 0; j < 5; j++)
        //     {
        //         int id = Random.Range(1, 4);
        //         string desc = GetDesc(id);
        //         int cnt = Random.Range(QuestDataList[id].Min, QuestDataList[id].Max);
        //         CityGradeQuests[cityID][i].Add(new QuestInstData(id, QuestDataList[id].Name, desc, QuestDataList[id].Type, QuestDataList[id].Days, cnt, i));
        //     }
        // }
    }
    private string GetDesc(int id)
    {
        string desc = "";
        switch (id)
        {
            case 1:
                return desc = "A도시의 B에게 편지를 전달해주세요.";
            case 2:
                return desc = "A를 처치하세요.";
            case 3:
                return desc = "A를 획득 후 전달해주세요.";
            case 4:
                return desc = "월드맵 특정 위치를 조사해주세요.";
            case 5:
                return desc = "A로 가는 상단을 호위해주세요.";
        }
        return desc;
    }
}
