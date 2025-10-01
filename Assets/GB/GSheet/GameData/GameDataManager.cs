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
                string ShopTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/ShopTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<ShopTable>(ShopTabledata,(result)=>{ I._Tables[TABLE.ShopTable] = result;}));
                string ShopItemTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/ShopItemTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<ShopItemTable>(ShopItemTabledata,(result)=>{ I._Tables[TABLE.ShopItemTable] = result;}));
                string AbilityTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/AbilityTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<AbilityTable>(AbilityTabledata,(result)=>{ I._Tables[TABLE.AbilityTable] = result;}));
                string NpcTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/NpcTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<NpcTable>(NpcTabledata,(result)=>{ I._Tables[TABLE.NpcTable] = result;}));
                string MonTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/MonTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<MonTable>(MonTabledata,(result)=>{ I._Tables[TABLE.MonTable] = result;}));
                string SpawnMonTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/SpawnMonTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<SpawnMonTable>(SpawnMonTabledata,(result)=>{ I._Tables[TABLE.SpawnMonTable] = result;}));
                string MonGrpTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/MonGrpTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<MonGrpTable>(MonGrpTabledata,(result)=>{ I._Tables[TABLE.MonGrpTable] = result;}));
                string SkTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/SkTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<SkTable>(SkTabledata,(result)=>{ I._Tables[TABLE.SkTable] = result;}));
                string QuestTabledata = Gzip.DeCompression( Resources.Load<TextAsset>("Json/QuestTable").text);
            gbCoroutine.AddIEnumerator(JsonLoader.LoadDataCoroutine<QuestTable>(QuestTabledata,(result)=>{ I._Tables[TABLE.QuestTable] = result;}));
                

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
case TABLE.ShopTable:
        ShopTable d_ShopTable = new ShopTable();
        d_ShopTable.SetJson(data);
        obj  = d_ShopTable;
        break;
case TABLE.ShopItemTable:
        ShopItemTable d_ShopItemTable = new ShopItemTable();
        d_ShopItemTable.SetJson(data);
        obj  = d_ShopItemTable;
        break;
case TABLE.AbilityTable:
        AbilityTable d_AbilityTable = new AbilityTable();
        d_AbilityTable.SetJson(data);
        obj  = d_AbilityTable;
        break;
case TABLE.NpcTable:
        NpcTable d_NpcTable = new NpcTable();
        d_NpcTable.SetJson(data);
        obj  = d_NpcTable;
        break;
case TABLE.MonTable:
        MonTable d_MonTable = new MonTable();
        d_MonTable.SetJson(data);
        obj  = d_MonTable;
        break;
case TABLE.SpawnMonTable:
        SpawnMonTable d_SpawnMonTable = new SpawnMonTable();
        d_SpawnMonTable.SetJson(data);
        obj  = d_SpawnMonTable;
        break;
case TABLE.MonGrpTable:
        MonGrpTable d_MonGrpTable = new MonGrpTable();
        d_MonGrpTable.SetJson(data);
        obj  = d_MonGrpTable;
        break;
case TABLE.SkTable:
        SkTable d_SkTable = new SkTable();
        d_SkTable.SetJson(data);
        obj  = d_SkTable;
        break;
case TABLE.QuestTable:
        QuestTable d_QuestTable = new QuestTable();
        d_QuestTable.SetJson(data);
        obj  = d_QuestTable;
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
	ShopTable,
	ShopItemTable,
	AbilityTable,
	NpcTable,
	MonTable,
	SpawnMonTable,
	MonGrpTable,
	SkTable,
	QuestTable,

}
