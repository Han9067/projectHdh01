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
        // HumanAppearance.I.SetUiBaseParts(0,
        //     mGameObject["Face"].GetComponent<Image>(),
        //     mGameObject["Eyebrow"].GetComponent<Image>(),
        //     mGameObject["Eye1"].GetComponent<Image>(),
        //     mGameObject["Eye2"].GetComponent<Image>(),
        //     mGameObject["Ear"].GetComponent<Image>(),
        //     mGameObject["Nose"].GetComponent<Image>(),
        //     mGameObject["Mouth"].GetComponent<Image>(),
        //     mGameObject["BaseBody"].GetComponent<Image>(),
        //     mGameObject["BaseHand1A"].GetComponent<Image>(),
        //     mGameObject["BaseHand1A2"].GetComponent<Image>(),
        //     mGameObject["BaseHand1B"].GetComponent<Image>(),
        //     mGameObject["BaseHand2"].GetComponent<Image>(),
        //     mGameObject["BaseBoth"].GetComponent<Image>(),
        //     mGameObject["Hair1A"].GetComponent<Image>(),
        //     mGameObject["Hair1B"].GetComponent<Image>(),
        //     mGameObject["Hair2"].GetComponent<Image>());
        UpdateCharAppearance();
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
        switch (key)
        {
            case "CharInfoPopClose":
                UIManager.ClosePopup("CharInfoPop");
                break;
        }

    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "UpdateCharAppearance":
                UpdateCharAppearance();
                break;
        }
    }
    public override void Refresh() { }
    public void UpdateCharAppearance()
    {
        HumanAppearance.I.SetUiBaseParts(0,
            mGameObject["Face"].GetComponent<Image>(),
            mGameObject["Eyebrow"].GetComponent<Image>(),
            mGameObject["Eye1"].GetComponent<Image>(),
            mGameObject["Eye2"].GetComponent<Image>(),
            mGameObject["Ear"].GetComponent<Image>(),
            mGameObject["Nose"].GetComponent<Image>(),
            mGameObject["Mouth"].GetComponent<Image>(),
            mGameObject["BaseBody"].GetComponent<Image>(),
            mGameObject["BaseHand1A"].GetComponent<Image>(),
            mGameObject["BaseHand1A2"].GetComponent<Image>(),
            mGameObject["BaseHand1B"].GetComponent<Image>(),
            mGameObject["BaseHand2"].GetComponent<Image>(),
            mGameObject["BaseBoth"].GetComponent<Image>(),
            mGameObject["Hair1A"].GetComponent<Image>(),
            mGameObject["Hair1B"].GetComponent<Image>(),
            mGameObject["Hair2"].GetComponent<Image>());
    }
    public void UpdateCharInfo()
    {
        var pData = PlayerManager.I.pData;
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
    }
    private void UpdateVitText(int v) => mTexts["VitVal"].text = v.ToString();
    private void UpdateEndText(int v) => mTexts["EndVal"].text = v.ToString();
    private void UpdateStrText(int v) => mTexts["StrVal"].text = v.ToString();
    private void UpdateAgiText(int v) => mTexts["AgiVal"].text = v.ToString();
    private void UpdateForText(int v) => mTexts["ForVal"].text = v.ToString();
    private void UpdateIntText(int v) => mTexts["IntVal"].text = v.ToString();
    private void UpdateChaText(int v) => mTexts["ChaVal"].text = v.ToString();
    private void UpdateLukText(int v) => mTexts["LukVal"].text = v.ToString();
}