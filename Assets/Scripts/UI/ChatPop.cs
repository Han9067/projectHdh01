using GB;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class ChatPop : UIScreen
{
    [SerializeField] private Transform parent;
    private List<GameObject> chatMentBtn = new List<GameObject>();
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("ChatPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("ChatPop", this);
        InitChatBtn();
    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });

    }
    public void OnButtonClick(string key)
    {
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "ChatStart":
                List<int> list = data.Get<List<int>>();//퀘스트 ID, NPC ID, Order ID
                int qid = list[0], npcId = list[1], order = list[2];
                GsManager.I.SetUiBaseParts(npcId, mGameObject, true, "Ot_");
                GsManager.I.SetUiEqParts(NpcManager.I.NpcDataList[npcId], "", mGameObject, true, "Ot_");
                GsManager.I.SetUiBaseParts(0, mGameObject, true, "My_");
                GsManager.I.SetUiEqParts(PlayerManager.I.pData, "", mGameObject, true, "My_");
                mTMPText["OtName"].text = NpcManager.I.NpcDataList[npcId].Name;
                string mentId = "";
                switch (qid)
                {
                    case 1:
                        mentId = "Talk_QstG1_1";
                        SetMyAskPreset("Confirm");
                        break;
                    case 101:
                        switch (order)
                        {
                            case 1:
                                mentId = "Talk_QstM_Tuto_1";
                                SetMyAskPreset("QstM_Tuto_1");
                                break;
                            case 3:
                                mentId = "Talk_QstM_Tuto_2_1";
                                SetMyAskPreset("QstM_Tuto_2_1");
                                break;
                            case 5:
                                mentId = "Talk_QstM_Tuto_5";
                                SetMyAskPreset("QstM_Tuto_5");
                                break;
                        }
                        break;
                }
                mTMPText["OtMent"].text = LocalizationManager.GetValue(mentId);
                break;
            case "ChatMentBtn":
                string sKey = data.Get<string>();
                switch (sKey)
                {
                    case "Confirm":
                        Close();
                        break;
                    case "QstM_Tuto_1":
                        PlayerManager.I.NextQuestOrder(101);
                        Presenter.Send("CityEnterPop", "Tuto_Join");
                        Close();
                        break;
                    case "QstM_Tuto_2_1":
                        NextChat("QstM_Tuto_2_2", "Talk_QstM_Tuto_2_2");
                        break;
                    case "QstM_Tuto_2_2":
                        PlayerManager.I.NextQuestOrder(101);
                        WorldCore.I.CreateTutoMarker();
                        UIManager.ClosePopup("CityEnterPop");
                        Close();
                        break;
                    case "QstM_Tuto_5":
                        PlayerManager.I.CompleteMainQuest(101);
                        Presenter.Send("WorldMainUI", "SetTraceQst");
                        UIManager.ClosePopup("CityEnterPop");
                        WorldCore.I.CheckAllAreaWorldMon();
                        Close();
                        break;
                }
                break;
        }
    }
    void SetMyAskPreset(string str)
    {
        List<string> askKeyList = new List<string>();
        List<string> askMentList = new List<string>();
        switch (str)
        {
            case "Confirm":
                askKeyList.Add("Confirm");
                askMentList.Add("Confirm");
                break;
            case "QstM_Tuto_1":
                askKeyList.Add("QstM_Tuto_1");
                askMentList.Add("Confirm");
                break;
            case "QstM_Tuto_2_1":
                askKeyList.Add("QstM_Tuto_2_1");
                askMentList.Add("Yes");
                askKeyList.Add("QstM_Tuto_2_1");
                askMentList.Add("Okay");
                break;
            case "QstM_Tuto_2_2":
                askKeyList.Add("QstM_Tuto_2_2");
                askMentList.Add("Confirm");
                break;
            case "QstM_Tuto_5":
                askKeyList.Add("QstM_Tuto_5");
                askMentList.Add("Confirm");
                break;
        }
        for (int i = 0; i < askKeyList.Count; i++)
        {
            var obj = Instantiate(ResManager.GetGameObject("ChatMentBtn"), parent);
            obj.GetComponent<ChatMentBtn>().SetChatMentBtn(askKeyList[i], askMentList[i], i + 1);
            chatMentBtn.Add(obj);
        }
    }
    private void NextChat(string key, string desc)
    {
        InitChatBtn();
        mTMPText["OtMent"].text = LocalizationManager.GetValue(desc);
        SetMyAskPreset(key);
    }
    private void InitChatBtn()
    {
        foreach (GameObject obj in chatMentBtn)
        {
            if (obj != null)
                Destroy(obj);
        }
        chatMentBtn.Clear();
    }
    public override void Refresh() { }

}