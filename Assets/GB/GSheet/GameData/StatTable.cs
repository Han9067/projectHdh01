using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class StatTable  : GameData
{	
	 [JsonProperty] public StatTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, StatTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <StatTable> (json);
        StatTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, StatTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].StatID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "StatID": return true;
				case "Name": return true;
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
            StatTableProb data = this[row];
            switch (col)
            {
				case "StatID": return data.StatID;
				case "Name": return data.Name;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             StatTableProb data = this[row];
            switch (col)
            {
				case "StatID": return data.StatID;
				case "Name": return data.Name;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            StatTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.StatID;
				case 1: return data.Name;
				case 2: return data.Note;

                default: return null;
            }
        }
    }

    public StatTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public StatTableProb this[int index]
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
public class StatTableProb : GameDataProb
{
		[JsonProperty] public readonly int StatID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly string Note;

}
