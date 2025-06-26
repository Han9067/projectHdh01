using System.Collections.Generic;
using UnityEngine;
using GB;
using System.Data;
using Newtonsoft.Json;
using System;

public class GameDataManager : AutoSingleton<GameDataManager>
{
 
    private void Awake()
    {
        if(I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(this.gameObject);
    }

    Dictionary<TABLE, GameData> _Tables = new Dictionary<TABLE, GameData>();

	public static bool ContainsTable(TABLE tableID)
    {
        return I._Tables.ContainsKey(tableID);
    }

    public static void LocalAllLoad(Action complete)
    {
        var gbCoroutine  = GBCoroutine.Create(I);

        string CityTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/CityTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<CityTable>(CityTabledata,(result)=>{ I._Tables[TABLE.CityTable] = result;}));
                string EqTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/EqTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<EqTable>(EqTabledata,(result)=>{ I._Tables[TABLE.EqTable] = result;}));
                string WpTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/WpTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<WpTable>(WpTabledata,(result)=>{ I._Tables[TABLE.WpTable] = result;}));
                string ItemTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/ItemTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<ItemTable>(ItemTabledata,(result)=>{ I._Tables[TABLE.ItemTable] = result;}));
                

        gbCoroutine.OnComplete(()=>{complete?.Invoke();}).Play();
    }

    public static void SetJson<T>(string json,Action complete) where T : GameData
    {
        var gbCoroutine  = GBCoroutine.Create(I);
        gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<T>(json,
        (result)=>
        {
             var tableID = I.StringToEnum(typeof(T).Name);
             I._Tables[tableID] = result;
        }))
        .OnComplete(()=>{complete?.Invoke();})
        .Play();
    }

    public static T GetTable<T>() where T : GameData
    {
        var tableID = I.StringToEnum(typeof(T).Name);
        if (I._Tables.ContainsKey(tableID) == false)
        {
            TextAsset assets = UnityEngine.Resources.Load<TextAsset>("Json/" + tableID.ToString());

            string data = Gzip.DeCompression(assets.text);
            TABLE type = I.StringToEnum(assets.name);
            I._Tables[type] = (GameData)I.GetJsonData(type, data);
        }
         return (T)I._Tables[tableID];
    }


    public static GameData GetTable(TABLE tableID)
    {
        if (I._Tables.ContainsKey(tableID) == false)
        {
            TextAsset assets = UnityEngine.Resources.Load<TextAsset>("Json/" + tableID.ToString());

            string data = GB.Gzip.DeCompression(assets.text);
            TABLE type = I.StringToEnum(assets.name);
            I._Tables[type] = (GameData)I.GetJsonData(type, data);
        }

        if (I._Tables.ContainsKey(tableID) == false) return null;
        return I._Tables[tableID];
    }


    private TABLE StringToEnum(string e)
    {
        return (TABLE)System.Enum.Parse(typeof(TABLE), e);
    }
    private object GetJsonData(TABLE table,string data)
    {
        object obj = null;

        switch (table)
        {

case TABLE.CityTable:
        CityTable d_CityTable = new CityTable();
        d_CityTable.SetJson(data);
        obj  = d_CityTable;
        break;
case TABLE.EqTable:
        EqTable d_EqTable = new EqTable();
        d_EqTable.SetJson(data);
        obj  = d_EqTable;
        break;
case TABLE.WpTable:
        WpTable d_WpTable = new WpTable();
        d_WpTable.SetJson(data);
        obj  = d_WpTable;
        break;
case TABLE.ItemTable:
        ItemTable d_ItemTable = new ItemTable();
        d_ItemTable.SetJson(data);
        obj  = d_ItemTable;
        break;
        }

        return obj;
    }
    
    public void SetTable(TABLE tableId, GameData data)
    {
        _Tables[tableId] = data;
    }

	public static double GetNumber(TABLE tableId, string rowKey, string colKey)
    {
        var table = GetTable(tableId);

        return table.GetNumber(rowKey, colKey);
    }

    public static double GetNumber(TABLE tableId, int rowKey, string colKey)
    {
        var table = GetTable(tableId);

        return table.GetNumber(rowKey, colKey);
    }

    public static double Operation(string operation, double param1)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(string.Format(operation, param1), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double Operation(string operation, double param1, double param2)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(string.Format(operation, param1, param2), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double Operation(string operation, double param1, double param2, double param3)
    {
        DataTable dt = new DataTable();
        var val =  dt.Compute(string.Format(operation, param1, param2, param3), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture); 
    }

    public static double Operation(string operation, double param1, double param2, double param3, double param4)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(string.Format(operation, param1, param2, param3,param4), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double Operation(string operation, double param1, double param2, double param3, double param4, double param5)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(string.Format(operation, param1, param2, param3, param4,param5), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double Operation(string operation, double param1, double param2, double param3, double param4, double param5, double param6)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(string.Format(operation, param1, param2, param3, param4, param5,param6), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }
    public static double Operation(string operation, double param1, double param2, double param3, double param4, double param5, double param6, double param7)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(string.Format(operation, param1, param2, param3, param4, param5, param6, param7), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double Operation(string operation, double param1, double param2, double param3, double param4, double param5, double param6, double param7, double param8)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(string.Format(operation, param1, param2, param3, param4, param5, param6, param7,param8), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double Operation(string operation, double param1, double param2, double param3, double param4, double param5, double param6, double param7, double param8, double param9)
    {
        DataTable dt = new DataTable();
        var val = dt.Compute(string.Format(operation, param1, param2, param3, param4, param5, param6, param7, param8,param9), string.Empty);
        return double.Parse(val.ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

}

public interface GameDataProb{}

public abstract class GameData
{
    public abstract double GetNumber(string row, string col);
    public abstract double GetNumber(int row, string col);

}

public enum TABLE 
{
	Empty,
	CityTable,
	EqTable,
	WpTable,
	ItemTable,

}
