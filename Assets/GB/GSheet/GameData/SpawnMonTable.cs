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
				case "MonList": return true;

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
				case "MonList": return data.MonList;


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
				case "MonList": return data.MonList;


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
				case 2: return data.MonList;

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
	[JsonProperty] public readonly string MonList;

}
