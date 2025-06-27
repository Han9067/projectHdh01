using System.Diagnostics;
using GB;
using QuickEye.Utility;
using System.Collections.Generic;
using System.Linq;


public class CityEnterPop : UIScreen
{
    public static bool IsActive { get; private set; } = false;
    private string curId = "";
    private List<ShopTableProb> shops = new List<ShopTableProb>();

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
        curId = ""; // 초기화
        WorldCore.I.enabled = false; // 월드맵 카메라 이동 비활성화

    }

    private void OnDisable() 
    {
        Presenter.UnBind("CityEnterPop", this);
        IsActive = false;
        WorldCore.I.enabled = true; // 월드맵 카메라 이동 활성화
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
                Close();
                UIManager.ClosePopup("InvenPop");
                break;
            case "Guild":
                UnityEngine.Debug.Log("Guild");
                break;
            case "Inn":
                break;
            case "Smith":
            case "Tailor":
            case "Apothecary":
                OpenShopPop(key);            
                break;
            case "Traning":
                break;
        };
    }
    void OpenShopPop(string key)
    {
        UIManager.ShowPopup("ShopInvenPop");
        UIManager.ShowPopup("InvenPop");    
        Presenter.Send("ShopInvenPop","Load" + key, curId);
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch(key)
        {
            case "EnterCity":
                curId = data.Get<string>();
                var table = GameDataManager.GetTable<CityTable>();
                mTexts["Name"].text = table[curId].Name;
                
                LoadShops(curId);
                break;

        }
    }

    private void LoadShops(string cityId)
    {
        var shopTable = GameDataManager.GetTable<ShopTable>();
        shops = shopTable.Datas.Where(shop => shop.CItyID == cityId).ToList();

        UnityEngine.Debug.Log($"도시 {cityId}의 상점 개수: {shops.Count}");
    
        foreach (var shop in shops)
        {
            UnityEngine.Debug.Log($"상점: {shop.Name} (ID: {shop.ID}, Type: {shop.Type}, CityID: {shop.CItyID})");
        }
    }

    public override void Refresh()
    {
            
    }



}