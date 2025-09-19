using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class MonGrpTable  : GameData
{	
	 [JsonProperty] public MonGrpTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, MonGrpTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <MonGrpTable> (json);
        MonGrpTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, MonGrpTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].GrpID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "GrpID": return true;
				case "Grade": return true;
				case "Type": return true;
				case "Min": return true;
				case "Max": return true;
				case "LeaderID": return true;
				case "List": return true;

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
            MonGrpTableProb data = this[row];
            switch (col)
            {
				case "GrpID": return data.GrpID;
				case "Grade": return data.Grade;
				case "Type": return data.Type;
				case "Min": return data.Min;
				case "Max": return data.Max;
				case "LeaderID": return data.LeaderID;
				case "List": return data.List;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             MonGrpTableProb data = this[row];
            switch (col)
            {
				case "GrpID": return data.GrpID;
				case "Grade": return data.Grade;
				case "Type": return data.Type;
				case "Min": return data.Min;
				case "Max": return data.Max;
				case "LeaderID": return data.LeaderID;
				case "List": return data.List;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            MonGrpTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.GrpID;
				case 1: return data.Grade;
				case 2: return data.Type;
				case 3: return data.Min;
				case 4: return data.Max;
				case 5: return data.LeaderID;
				case 6: return data.List;

                default: return null;
            }
        }
    }

    public MonGrpTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public MonGrpTableProb this[int index]
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
public class MonGrpTableProb : GameDataProb
{
		[JsonProperty] public readonly int GrpID;
	[JsonProperty] public readonly int Grade;
	[JsonProperty] public readonly int Type;
	[JsonProperty] public readonly int Min;
	[JsonProperty] public readonly int Max;
	[JsonProperty] public readonly int LeaderID;
	[JsonProperty] public readonly string List;

}
