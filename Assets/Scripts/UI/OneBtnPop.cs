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
                mImages["GradeIcon"].sprite = ResManager.GetSprite("icon_badge_H");
                mTMPText["GradeDesc"].text = LocalizationManager.GetValue("One_GuildJoin");
                mTMPText["GradeName"].text = string.Format(LocalizationManager.GetValue("ItemGrade"), GsManager.GetGradeName(1));
                Presenter.Send("CityEnterPop", "UpdateCityList");
                break;
            case "GradeUp":
                PlayerManager.I.pData.Grade++;
                mImages["GradeIcon"].sprite = ResManager.GetSprite($"icon_badge_{GsManager.GetGradeName(PlayerManager.I.pData.Grade)}");
                mTMPText["GradeName"].text = string.Format(LocalizationManager.GetValue("ItemGrade"), GsManager.GetGradeName(PlayerManager.I.pData.Grade));
                Presenter.Send("CityEnterPop", "UpdateCityList");
                break;
        }
    }
    public override void Refresh() { }
}