using System.Diagnostics;
using GB;
using QuickEye.Utility;
using System.Collections.Generic;
using System.Linq;


public class CityEnterPop : UIScreen
{
    public static bool isActive { get; private set; } = false;
    private int curId;
    private List<int> splitData = new List<int>();
    public Dictionary<int, int> shopList = new Dictionary<int, int>();
    
    private void Awake()
    {
        Regist();
        RegistButton();
        RegistText();
    }
    private void OnEnable()
    {
        Presenter.Bind("CityEnterPop", this);
        isActive = true;
        curId = 0; // 초기화
        WorldCore.I.enabled = false; // 월드맵 카메라 이동 비활성화
        splitData.Clear();
        shopList.Clear();
    }

    private void OnDisable() 
    {
        Presenter.UnBind("CityEnterPop", this);
        isActive = false;
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
                break;
            case "Inn":
                break;
            case "Smith":
                OpenShopPop(key, 2);  
                break;
            case "Tailor":
                OpenShopPop(key, 3);
                break;
            case "Apothecary":
                OpenShopPop(key, 4);
                break;
            case "Traning":
                break;
        };
    }
    void OpenShopPop(string key, int type)
    {
        UIManager.ShowPopup("ShopInvenPop");
        UIManager.ShowPopup("InvenPop");    
        Presenter.Send("ShopInvenPop","Load" + key, shopList[type]);
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch(key)
        {
            case "EnterCity":
                curId = data.Get<int>();
                var table = GameDataManager.GetTable<CityTable>();
                mTexts["Name"].text = table[curId].Name;
                string[] strArr = table[curId].Place.Split('_');
                splitData = strArr.Select(int.Parse).ToList();
                LoadPlace();
                break;
        }
    }
    private void LoadPlace()
    {
        string[] keys = { "Guild", "Inn", "Smith", "Tailor", "Apothecary", "Market", "Traning" };
        foreach (var key in keys)
            mButtons[key].gameObject.SetActive(false);
        int y = 350;
        foreach(var v in splitData)
        {
            string btnKey = null;
            switch(v)
            {
                case 0: btnKey = "Guild"; break;
                case 1: btnKey = "Inn"; break;
                case 2: btnKey = "Smith"; break;
                case 3: btnKey = "Tailor"; break;
                case 4: btnKey = "Apothecary"; break;
                case 5: btnKey = "Traning"; break;
            }

            if (btnKey != null && mButtons.ContainsKey(btnKey))
            {
                var btn = mButtons[btnKey];
                btn.gameObject.SetActive(true);

                // 버튼의 위치를 y값으로 지정
                var pos = btn.transform.localPosition;
                btn.transform.localPosition = new UnityEngine.Vector3(pos.x, y, pos.z);

                y -= 100;
            }
            // shopAllData에서 조건에 맞는 데이터 찾기
            var result = ShopManager.I.shopAllData.Values
                .Where(data => data.cityId == curId && data.type == v)
                .ToList();
            foreach(var shop in result)
                shopList[v] = shop.id;
        }
    }
    public override void Refresh()
    {
            
    }
}