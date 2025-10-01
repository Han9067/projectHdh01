using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class CityManager : AutoSingleton<CityManager>
{
    private CityTable _cityTable;
    public CityTable CityTable => _cityTable ?? (_cityTable = GameDataManager.GetTable<CityTable>());
    public Dictionary<int, CityData> CityDataList = new Dictionary<int, CityData>();
    private void Awake()
    {
        LoadCityData();
    }

    private void LoadCityData()
    {
        // foreach (var city in CityTable.Datas)
        //     CityDataList[city.CityID] = new CityData(city.CityID, city.Name, city.Place);
        for (int i = 0; i < 5; i++)
        {
            CityDataList[CityTable.Datas[i].CityID] = new CityData(CityTable.Datas[i].CityID, CityTable.Datas[i].Name, CityTable.Datas[i].Place, CityTable.Datas[i].Area);
        }
    }
    public string GetCityName(int cityID)
    {
        return LocalizationManager.GetValue(CityDataList[cityID].Name);
    }
}
