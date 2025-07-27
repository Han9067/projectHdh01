using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class AbilityTable  : GameData
{	
	 [JsonProperty] public AbilityTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, AbilityTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <AbilityTable> (json);
        AbilityTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, AbilityTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].AbID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "AbID": return true;
				case "Name": return true;
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
            AbilityTableProb data = this[row];
            switch (col)
            {
				case "AbID": return data.AbID;
				case "Name": return data.Name;
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
             AbilityTableProb data = this[row];
            switch (col)
            {
				case "AbID": return data.AbID;
				case "Name": return data.Name;
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
            AbilityTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.AbID;
				case 1: return data.Name;
				case 2: return data.KR;
				case 3: return data.EN;

                default: return null;
            }
        }
    }

    public AbilityTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public AbilityTableProb this[int index]
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
public class AbilityTableProb : GameDataProb
{
		[JsonProperty] public readonly int AbID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly string KR;
	[JsonProperty] public readonly string EN;

}
