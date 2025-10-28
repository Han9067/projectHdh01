using GB;
using UnityEngine;
using System.Collections.Generic;


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
        int npcId = data.Get<int>();
        switch (key)
        {
            case "ChatGQst1":
                GsManager.I.SetUiBaseParts(npcId, mGameObject, "Ot_");
                GsManager.I.SetUiBaseParts(0, mGameObject, "My_");
                mTexts["OtName"].text = NpcManager.I.NpcDataList[npcId].Name;
                mTexts["MyName"].text = PlayerManager.I.pData.Name;
                mTexts["OtMent"].text = LocalizationManager.GetValue("Ment_QstG1_1");
                int idx = 0;
                chatMentBtn.Add(ResManager.GetGameObject("ChatMentBtn"));
                chatMentBtn[idx].GetComponent<ChatMentBtn>().SetChatMentBtn("Confirm", "Confirm");
                chatMentBtn[idx].name = "ChatMentBtn_Confirm";
                //
                break;
            case "ChatMentBtn":
                string sKey = data.Get<string>();
                switch (sKey)
                {
                    case "Confirm":
                        Close();
                        break;
                }
                break;
        }
    }

    public override void Refresh()
    {

    }

}