using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class NpcTable  : GameData
{	
	 [JsonProperty] public NpcTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, NpcTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <NpcTable> (json);
        NpcTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, NpcTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].NpcID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "NpcID": return true;
				case "Name": return true;
				case "Gen": return true;
				case "Age": return true;
				case "Fame": return true;
				case "Personality": return true;
				case "Lv": return true;
				case "Parts": return true;
				case "Stat": return true;
				case "Eq": return true;
				case "Wp": return true;
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
            NpcTableProb data = this[row];
            switch (col)
            {
				case "NpcID": return data.NpcID;
				case "Name": return data.Name;
				case "Gen": return data.Gen;
				case "Age": return data.Age;
				case "Fame": return data.Fame;
				case "Personality": return data.Personality;
				case "Lv": return data.Lv;
				case "Parts": return data.Parts;
				case "Stat": return data.Stat;
				case "Eq": return data.Eq;
				case "Wp": return data.Wp;
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
             NpcTableProb data = this[row];
            switch (col)
            {
				case "NpcID": return data.NpcID;
				case "Name": return data.Name;
				case "Gen": return data.Gen;
				case "Age": return data.Age;
				case "Fame": return data.Fame;
				case "Personality": return data.Personality;
				case "Lv": return data.Lv;
				case "Parts": return data.Parts;
				case "Stat": return data.Stat;
				case "Eq": return data.Eq;
				case "Wp": return data.Wp;
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
            NpcTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.NpcID;
				case 1: return data.Name;
				case 2: return data.Gen;
				case 3: return data.Age;
				case 4: return data.Fame;
				case 5: return data.Personality;
				case 6: return data.Lv;
				case 7: return data.Parts;
				case 8: return data.Stat;
				case 9: return data.Eq;
				case 10: return data.Wp;
				case 11: return data.KR;
				case 12: return data.EN;

                default: return null;
            }
        }
    }

    public NpcTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public NpcTableProb this[int index]
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
public class NpcTableProb : GameDataProb
{
		[JsonProperty] public readonly int NpcID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly int Gen;
	[JsonProperty] public readonly int Age;
	[JsonProperty] public readonly int Fame;
	[JsonProperty] public readonly int Personality;
	[JsonProperty] public readonly int Lv;
	[JsonProperty] public readonly string Parts;
	[JsonProperty] public readonly string Stat;
	[JsonProperty] public readonly string Eq;
	[JsonProperty] public readonly string Wp;
	[JsonProperty] public readonly string KR;
	[JsonProperty] public readonly string EN;

}
