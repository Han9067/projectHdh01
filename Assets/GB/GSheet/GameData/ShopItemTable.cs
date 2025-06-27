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
            dic[Datas[i].ID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "ID": return true;
				case "Type": return true;
				case "Name": return true;
				case "Gender": return true;
				case "Age": return true;
				case "CItyID": return true;
				case "KR": return true;
				case "EN": return true;
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
				case "ID": return data.ID;
				case "Type": return data.Type;
				case "Name": return data.Name;
				case "Gender": return data.Gender;
				case "Age": return data.Age;
				case "CItyID": return data.CItyID;
				case "KR": return data.KR;
				case "EN": return data.EN;
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
				case "ID": return data.ID;
				case "Type": return data.Type;
				case "Name": return data.Name;
				case "Gender": return data.Gender;
				case "Age": return data.Age;
				case "CItyID": return data.CItyID;
				case "KR": return data.KR;
				case "EN": return data.EN;
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
				case 0: return data.ID;
				case 1: return data.Type;
				case 2: return data.Name;
				case 3: return data.Gender;
				case 4: return data.Age;
				case 5: return data.CItyID;
				case 6: return data.KR;
				case 7: return data.EN;
				case 8: return data.Note;

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
		[JsonProperty] public readonly string ID;
	[JsonProperty] public readonly int Type;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly int Gender;
	[JsonProperty] public readonly int Age;
	[JsonProperty] public readonly string CItyID;
	[JsonProperty] public readonly string KR;
	[JsonProperty] public readonly string EN;
	[JsonProperty] public readonly string Note;

}
