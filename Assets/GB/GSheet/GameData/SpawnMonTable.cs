using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class SpawnMonTable  : GameData
{	
	 [JsonProperty] public SpawnMonTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, SpawnMonTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <SpawnMonTable> (json);
        SpawnMonTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, SpawnMonTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].AreaID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "AreaID": return true;
				case "Cnt": return true;
				case "MG10": return true;
				case "MG9": return true;
				case "MG8": return true;
				case "MG7": return true;
				case "MG6": return true;
				case "MG5": return true;
				case "MG4": return true;

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
            SpawnMonTableProb data = this[row];
            switch (col)
            {
				case "AreaID": return data.AreaID;
				case "Cnt": return data.Cnt;
				case "MG10": return data.MG10;
				case "MG9": return data.MG9;
				case "MG8": return data.MG8;
				case "MG7": return data.MG7;
				case "MG6": return data.MG6;
				case "MG5": return data.MG5;
				case "MG4": return data.MG4;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             SpawnMonTableProb data = this[row];
            switch (col)
            {
				case "AreaID": return data.AreaID;
				case "Cnt": return data.Cnt;
				case "MG10": return data.MG10;
				case "MG9": return data.MG9;
				case "MG8": return data.MG8;
				case "MG7": return data.MG7;
				case "MG6": return data.MG6;
				case "MG5": return data.MG5;
				case "MG4": return data.MG4;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            SpawnMonTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.AreaID;
				case 1: return data.Cnt;
				case 2: return data.MG10;
				case 3: return data.MG9;
				case 4: return data.MG8;
				case 5: return data.MG7;
				case 6: return data.MG6;
				case 7: return data.MG5;
				case 8: return data.MG4;

                default: return null;
            }
        }
    }

    public SpawnMonTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public SpawnMonTableProb this[int index]
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
public class SpawnMonTableProb : GameDataProb
{
		[JsonProperty] public readonly int AreaID;
	[JsonProperty] public readonly int Cnt;
	[JsonProperty] public readonly string MG10;
	[JsonProperty] public readonly string MG9;
	[JsonProperty] public readonly string MG8;
	[JsonProperty] public readonly string MG7;
	[JsonProperty] public readonly string MG6;
	[JsonProperty] public readonly string MG5;
	[JsonProperty] public readonly string MG4;

}
