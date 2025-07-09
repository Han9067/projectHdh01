using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GB;

public class ShopManager : AutoSingleton<ShopManager>
{
    [Header("Shop Data")]
    private ShopTable shopTable;
    private ShopItemTable shopItemTable;
    public Dictionary<int, ShopData> shopAllData = new Dictionary<int, ShopData>();
    
    void Start()
    {
        shopTable = GameDataManager.GetTable<ShopTable>();
        shopItemTable = GameDataManager.GetTable<ShopItemTable>();
        InitShopData();
    }
    public void InitShopData()
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