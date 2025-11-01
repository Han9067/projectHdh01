using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class WpTable  : GameData
{	
	 [JsonProperty] public WpTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, WpTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <WpTable> (json);
        WpTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, WpTableProb>();

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
				case "Price": return true;
				case "Val": return true;
				case "W": return true;
				case "H": return true;
				case "Dur": return true;
				case "Both": return true;
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
            WpTableProb data = this[row];
            switch (col)
            {
				case "ItemID": return data.ItemID;
				case "Name": return data.Name;
				case "Type": return data.Type;
				case "Price": return data.Price;
				case "Val": return data.Val;
				case "W": return data.W;
				case "H": return data.H;
				case "Dur": return data.Dur;
				case "Both": return data.Both;
				case "Res": return data.Res;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             WpTableProb data = this[row];
            switch (col)
            {
				case "ItemID": return data.ItemID;
				case "Name": return data.Name;
				case "Type": return data.Type;
				case "Price": return data.Price;
				case "Val": return data.Val;
				case "W": return data.W;
				case "H": return data.H;
				case "Dur": return data.Dur;
				case "Both": return data.Both;
				case "Res": return data.Res;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            WpTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.ItemID;
				case 1: return data.Name;
				case 2: return data.Type;
				case 3: return data.Price;
				case 4: return data.Val;
				case 5: return data.W;
				case 6: return data.H;
				case 7: return data.Dur;
				case 8: return data.Both;
				case 9: return data.Res;

                default: return null;
            }
        }
    }

    public WpTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public WpTableProb this[int index]
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
public class WpTableProb : GameDataProb
{
		[JsonProperty] public readonly int ItemID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly int Type;
	[JsonProperty] public readonly int Price;
	[JsonProperty] public readonly int Val;
	[JsonProperty] public readonly int W;
	[JsonProperty] public readonly int H;
	[JsonProperty] public readonly int Dur;
	[JsonProperty] public readonly int Both;
	[JsonProperty] public readonly string Res;

}
