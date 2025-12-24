using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
public class PlaceManager : AutoSingleton<PlaceManager>
{
    #region 도시
    private CityTable _cityTable;
    public CityTable CityTable => _cityTable ?? (_cityTable = GameDataManager.GetTable<CityTable>());
    public Dictionary<int, CityData> CityDic = new Dictionary<int, CityData>();
    private void LoadCityData()
    {
        // foreach (var city in CityTable.Datas)
        //     CityDic[city.CityID] = new CityData(city.CityID, city.Name, city.Place);
        for (int i = 0; i < 3; i++)
        {
            Debug.Log(CityTable.Datas[i].Name);
            CityDic[CityTable.Datas[i].CityID] = new CityData(CityTable.Datas[i].CityID, CityTable.Datas[i].Name, CityTable.Datas[i].Place, CityTable.Datas[i].Area);
        }
    }
    public string GetCityName(int cityID)
    {
        return LocalizationManager.GetValue(CityDic[cityID].Name);
    }
    #endregion

    #region 상점
    private ShopTable shopTable;
    private ShopItemTable shopItemTable;
    public Dictionary<int, ShopData> shopAllData = new Dictionary<int, ShopData>();
    public void LoadShopData()
    {
        shopTable = GameDataManager.GetTable<ShopTable>();
        shopItemTable = GameDataManager.GetTable<ShopItemTable>();
        shopAllData.Clear();
        int len = shopTable.Count;
        for (int i = 0; i < len; i++)
        {
            shopAllData.Add(shopTable[i].ShopID, new ShopData
            {
                Id = shopTable[i].ShopID,
                CityId = shopTable[i].CityID,
                Type = shopTable[i].Type,
                NpcId = shopTable[i].NpcID
            });
        }
        int len2 = shopItemTable.Count;
        for (int j = 0; j < len2; j++)
        {
            var shopItem = shopItemTable[j];
            shopAllData[shopItem.ShopID].items.Add(new ShopItemData(shopItem.ItemID, shopItem.Type));
        }
    }
    public ShopData GetShopData(int id)
    {
        return shopAllData[id];
    }
    #endregion

    public void LoadPlaceManager()
    {
        LoadCityData();
        LoadShopData();
    }
}
