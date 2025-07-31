using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class MonTable  : GameData
{	
	 [JsonProperty] public MonTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, MonTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <MonTable> (json);
        MonTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, MonTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].MonID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "MonID": return true;
				case "Name": return true;
				case "Stat": return true;
				case "KR": return true;
				case "EN": return true;

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
            MonTableProb data = this[row];
            switch (col)
            {
				case "MonID": return data.MonID;
				case "Name": return data.Name;
				case "Stat": return data.Stat;
				case "KR": return data.KR;
				case "EN": return data.EN;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             MonTableProb data = this[row];
            switch (col)
            {
				case "MonID": return data.MonID;
				case "Name": return data.Name;
				case "Stat": return data.Stat;
				case "KR": return data.KR;
				case "EN": return data.EN;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            MonTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.MonID;
				case 1: return data.Name;
				case 2: return data.Stat;
				case 3: return data.KR;
				case 4: return data.EN;

                default: return null;
            }
        }
    }

    public MonTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public MonTableProb this[int index]
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
public class MonTableProb : GameDataProb
{
		[JsonProperty] public readonly int MonID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly string Stat;
	[JsonProperty] public readonly string KR;
	[JsonProperty] public readonly string EN;

}
