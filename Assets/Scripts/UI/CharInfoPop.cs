using System.Diagnostics;
using GB;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;


public class CharInfoPop : UIScreen
{
    public bool isActive { get; private set; } = false;
    private string curBodyKey = "";
    [SerializeField] private GameObject statAddParent;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        HumanAppearance.I.SetUiBaseParts(0, mGameObject);
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
            PlayerManager.I.pData.NextExp = LevelData.I.GetNextExp(PlayerManager.I.pData.Lv);
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
                HumanAppearance.I.SetUiBaseParts(0, mGameObject);
                break;
        }
    }
    public override void Refresh() { }
    public void UpdateCharInfo()
    {
        var pData = PlayerManager.I.pData;
        //string text = GB.LocalizationManager.GetValue("0"); //로컬라이즈 매니저에서 사용되는 언어 호출....추후 소제목들 이걸로 대응
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

        UpdateVitText(pData.VIT);
        UpdateEndText(pData.END);
        UpdateStrText(pData.STR);
        UpdateAgiText(pData.AGI);
        UpdateForText(pData.FOR);
        UpdateIntText(pData.INT);
        UpdateChaText(pData.CHA);
        UpdateLukText(pData.LUK);

        HumanAppearance.I.SetUiEqParts(pData, curBodyKey, mGameObject);
        if (curBodyKey != mGameObject["EqBody"].GetComponent<Image>().sprite.name)
            curBodyKey = mGameObject["EqBody"].GetComponent<Image>().sprite.name;

        statAddParent.SetActive(PlayerManager.I.pData.Exp >= PlayerManager.I.pData.NextExp);
    }
    void UpdateVitText(int v) => mTexts["VitVal"].text = v.ToString();
    void UpdateEndText(int v) => mTexts["EndVal"].text = v.ToString();
    void UpdateStrText(int v) => mTexts["StrVal"].text = v.ToString();
    void UpdateAgiText(int v) => mTexts["AgiVal"].text = v.ToString();
    void UpdateForText(int v) => mTexts["ForVal"].text = v.ToString();
    void UpdateIntText(int v) => mTexts["IntVal"].text = v.ToString();
    void UpdateChaText(int v) => mTexts["ChaVal"].text = v.ToString();
    void UpdateLukText(int v) => mTexts["LukVal"].text = v.ToString();
}