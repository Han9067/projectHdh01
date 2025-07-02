using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GB;

public class ShopManager : AutoSingleton<ShopManager>
{
    // [System.Serializable]
    public class ShopData
    {
        public int id; // 상점ID
        public string name; // 상점이름
        public int cityId; // 도시ID
        public int type; // 상점타입
        public List<ShopItemData> items; // 상점아이템
        
        public ShopData()
        {
            items = new List<ShopItemData>();
        }
    }

    public class ShopItemData
    {
        public int itemId;
        public int type;
        public int cnt;
        
        public ShopItemData(int iId, int type, int cnt)
        {
            this.itemId = iId;
            this.type = type;
            this.cnt = cnt;
        }
    }

    [Header("Shop Data")]
    private ShopTable shopTable;
    private ShopItemTable shopItemTable;
    public Dictionary<int, ShopData> shopAllData = new Dictionary<int, ShopData>();
    
    void Start()
    {
        shopTable = GameDataManager.GetTable<ShopTable>();
        shopItemTable = GameDataManager.GetTable<ShopItemTable>();
        LoadShopData();
    }
    public void LoadShopData()
    {   
        shopAllData.Clear();
        int len = shopTable.Count;
        for(int i = 0; i < len; i++)
        {
            shopAllData.Add(shopTable[i].ID, new ShopData{id = shopTable[i].ID, name = shopTable[i].Name, cityId = shopTable[i].CityID, type = shopTable[i].Type});
        }
        int len2 = shopItemTable.Count;
        for(int j = 0; j < len2; j++)
        {   
            var shopItem = shopItemTable[j];
            shopAllData[shopItem.ShopID].items.Add(new ShopItemData(shopItem.ItemID, shopItem.Type, shopItem.Cnt));
        }
    }
} 