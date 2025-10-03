using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class SkTable  : GameData
{	
	 [JsonProperty] public SkTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, SkTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <SkTable> (json);
        SkTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, SkTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].SkID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "SkID": return true;
				case "Name": return true;
				case "Type": return true;
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
            SkTableProb data = this[row];
            switch (col)
            {
				case "SkID": return data.SkID;
				case "Name": return data.Name;
				case "Type": return data.Type;
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
             SkTableProb data = this[row];
            switch (col)
            {
				case "SkID": return data.SkID;
				case "Name": return data.Name;
				case "Type": return data.Type;
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
            SkTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.SkID;
				case 1: return data.Name;
				case 2: return data.Type;
				case 3: return data.EN;
				case 4: return data.Note;

                default: return null;
            }
        }
    }

    public SkTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public SkTableProb this[int index]
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
public class SkTableProb : GameDataProb
{
		[JsonProperty] public readonly int SkID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly int Type;
	[JsonProperty] public readonly string EN;
	[JsonProperty] public readonly string Note;

}
