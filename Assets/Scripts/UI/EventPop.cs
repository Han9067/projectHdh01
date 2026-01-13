using GB;
using System.Collections.Generic;
using UnityEngine;

public class EventPop : UIScreen
{
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("EventPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("EventPop", this);

    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });

    }

    public void OnButtonClick(string key)
    {
        switch (key)
        {
            case "OnBattle":
                WorldCore.I.SceneFadeOut(); //페이드
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetEvent":
                List<int> list = data.Get<List<int>>();
                switch (list[0])
                {
                    case 1:
                        mTMPText["Ment"].text = LocalizationManager.GetValue("Event_QstG_FindMonGrp");
                        foreach (var q in PlayerManager.I.pData.QuestList)
                        {
                            if (q.MarkerUid == list[1])
                            {
                                MonGrpData grp = WorldObjManager.I.monGrpData[q.MonId];
                                // int cnt = Random.Range(grp.Min, grp.Max) * 2;
                                int cnt = grp.Min; //테스트용
                                List<int> monList = new List<int>();
                                for (int i = 0; i < cnt; i++)
                                {
                                    int m = grp.List[Random.Range(0, grp.List.Count)];
                                    monList.Add(m);
                                }
                                WorldObjManager.I.SetEventMon(monList);
                                break;
                            }
                        }
                        break;
                }
                break;
        }
    }

    public override void Refresh() { }
}