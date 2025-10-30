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
        switch (key)
        {
            case "ChatGQst1":
                int npcId = data.Get<int>();
                GsManager.I.SetUiBaseParts(npcId, mGameObject, "Ot_");
                GsManager.I.SetUiEqParts(NpcManager.I.NpcDataList[npcId], "", mGameObject, "Ot_");
                GsManager.I.SetUiBaseParts(0, mGameObject, "My_");
                GsManager.I.SetUiEqParts(PlayerManager.I.pData, "", mGameObject, "My_");
                mTexts["OtName"].text = NpcManager.I.NpcDataList[npcId].Name;
                mTexts["OtMent"].text = LocalizationManager.GetValue("Ment_QstG1_1");
                var obj = Instantiate(ResManager.GetGameObject("ChatMentBtn"), parent);
                obj.GetComponent<ChatMentBtn>().SetChatMentBtn("Confirm", "Confirm", 1);
                obj.name = "ChatMentBtn_Confirm";
                chatMentBtn.Add(obj);
                PlayerManager.I.CompleteQuest(1); // 퀘스트 완료 처리
                Presenter.Send("CityEnterPop", "UpdateCityList"); //도시 팝업 갱신
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