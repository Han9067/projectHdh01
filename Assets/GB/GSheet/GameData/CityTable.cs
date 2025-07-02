using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class CityTable  : GameData
{	
	 [JsonProperty] public CityTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, CityTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <CityTable> (json);
        CityTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, CityTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].ID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "ID": return true;
				case "Name": return true;
				case "Place": return true;
				case "Res": return true;
				case "KR": return true;
				case "EN": return true;
				case "Desc": return true;
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
            CityTableProb data = this[row];
            switch (col)
            {
				case "ID": return data.ID;
				case "Name": return data.Name;
				case "Place": return data.Place;
				case "Res": return data.Res;
				case "KR": return data.KR;
				case "EN": return data.EN;
				case "Desc": return data.Desc;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             CityTableProb data = this[row];
            switch (col)
            {
				case "ID": return data.ID;
				case "Name": return data.Name;
				case "Place": return data.Place;
				case "Res": return data.Res;
				case "KR": return data.KR;
				case "EN": return data.EN;
				case "Desc": return data.Desc;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            CityTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.ID;
				case 1: return data.Name;
				case 2: return data.Place;
				case 3: return data.Res;
				case 4: return data.KR;
				case 5: return data.EN;
				case 6: return data.Desc;
				case 7: return data.Note;

                default: return null;
            }
        }
    }

    public CityTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public CityTableProb this[int index]
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
public class CityTableProb : GameDataProb
{
		[JsonProperty] public readonly int ID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly string Place;
	[JsonProperty] public readonly string Res;
	[JsonProperty] public readonly string KR;
	[JsonProperty] public readonly  string EN;
	[JsonProperty] public readonly string Desc;
	[JsonProperty] public readonly string Note;

}
