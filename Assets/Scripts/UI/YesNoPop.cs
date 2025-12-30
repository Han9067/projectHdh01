using System.Collections;
using GB;
using Unity.Mathematics;


public class YesNoPop : UIScreen
{
    private string callKey = "";
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("YesNoPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("YesNoPop", this);

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
            case "ClickYes":
                switch (callKey)
                {
                    case "GuildJoin":
                        if (PlayerManager.I.pData.QuestList.FindIndex(q => q.Qid == 101) != -1)
                            PlayerManager.I.NextQuestOrder(101);
                        UIManager.ShowPopup("OneBtnPop");
                        Presenter.Send("OneBtnPop", "GuildJoin");
                        break;
                }
                Close();
                break;
            case "ClickNo":
                callKey = "";
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SendYesNoPopData":
                // callKey = data.Get<string>();
                string[] arr = data.Get<string>().Split('/');
                callKey = arr[0];
                mTexts["TxtDesc"].text = GB.LocalizationManager.GetValue(arr[1]);

                break;
        }
    }

    public override void Refresh()
    {

    }



}