using GB;
using System.Collections.Generic;

[System.Serializable]
public class Grid
{
    public int x;
    public int y;
}

public class ShopInvenPop : UIScreen
{
    public static bool isActive { get; private set; } = false;
    private int gw = 10; //기본 넓이 10칸
    private int gh = 12; //기본 높이 12칸
    private List<List<Grid>> grids;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("ShopInvenPop",this);
        isActive = true;
        
    }
    private void OnDisable() 
    {
        Presenter.UnBind("ShopInvenPop", this);
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
                // UnityEngine.Debug.Log("LoadSmith");
                name = "대장간";
                UnityEngine.Debug.Log($"grids: {grids.Count}");
                break;
            case "LoadTailor":
                name = "재봉사";
                break;
            case "LoadApothecary":
                name = "약제상";
                break;
        }
        mTexts["ShopName"].text = name;
        CreateGrid(data.Get<int>());
    }
    public void CreateGrid(int id)
    {
        grids = new List<List<Grid>>();
        for(int y = 0; y < gh; y++)
        {
            List<Grid> row = new List<Grid>();
            for(int x = 0; x < gw; x++)
            {
                row.Add(new Grid { x = x, y = y });
            }
            grids.Add(row);
        }

        var shopData = ShopManager.I.shopAllData[id];
        var items = shopData.items;
        foreach(var item in items)
        {
            // UnityEngine.Debug.Log($"itemId: {item.itemId}, type: {item.type}, cnt: {item.cnt}");
            ItemInfo itemInfo = (ItemInfo)ItemManager.I.GetItemInfo(item.itemId.ToString(), item.type);
            ApplyGrid(itemInfo.itemId, itemInfo.W, itemInfo.H);
        }
    }
    public void ApplyGrid(int itemId, int w, int h)
    {
       
    }

    public override void Refresh()
    {           
    }
}