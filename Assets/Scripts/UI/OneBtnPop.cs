using GB;
using UnityEngine;

public class OneBtnPop : UIScreen
{
    [SerializeField] private string oneBtnKey;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("OneBtnPop", this);
        InitMainObj();
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
            case "BtnObj":
                switch (oneBtnKey)
                {
                    case "ViewRewards":
                        //추후 보상 팝업 활성화
                        Close();
                        break;
                    default:
                        Close();
                        break;
                }
                break;
        }
    }
    private void InitMainObj()
    {
        mGameObject["Grade"].SetActive(false);
        mGameObject["Clear"].SetActive(false);
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "GuildJoin":
                oneBtnKey = "GuildJoin";
                mGameObject["Grade"].SetActive(true);
                SetBtnInfo(LocalizationManager.GetValue("Close"));
                PlayerManager.I.pData.Grade = 1;
                mImages["GradeIcon"].sprite = ResManager.GetSprite("icon_badge_H");
                mTMPText["GradeDesc"].text = LocalizationManager.GetValue("One_GuildJoin");
                mTMPText["GradeName"].text = string.Format(LocalizationManager.GetValue("ItemGrade"), GsManager.GetGradeName(1));
                Presenter.Send("CityEnterPop", "UpdateCityList");
                break;
            case "GradeUp":
                oneBtnKey = "GradeUp";
                mGameObject["Grade"].SetActive(true);
                SetBtnInfo(LocalizationManager.GetValue("Close"));
                mTMPText["ClearDesc"].text = LocalizationManager.GetValue("ItemGrade");
                Presenter.Send("CityEnterPop", "UpdateCityList");
                break;
            case "ExpClear":
                oneBtnKey = "ExpClear";
                mGameObject["Clear"].SetActive(true);
                SetBtnInfo(LocalizationManager.GetValue("ViewRewards"));
                mTMPText["ClearDesc"].text = LocalizationManager.GetValue("One_ExpClear");
                break;
        }
    }
    private void SetBtnInfo(string txt)
    {
        mTMPText["BtnTxt"].text = txt;
    }
    public override void Refresh() { }
}