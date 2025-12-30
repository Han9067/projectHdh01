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
                mImages["GradeIcon"].color = ColorData.GetBadgeGradeColor(1);
                mTMPText["GradeDesc"].text = LocalizationManager.GetValue("One_GuildJoin");
                mTMPText["GradeName"].text = string.Format(LocalizationManager.GetValue("ItemGrade"), GetGradeName(1));
                Presenter.Send("CityEnterPop", "UpdateCityList");
                break;
            case "GradeUp":
                PlayerManager.I.pData.Grade++;
                mImages["GradeIcon"].color = ColorData.GetBadgeGradeColor(PlayerManager.I.pData.Grade);
                mTMPText["GradeName"].text = string.Format(LocalizationManager.GetValue("ItemGrade"), GetGradeName(PlayerManager.I.pData.Grade));
                Presenter.Send("CityEnterPop", "UpdateCityList");
                break;
        }
    }
    string GetGradeName(int grade)
    {
        switch (grade)
        {
            case 1: return "H";
            case 2: return "G";
            case 3: return "F";
            case 4: return "E";
            case 5: return "D";
            case 6: return "C";
            case 7: return "B";
            case 8: return "A";
            case 9: return "S";
            case 10: return "SS";
        }
        return "";
    }
    public override void Refresh() { }
}