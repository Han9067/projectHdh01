using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class ItemTable  : GameData
{	
	 [JsonProperty] public ItemTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, ItemTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <ItemTable> (json);
        ItemTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, ItemTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].ItemID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "ItemID": return true;
				case "Name": return true;
				case "Type": return true;
				case "Grade": return true;
				case "Price": return true;
				case "AttKey": return true;
				case "AttVal": return true;
				case "W": return true;
				case "H": return true;
				case "Res": return true;

		  default: return false;

        }
    }

	public override double GetNumber(int row, string col)
    {
        return double.Parse(this[row, col].ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }

    public override double GetNumber(string row, string col)
    {
        return double.Parse(this[row, col].ToString(), System.Globalization.CultureInfo.InvariantCulture);
    }


	public object this[int row, string col]
    {
        get
        {
            ItemTableProb data = this[row];
            switch (col)
            {
				case "ItemID": return data.ItemID;
				case "Name": return data.Name;
				case "Type": return data.Type;
				case "Grade": return data.Grade;
				case "Price": return data.Price;
				case "AttKey": return data.AttKey;
				case "AttVal": return data.AttVal;
				case "W": return data.W;
				case "H": return data.H;
				case "Res": return data.Res;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             ItemTableProb data = this[row];
            switch (col)
            {
				case "ItemID": return data.ItemID;
				case "Name": return data.Name;
				case "Type": return data.Type;
				case "Grade": return data.Grade;
				case "Price": return data.Price;
				case "AttKey": return data.AttKey;
				case "AttVal": return data.AttVal;
				case "W": return data.W;
				case "H": return data.H;
				case "Res": return data.Res;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            ItemTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.ItemID;
				case 1: return data.Name;
				case 2: return data.Type;
				case 3: return data.Grade;
				case 4: return data.Price;
				case 5: return data.AttKey;
				case 6: return data.AttVal;
				case 7: return data.W;
				case 8: return data.H;
				case 9: return data.Res;

                default: return null;
            }
        }
    }

    public ItemTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public ItemTableProb this[int index]
    {
        get
        {
            return Datas[index];
        }
    }

    public bool ContainsKey(string name)
    {
        return _DicDatas.ContainsKey(name);
    }



    public int Count
    {
        get
        {
            return Datas.Length;
        }
    }
}

[Serializable]
public class ItemTableProb : GameDataProb
{
		[JsonProperty] public readonly int ItemID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly int Type;
	[JsonProperty] public readonly int Grade;
	[JsonProperty] public readonly int Price;
	[JsonProperty] public readonly string AttKey;
	[JsonProperty] public readonly string AttVal;
	[JsonProperty] public readonly int W;
	[JsonProperty] public readonly int H;
	[JsonProperty] public readonly string Res;

}
