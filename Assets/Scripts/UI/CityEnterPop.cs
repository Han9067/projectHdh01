using System.Diagnostics;
using GB;
using QuickEye.Utility;


public class CityEnterPop : UIScreen
{
    public static bool IsActive { get; private set; } = false;

    private void Awake()
    {
        Regist();
        RegistButton();
        RegistText();
    }


    private void OnEnable()
    {
        Presenter.Bind("CityEnterPop", this);
        IsActive = true;
        
        // UnityEngine.Debug.Log("CityEnterPop OnEnable");
        // UnityDictionary<string, Text> mTexts;
    }

    private void OnDisable() 
    {
        Presenter.UnBind("CityEnterPop", this);
        IsActive = false;
    }

    public void RegistButton()
    {
        foreach(var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key);});
        
    }

    public void RegistText()
    {
        foreach(var v in mTexts)
            v.Value.text = v.Key;
    }

    public void OnButtonClick(string key)
    {
        switch (key)
        {
            case "Close":
                // UIManager.ClosePopup("CityEnterPop");
                Close();
                break;
            case "Guild":
                UnityEngine.Debug.Log("Guild");
                break;
            case "Inn":
                break;
            case "Smith":
                UIManager.ShowPopup("ShopInvenPop");
                break;
            case "Tailor":
                break;
            case "Potion":
                break;
            case "Variety":
                break;
            case "Traning":
                break;
        };
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch(key)
        {
            case "EnterCity":
                // UnityEngine.Debug.Log(data.Get<string>())
                string cityId = data.Get<string>();
                var table = GameDataManager.GetTable<CityTable>();
                mTexts["Name"].text = table[cityId].Name;
                break;
        }
    }

    public override void Refresh()
    {
            
    }



}