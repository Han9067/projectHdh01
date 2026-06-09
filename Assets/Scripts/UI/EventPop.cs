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
            case "OnLeft":
                WorldCore.I.SceneFadeOut(); //페이드
                break;
            case "OnRight":
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetEvent":
                List<int> list = data.Get<List<int>>();
                int mkUid = list[0];
                int eventID = list[1];
                string title = "";
                string ment = "";
                if (eventID < 200001)
                {
                    title = LocalizationManager.GetValue("Evt_FindMonGrp_Title");
                    ment = LocalizationManager.GetValue("Evt_FindMonGrp_Ment");
                    foreach (var q in PlayerManager.I.pData.MainQst)
                    {
                        if (q.MarkerUid == mkUid)
                        {
                            MonGrpData grp = WorldObjManager.I.monGrpData[q.MonId];
                            int cnt = Random.Range(grp.Min, grp.Max) * 2;
                            List<int> monList = new List<int>();
                            for (int i = 0; i < cnt; i++)
                            {
                                int m = grp.List[Random.Range(0, grp.List.Count)];
                                monList.Add(m);
                            }
                            WorldObjManager.I.SetMonList(monList);
                        }
                    }
                }
                else if (eventID < 300001)
                {
                    switch (eventID)
                    {
                        case 200001:
                            title = LocalizationManager.GetValue("Evt_BanditFortress_Title");
                            ment = LocalizationManager.GetValue("Evt_BanditFortress_Ment");
                            break;
                        case 200002:
                            title = LocalizationManager.GetValue("Evt_OrcFortress_Title");
                            ment = LocalizationManager.GetValue("Evt_OrcFortress_Ment");
                            break;
                    }
                }
                else if (eventID < 400001)
                {

                }
                mTMPText["Title"].text = title;
                mTMPText["Ment"].text = ment;
                break;
        }
    }

    public override void Refresh() { }
}