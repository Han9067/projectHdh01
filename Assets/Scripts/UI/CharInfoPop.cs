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
        HumanAppearance.I.SetUiBaseParts(PlayerManager.I.pData,
            mGameObject["Face"].GetComponent<Image>(),
            mGameObject["Eyebrow"].GetComponent<Image>(),
            mGameObject["Eye"].GetComponent<Image>(),
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
    public override void ViewQuick(string key, IOData data) { }
    public override void Refresh() { }
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
        // UpdateEq(pData);
    }
    public void UpdateEq(PlayerData pData)
    {
        var eq = pData.EqSlot;
        string[] allParts = {"BaseHand1A", "BaseHand1A2", "BaseHand1B", "BaseHand2", "BaseBoth",
            "EqBody", "EqHand1A", "EqHand1B", "EqHand2", "EqBoth", "OneWp1", "OneWp2", "TwoWp1", "TwoWp2", "TwoWp3"};
        foreach (var v in allParts)
            mGameObject[v].SetActive(false);
        curBodyKey = "";

        if (eq["Armor"] != null)
        {
            string eqStr = eq["Armor"].ItemId.ToString();
            if (curBodyKey != eqStr + "_body")
            {
                curBodyKey = eqStr + "_body";
                mGameObject["EqBody"].GetComponent<Image>().sprite = ResManager.GetSprite(curBodyKey);
                mGameObject["EqHand1A"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1A");
                mGameObject["EqHand1B"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1B");
                mGameObject["EqHand2"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand2");
                mGameObject["EqBoth"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_both");
            }
            mGameObject["EqBody"].SetActive(true);

            List<string> parts = new List<string>();
            if (eq["Hand1"] == null && eq["Hand2"] == null)
                parts = new List<string> { "BaseHand1A", "BaseHand2", "EqHand1A", "EqHand2" };
            else
            {
                string[] eqHand = { "Hand1", "Hand2" };
                foreach (var v in eqHand)
                {
                    if (eq[v] != null)
                    {
                        switch (eq[v].Both)
                        {
                            case 1: parts = new List<string> { "BaseBoth", "EqBoth" }; break;
                            case 2: parts = new List<string> { "BaseHand1A", "BaseHand1A2", "BaseHand2", "EqHand1A", "EqHand2" }; break;
                        }
                    }
                }
                if (parts.Count == 0)
                {
                    parts = new List<string> { "BaseHand2", "EqHand2" };
                    if (eq["Hand1"] != null)
                    {
                        parts.Add("BaseHand1B"); parts.Add("EqHand1B");
                    }
                    else
                    {
                        parts.Add("BaseHand1A"); parts.Add("EqHand1A");
                    }
                }
            }
            foreach (var v in parts)
                mGameObject[v].SetActive(true);
        }

        string[] hKey = { "Hand1", "Hand2" };
        foreach (var v in hKey)
        {
            if (eq[v] != null)
            {
                switch (eq[v].Both)
                {
                    case 0:
                        string one = v == "Hand1" ? "OneWp1" : "OneWp2";
                        mGameObject[one].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq[v].ItemId.ToString());
                        mGameObject[one].SetActive(true);
                        break;
                    case 1:
                        mGameObject["TwoWp1"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                        mGameObject["TwoWp1"].SetActive(true);
                        break;
                    case 2:
                        mGameObject["TwoWp2"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                        mGameObject["TwoWp2"].SetActive(true);
                        break;
                }
            }
        }
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