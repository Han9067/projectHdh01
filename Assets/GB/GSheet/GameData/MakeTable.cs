using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class MakeTable  : GameData
{	
	 [JsonProperty] public MakeTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, MakeTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <MakeTable> (json);
        MakeTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, MakeTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].MakeId.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "MakeId": return true;
				case "ItemId": return true;
				case "Recipe": return true;
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
            MakeTableProb data = this[row];
            switch (col)
            {
				case "MakeId": return data.MakeId;
				case "ItemId": return data.ItemId;
				case "Recipe": return data.Recipe;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             MakeTableProb data = this[row];
            switch (col)
            {
				case "MakeId": return data.MakeId;
				case "ItemId": return data.ItemId;
				case "Recipe": return data.Recipe;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            MakeTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.MakeId;
				case 1: return data.ItemId;
				case 2: return data.Recipe;
				case 3: return data.Note;

                default: return null;
            }
        }
    }

    public MakeTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public MakeTableProb this[int index]
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
public class MakeTableProb : GameDataProb
{
		[JsonProperty] public readonly int MakeId;
	[JsonProperty] public readonly int ItemId;
	[JsonProperty] public readonly string Recipe;
	[JsonProperty] public readonly string Note;

}
