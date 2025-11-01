using GB;
using UnityEngine;

public class OneBtnPop : UIScreen
{
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("OneBtnPop", this);
    }
    private void OnDisable()
    {
        Presenter.UnBind("OneBtnPop", this);

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
            case "ClickClose":
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "GuildJoin":
                PlayerManager.I.pData.Grade = 1;
                Debug.Log("GuildJoin");
                mImages["GradeIcon"].color = ColorData.GetBadgeGradeColor(1);
                mTexts["GradeDesc"].text = LocalizationManager.GetValue("One_GuildJoin");
                Presenter.Send("CityEnterPop", "UpdateCityList");
                break;
        }
    }

    public override void Refresh() { }
}