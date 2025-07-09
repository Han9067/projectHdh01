using GB;


public class CharInfoPop : UIScreen
{

    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("CharInfoPop",this);
        UpdateAllStatTexts();
    }
    private void OnDisable() 
    {
        Presenter.UnBind("CharInfoPop", this);
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
            case "Close":
                UIManager.ClosePopup("CharInfoPop");
                break;
        }
        
    }
    public override void ViewQuick(string key, IOData data){}
    public override void Refresh(){}
    public void UpdateAllStatTexts()
    {
        var pData = PlayerManager.I.pData;
        mTexts["NameVal"].text = pData.Name;
        mTexts["AgeVal"].text = pData.Age.ToString();
        mTexts["LvVal"].text = pData.Lv.ToString();
        mTexts["ExpVal"].text = pData.Exp.ToString();
        mTexts["NextExpVal"].text = pData.NextExp.ToString();

        UpdateVitText(pData.VIT);
        UpdateEndText(pData.END);
        UpdateStrText(pData.STR);
        UpdateAgiText(pData.AGI);
        UpdateForText(pData.FOR);
        UpdateIntText(pData.INT);
        UpdateChaText(pData.CHA);
        UpdateLukText(pData.LUK);
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