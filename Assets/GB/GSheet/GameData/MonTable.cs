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
				case "Type": return true;
				case "Stat": return true;
				case "W": return true;
				case "H": return true;
				case "SdwScr": return true;
				case "GgY": return true;
				case "Drop": return true;
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
            MonTableProb data = this[row];
            switch (col)
            {
				case "MonID": return data.MonID;
				case "Name": return data.Name;
				case "Type": return data.Type;
				case "Stat": return data.Stat;
				case "W": return data.W;
				case "H": return data.H;
				case "SdwScr": return data.SdwScr;
				case "GgY": return data.GgY;
				case "Drop": return data.Drop;
				case "Note": return data.Note;


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
				case "Type": return data.Type;
				case "Stat": return data.Stat;
				case "W": return data.W;
				case "H": return data.H;
				case "SdwScr": return data.SdwScr;
				case "GgY": return data.GgY;
				case "Drop": return data.Drop;
				case "Note": return data.Note;


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
				case 2: return data.Type;
				case 3: return data.Stat;
				case 4: return data.W;
				case 5: return data.H;
				case 6: return data.SdwScr;
				case 7: return data.GgY;
				case 8: return data.Drop;
				case 9: return data.Note;

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
	[JsonProperty] public readonly int Type;
	[JsonProperty] public readonly string Stat;
	[JsonProperty] public readonly int W;
	[JsonProperty] public readonly int H;
	[JsonProperty] public readonly float SdwScr;
	[JsonProperty] public readonly float GgY;
	[JsonProperty] public readonly string Drop;
	[JsonProperty] public readonly string Note;

}
