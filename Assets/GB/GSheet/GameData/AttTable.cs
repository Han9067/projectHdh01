using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class AttTable  : GameData
{	
	 [JsonProperty] public AttTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, AttTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <AttTable> (json);
        AttTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, AttTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].AttID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "AttID": return true;
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
            AttTableProb data = this[row];
            switch (col)
            {
				case "AttID": return data.AttID;
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
             AttTableProb data = this[row];
            switch (col)
            {
				case "AttID": return data.AttID;
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
            AttTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.AttID;
				case 1: return data.Name;
				case 2: return data.Note;

                default: return null;
            }
        }
    }

    public AttTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public AttTableProb this[int index]
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
public class AttTableProb : GameDataProb
{
		[JsonProperty] public readonly int AttID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly string Note;

}
