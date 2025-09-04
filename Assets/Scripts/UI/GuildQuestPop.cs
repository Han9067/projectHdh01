using GB;
using UnityEngine;


public class GuildQuestPop : UIScreen
{
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("GuildQuestPop", this);
    }
    private void OnDisable()
    {
        Presenter.UnBind("GuildQuestPop", this);

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
            case "QuestClose":
                Close();
                break;
            case "QuestAccept":
                Debug.Log("QuestAccept 클릭되었습니다!");
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {

    }

    public override void Refresh()
    {

    }



}