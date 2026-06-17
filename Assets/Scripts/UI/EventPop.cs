using GB;
using System.Collections.Generic;
using UnityEngine;

public class EventPop : UIScreen
{
    public static bool isActive { get; private set; } = false;
    private int eventID = 0, mkUid = 0;
    private string left = "", right = "";

    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("EventPop", this);
        isActive = true;
        eventID = 0;
        mkUid = 0;
        left = "";
        right = "";
        GsManager.I.CheckWorldCmr();
    }

    private void OnDisable()
    {
        Presenter.UnBind("EventPop", this);
        isActive = false;
        GsManager.I.CheckWorldCmr();
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
                if (eventID < 200001)
                {
                    GsManager.I.SetBtSeed();
                    WorldCore.I.GoToBattle();
                }
                else if (eventID < 300001)
                {
                    GsManager.I.SetBtSeed("Tile_201");
                    WorldCore.I.GoToBattle();
                }
                else if (eventID < 400001)
                {
                    Close();
                    Presenter.Send("WorldMainUI", "ShowExplorePop", eventID);
                }
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
                mkUid = list[0];
                eventID = list[1];
                string title = "", ment = "";
                if (eventID < 200001)
                {
                    left = "Fight";
                    right = "Run";
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
                    left = "Fight";
                    right = "Run";
                    switch (eventID)
                    {
                        case 200001:
                            title = LocalizationManager.GetValue("Evt_BanditFortress_Title");
                            ment = LocalizationManager.GetValue("Evt_BanditFortress_Ment");
                            break;
                        case 200011:
                            title = LocalizationManager.GetValue("Evt_OrcFortress_Title");
                            ment = LocalizationManager.GetValue("Evt_OrcFortress_Ment");
                            break;
                    }
                }
                else if (eventID < 400001)
                {
                    left = "Explore";
                    right = "Leave";
                    switch (eventID)
                    {
                        case 300001:
                            title = LocalizationManager.GetValue("Evt_ExpGoblin_TItle");
                            ment = LocalizationManager.GetValue("Evt_ExpGoblin_Ment");
                            break;
                        case 300011:
                            title = LocalizationManager.GetValue("Evt_ExpWolf_TItle");
                            ment = LocalizationManager.GetValue("Evt_ExpWolf_Ment");
                            break;
                        case 300021:
                            title = LocalizationManager.GetValue("Evt_ExpSpider_TItle");
                            ment = LocalizationManager.GetValue("Evt_ExpSpider_Ment");
                            break;
                    }
                }
                mTMPText["Left"].text = LocalizationManager.GetValue(left);
                mTMPText["Right"].text = LocalizationManager.GetValue(right);
                mTMPText["Title"].text = title;
                mTMPText["Ment"].text = ment;
                break;
        }
    }

    public override void Refresh() { }
}