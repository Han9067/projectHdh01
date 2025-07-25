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
        LoadBaseCharAppearance(); //최초 1번 로드
    }
    private void OnEnable()
    {
        Presenter.Bind("CharInfoPop",this);
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
        foreach(var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key);});
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
    public override void ViewQuick(string key, IOData data){}
    public override void Refresh(){}
    private void LoadBaseCharAppearance(){
        var pData = PlayerManager.I.pData;
        mGameObject["Face"].GetComponent<Image>().sprite = ResManager.GetSprite("Face_" + pData.Face);
        mGameObject["Eyebrow"].GetComponent<Image>().sprite = ResManager.GetSprite("Eyebrow_" + pData.Eyebrow);
        mGameObject["Eye"].GetComponent<Image>().sprite = ResManager.GetSprite("Eye_" + pData.Eye);
        mGameObject["Ear"].GetComponent<Image>().sprite = ResManager.GetSprite("Ear_" + pData.Ear);
        mGameObject["Nose"].GetComponent<Image>().sprite = ResManager.GetSprite("Nose_" + pData.Nose);
        mGameObject["Mouth"].GetComponent<Image>().sprite = ResManager.GetSprite("Mouth_" + pData.Mouth);

        float[] arr = {0, 0, 0};
        switch(pData.Skin){
            case 1:arr = new float[] {255f, 235f, 210f};break;
            case 2:arr = new float[] {236f, 204f, 169f};break;
            case 3:arr = new float[] {251f, 206f, 177f};break;
            case 4:arr = new float[] {199f, 165f, 137f};break;
            case 5:arr = new float[] {92f, 73f, 57f};break;default:arr = new float[] {0, 0, 0};break;
        }
        arr = new float[] {arr[0] / 255f, arr[1] / 255f, arr[2] / 255f};
        mGameObject["Face"].GetComponent<Image>().color = new Color(arr[0], arr[1], arr[2], 1f);
        mGameObject["Ear"].GetComponent<Image>().color = new Color(arr[0], arr[1], arr[2], 1f);
        mGameObject["BaseBody"].GetComponent<Image>().color = new Color(arr[0], arr[1], arr[2], 1f);
        mGameObject["BaseHand1A"].GetComponent<Image>().color = new Color(arr[0], arr[1], arr[2], 1f);
        mGameObject["BaseHand1B"].GetComponent<Image>().color = new Color(arr[0], arr[1], arr[2], 1f);
        mGameObject["BaseHand2"].GetComponent<Image>().color = new Color(arr[0], arr[1], arr[2], 1f);
        mGameObject["BaseBoth"].GetComponent<Image>().color = new Color(arr[0], arr[1], arr[2], 1f);

        switch(pData.HairColor){
            case 1:arr = new float[] {30f, 30f, 30f};break;
            case 2:arr = new float[] {57f, 41f, 28f};break;
            case 3:arr = new float[] {104f, 69f, 35f};break;
            case 4:arr = new float[] {138f, 97f, 69f};break;
            case 5:arr = new float[] {219f, 169f, 93f};break;
            case 6:arr = new float[] {245f, 238f, 286f};break;
            case 7:arr = new float[] {170f, 255f, 216f};break;
        }

        switch(pData.Hair){
            case 1:
                mGameObject["Hair1A"].SetActive(true);mGameObject["Hair1B"].SetActive(false);mGameObject["Hair2"].SetActive(true);
                mGameObject["Hair1A"].GetComponent<Image>().sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                mGameObject["Hair2"].GetComponent<Image>().sprite = ResManager.GetSprite("Hair_2_" + pData.Hair);
                mGameObject["Hair1A"].GetComponent<Image>().color = new Color(arr[0] / 255f, arr[1] / 255f, arr[2] / 255f, 1f);
                mGameObject["Hair2"].GetComponent<Image>().color = new Color(arr[0] / 255f, arr[1] / 255f, arr[2] / 255f, 1f);
                break;
            case 2:
            case 3:
                mGameObject["Hair1A"].SetActive(true);mGameObject["Hair1B"].SetActive(false);mGameObject["Hair2"].SetActive(false);
                mGameObject["Hair1A"].GetComponent<Image>().sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                mGameObject["Hair1A"].GetComponent<Image>().color = new Color(arr[0] / 255f, arr[1] / 255f, arr[2] / 255f, 1f);
                break;
            case 100:
                mGameObject["Hair1A"].SetActive(false);mGameObject["Hair1B"].SetActive(true);mGameObject["Hair2"].SetActive(true);
                mGameObject["Hair1B"].GetComponent<Image>().sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                mGameObject["Hair2"].GetComponent<Image>().sprite = ResManager.GetSprite("Hair_2_" + pData.Hair);
                mGameObject["Hair1B"].GetComponent<Image>().color = new Color(arr[0] / 255f, arr[1] / 255f, arr[2] / 255f, 1f);
                mGameObject["Hair2"].GetComponent<Image>().color = new Color(arr[0] / 255f, arr[1] / 255f, arr[2] / 255f, 1f);
                break;
        }
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
        mTexts["AccVal"].text = pData.Acc.ToString() + "%";
        mTexts["DodVal"].text = pData.Dod.ToString() + "%";

        UpdateVitText(pData.VIT);
        UpdateEndText(pData.END);
        UpdateStrText(pData.STR);
        UpdateAgiText(pData.AGI);
        UpdateForText(pData.FOR);
        UpdateIntText(pData.INT);
        UpdateChaText(pData.CHA);
        UpdateLukText(pData.LUK);

        UpdateEq(pData);
    }
    public void UpdateEq(PlayerData pData)
    {
        var eq = pData.EqSlot;

        if (eq["Armor"] != null) {
            string idStr = eq["Armor"].ItemId.ToString();
            if(curBodyKey != idStr + "_body"){
                curBodyKey = idStr + "_body";
                mGameObject["EqBody"].GetComponent<Image>().sprite = ResManager.GetSprite(curBodyKey);
                mGameObject["EqHand1A"].GetComponent<Image>().sprite = ResManager.GetSprite(idStr + "_hand1A");
                mGameObject["EqHand1B"].GetComponent<Image>().sprite = ResManager.GetSprite(idStr + "_hand1B");
                mGameObject["EqHand2"].GetComponent<Image>().sprite = ResManager.GetSprite(idStr + "_hand2");
                mGameObject["EqBoth"].GetComponent<Image>().sprite = ResManager.GetSprite(idStr + "_both");
            }

            List<string> body;
            if (eq["Hand1"] == null && eq["Hand2"] == null) {
                body = new List<string> { "EqBody", "EqHand1A", "EqHand2" };
            } else {
                if (eq["Hand1"].Both == 1 || eq["Hand2"].Both == 1) {
                    body = new List<string> { "EqBody", "EqBoth" };
                } else if (eq["Hand1"].Both == 2 || eq["Hand2"].Both == 2) {
                    body = new List<string> { "EqBody", "EqHand1A", "EqHand2" };
                } else {
                    body = new List<string> { "EqBody", "EqHand1B", "EqHand2" };
                }
            }
            foreach(var v in body)
                mGameObject[v].SetActive(true);
        }else{
            string[] body = {"EqBody", "EqHand1A", "EqHand1B", "EqHand2", "EqBoth"};
            foreach(var v in body)
                mGameObject[v].SetActive(false);
            curBodyKey = "";
        }
    }
    private void UpdateVitText(int v)  => mTexts["VitVal"].text = v.ToString();
    private void UpdateEndText(int v)  => mTexts["EndVal"].text = v.ToString();
    private void UpdateStrText(int v)  => mTexts["StrVal"].text = v.ToString();
    private void UpdateAgiText(int v)  => mTexts["AgiVal"].text = v.ToString();
    private void UpdateForText(int v)  => mTexts["ForVal"].text = v.ToString();
    private void UpdateIntText(int v)  => mTexts["IntVal"].text = v.ToString();
    private void UpdateChaText(int v)  => mTexts["ChaVal"].text = v.ToString();
    private void UpdateLukText(int v)  => mTexts["LukVal"].text = v.ToString();
}