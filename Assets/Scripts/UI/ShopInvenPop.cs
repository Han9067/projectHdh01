using GB;

public class ShopInvenPop : UIScreen
{
    public static bool IsActive { get; private set; } = false;
    // private ShopTable _shopTable;
    // private ShopItemTable _shopItemTable;
    // private ShopTable ShopTable => _shopTable ??= GameDataManager.GetTable<ShopTable>();
    // private ShopItemTable ShopItemTable => _shopItemTable ??= GameDataManager.GetTable<ShopItemTable>();
    
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("ShopInvenPop",this);
        IsActive = true;
    }

    private void OnDisable() 
    {
        Presenter.UnBind("ShopInvenPop", this);
        IsActive = false;
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
                UIManager.ClosePopup("ShopInvenPop");
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {   
        string name = "";
        switch(key)
        {
            case "LoadSmith":
                UnityEngine.Debug.Log("LoadSmith");
                name = "대장간";
                
                var wpTable = GameDataManager.GetTable<WpTable>();
                //var wpTable = GameDataManager.GetTable<WpTable>();

                break;
            case "LoadTailor":
                name = "재봉사";
                break;
            case "LoadApothecary":
                name = "약제상";
                break;
        }
        mTexts["ShopName"].text = name;
    }

    public override void Refresh()
    {
            
    }
}