using GB;
using System.Collections.Generic;
using UnityEngine;

public class TalkPop : UIScreen
{
    [SerializeField] private Transform parent;
    [SerializeField] private RectTransform box;
    private List<GameObject> talkMentBtn = new List<GameObject>();
    private string talkKey = "";
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("TalkPop", this);
        talkKey = "";
    }

    private void OnDisable()
    {
        Presenter.UnBind("TalkPop", this);
        InitChatBtn();
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
            case "ClickNext":
                //현재 대화를 종료하면서 그 이후에 발생해야하는 이벤트를 적용해줌
                switch (talkKey)
                {
                    default:
                        Close();
                        break;
                }
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "TalkStart":
                int tType = 0; //0: 상대방 혼자 대화, 1: 나만 대화, 2: 둘이 대화
                List<int> list = data.Get<List<int>>();//퀘스트 ID, NPC ID, Order ID
                int qid = list[0], npcId = list[1], order = list[2];
                string mentId = "";
                switch (qid)
                {
                    case 0:
                        mentId = "Talk_Test";
                        break;
                    case 1:
                        mentId = "Talk_QstG1_1";
                        foreach (var q in PlayerManager.I.pData.QuestList)
                        {
                            if (q.Qid == 101 && q.CityId == WorldCore.intoCity)
                            {
                                PlayerManager.I.CompleteGuildQst(q.QUid);
                                break;
                            }
                        }
                        break;
                    case 101:
                        switch (order)
                        {
                            case 1:
                                //길드 안내원이 길드 가입 권유 -> NPC 혼자 대화
                                mentId = "Talk_QstM_Tuto_1";
                                SetMyAskPreset("QstM_Tuto_1");
                                break;
                            case 3:
                                //길드 가입 후 길드 안내원이 퀘스트 권유 -> 둘이 대화
                                mentId = "Talk_QstM_Tuto_2_1";
                                SetMyAskPreset("QstM_Tuto_2_1");
                                tType = 2;
                                break;
                            case 5:
                                //퀘스트 클리어 후 길드 안내원과의 대화 -> NPC 혼자 대화
                                mentId = "Talk_QstM_Tuto_5";
                                SetMyAskPreset("QstM_Tuto_5");
                                break;
                        }
                        break;
                }
                SetTalkType(tType, npcId, mentId);
                break;
            case "talkMentBtn":
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
                        WorldCore.I.CreateWorldMarker(new Vector3(-9.65f, -35.2f, 0), 999);
                        UIManager.ClosePopup("CityEnterPop");
                        Close();
                        break;
                    case "QstM_Tuto_5":
                        PlayerManager.I.ClearMainQst(101);
                        Presenter.Send("WorldMainUI", "SetTraceQst");
                        UIManager.ClosePopup("CityEnterPop");
                        WorldCore.I.CheckAllAreaWorldMon();
                        Close();
                        break;
                }
                break;
            case "NextBtn":
                Close();
                break;
        }
    }
    void SetTalkType(int type, int nId, string mentId)
    {
        mGameObject["LineObj"].SetActive(type == 2);
        bool isOt = type == 0 || type == 2;
        bool isMy = type == 1 || type == 2;
        mTMPText["OtName"].gameObject.SetActive(isOt);
        mButtons["ClickNext"].gameObject.SetActive(type != 2);
        //0: 상대방 혼자 대화, 1: 나만 대화, 2: 둘이 대화
        switch (type)
        {
            case 0:
            case 1:
                box.sizeDelta = new Vector2(1600, 350);
                mGameObject["OtObj"].SetActive(isOt);
                mGameObject["MyObj"].SetActive(isMy);
                mGameObject["NextObj"].SetActive(true);
                mGameObject["OtObj"].transform.localPosition = new Vector3(0, -530, 0);
                break;
            case 2:
                box.sizeDelta = new Vector2(1600, 600);
                mGameObject["NextObj"].SetActive(false);
                mGameObject["OtObj"].transform.localPosition = new Vector3(0, -280, 0);
                break;
        }
        if (isOt)
        {
            GsManager.I.SetUiBaseParts(nId, mGameObject, true, "Ot_");
            GsManager.I.SetUiEqParts(NpcManager.I.NpcDataList[nId], mGameObject, "Ot_");
            mTMPText["OtName"].text = NpcManager.I.NpcDataList[nId].Name;
            mTMPText["OtMent"].text = LocalizationManager.GetValue(mentId);
        }
        if (isMy)
        {
            GsManager.I.SetUiBaseParts(0, mGameObject, true, "My_");
            GsManager.I.SetUiEqParts(PlayerManager.I.pData, mGameObject, "My_");
        }
    }
    void SetMyAskPreset(string str)
    {
        //플레이어의 대화 선택지가 필요할떄, 선택지 버튼 생성하는 함수
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
            var obj = Instantiate(ResManager.GetGameObject("TalkAskBtn"), parent);
            obj.GetComponent<TalkAskBtn>().SetTalkAskBtn(askKeyList[i], askMentList[i], i + 1);
            talkMentBtn.Add(obj);
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
        foreach (GameObject obj in talkMentBtn)
        {
            if (obj != null)
                Destroy(obj);
        }
        talkMentBtn.Clear();
    }
    public override void Refresh() { }
}