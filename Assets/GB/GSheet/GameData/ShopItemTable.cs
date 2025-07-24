using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class ShopItemTable  : GameData
{	
	 [JsonProperty] public ShopItemTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, ShopItemTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <ShopItemTable> (json);
        ShopItemTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, ShopItemTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].ShopID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "ShopID": return true;
				case "ItemID": return true;
				case "Type": return true;
				case "Note": return true;

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
            ShopItemTableProb data = this[row];
            switch (col)
            {
				case "ShopID": return data.ShopID;
				case "ItemID": return data.ItemID;
				case "Type": return data.Type;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             ShopItemTableProb data = this[row];
            switch (col)
            {
				case "ShopID": return data.ShopID;
				case "ItemID": return data.ItemID;
				case "Type": return data.Type;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            ShopItemTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.ShopID;
				case 1: return data.ItemID;
				case 2: return data.Type;
				case 3: return data.Note;

                default: return null;
            }
        }
    }

    public ShopItemTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public ShopItemTableProb this[int index]
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
public class ShopItemTableProb : GameDataProb
{
		[JsonProperty] public readonly int ShopID;
	[JsonProperty] public readonly int ItemID;
	[JsonProperty] public readonly int Type;
	[JsonProperty] public readonly string Note;

}
