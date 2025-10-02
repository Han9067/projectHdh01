using System.Diagnostics;
using GB;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;


public class CharInfoPop : UIScreen
{
    public bool isActive { get; private set; } = false;
    private string curBodyKey = "";
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        AppearanceManager.I.SetUiBaseParts(0, mGameObject);
    }
    private void OnEnable()
    {
        Presenter.Bind("CharInfoPop", this);
        UpdateCharInfo();
        isActive = true;
    }
    private void OnDisable()
    {
        Presenter.UnBind("CharInfoPop", this);
        isActive = false;
    }
    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }
    public void OnButtonClick(string key)
    {
        if (key.Contains("ClickAdd"))
        {
            switch (key)
            {
                case "ClickAddVit":
                    PlayerManager.I.pData.VIT++;
                    break;
                case "ClickAddEnd":
                    PlayerManager.I.pData.END++;
                    break;
                case "ClickAddStr":
                    PlayerManager.I.pData.STR++;
                    break;
                case "ClickAddAgi":
                    PlayerManager.I.pData.AGI++;
                    break;
                case "ClickAddFor":
                    PlayerManager.I.pData.FOR++;
                    break;
                case "ClickAddInt":
                    PlayerManager.I.pData.INT++;
                    break;
                case "ClickAddCha":
                    PlayerManager.I.pData.CHA++;
                    break;
                case "ClickAddLuk":
                    PlayerManager.I.pData.LUK++;
                    break;
            }
            PlayerManager.I.pData.Exp -= PlayerManager.I.pData.NextExp;
            PlayerManager.I.pData.Lv += 1;
            PlayerManager.I.pData.NextExp = ObjLevelManager.I.GetNextExp(PlayerManager.I.pData.Lv);
            UpdateCharInfo();
        }
        else
        {
            switch (key)
            {
                case "CharInfoPopClose":
                    Close();
                    break;
            }
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "UpdateCharAppearance":
                AppearanceManager.I.SetUiBaseParts(0, mGameObject);
                break;
        }
    }
    public override void Refresh() { }
    public void UpdateCharInfo()
    {
        var pData = PlayerManager.I.pData;
        //string text = GB.LocalizationManager.GetValue("0"); //로컬라이즈 매니저에서 사용되는 언어 호출....추후 소제목들 이걸로 대응
        #region 기본 정보
        mTexts["NameVal"].text = pData.Name;
        mTexts["AgeVal"].text = pData.Age.ToString();
        mTexts["LvVal"].text = pData.Lv.ToString();
        mTexts["ExpVal"].text = pData.Exp.ToString();
        mTexts["NextExpVal"].text = pData.NextExp.ToString();

        mTexts["HpVal"].text = pData.HP.ToString() + "/" + pData.MaxHP.ToString();
        mTexts["MpVal"].text = pData.MP.ToString() + "/" + pData.MaxMP.ToString();
        mTexts["SpVal"].text = pData.SP.ToString() + "/" + pData.MaxSP.ToString();
        mTexts["AttVal"].text = pData.Att.ToString();
        mTexts["DefVal"].text = pData.Def.ToString();
        mTexts["CrtVal"].text = pData.Crt.ToString() + "%";
        mTexts["CrtRateVal"].text = pData.CrtRate.ToString() + "%";
        mTexts["HitVal"].text = pData.Hit.ToString() + "%";
        mTexts["EvaVal"].text = pData.Eva.ToString() + "%";

        mTexts["VitVal"].text = pData.VIT.ToString();
        mTexts["EndVal"].text = pData.END.ToString();
        mTexts["StrVal"].text = pData.STR.ToString();
        mTexts["AgiVal"].text = pData.AGI.ToString();
        mTexts["ForVal"].text = pData.FOR.ToString();
        mTexts["IntVal"].text = pData.INT.ToString();
        mTexts["ChaVal"].text = pData.CHA.ToString();
        mTexts["LukVal"].text = pData.LUK.ToString();
        #endregion
        AppearanceManager.I.SetUiEqParts(pData, curBodyKey, mGameObject);
        if (curBodyKey != mGameObject["EqBody"].GetComponent<Image>().sprite.name)
            curBodyKey = mGameObject["EqBody"].GetComponent<Image>().sprite.name;

        UpdateStatAdd(); // 스탯 추가 버튼 활성화
        UpdateGrade(); // 등급 표시
    }
    void UpdateStatAdd()
    {
        bool isActive = PlayerManager.I.pData.Exp >= PlayerManager.I.pData.NextExp;
        string[] arr = new string[] { "ClickAddVit", "ClickAddEnd", "ClickAddStr", "ClickAddAgi", "ClickAddFor", "ClickAddInt", "ClickAddCha", "ClickAddLuk" };
        foreach (string s in arr)
            mButtons[s].gameObject.SetActive(isActive);
    }
    void UpdateGrade()
    {
        int g = PlayerManager.I.pData.Grade;
        mTexts["GradeVal"].text = GetGradeName(g);
        mTexts["GradeVal"].transform.localPosition = g == 0 ? new Vector3(0, -48f, 0) : new Vector3(20f, -48f, 0);
        mImages["BadgeIcon"].gameObject.SetActive(g != 0);
        if (g > 0)
        {
            mImages["BadgeIcon"].color = ColorData.GetGradeColor(g);
            int n = g > 3 ? 1 : 2;
            mImages["BadgeIcon"].sprite = ResManager.GetSprite($"icon_badge{n}");
        }
    }
    string GetGradeName(int grade)
    {
        switch (grade)
        {
            case 0: return "없음";
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
}