using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class ExpTable  : GameData
{	
	 [JsonProperty] public ExpTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, ExpTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <ExpTable> (json);
        ExpTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, ExpTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].Lv.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "Lv": return true;
				case "Exp": return true;

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
            ExpTableProb data = this[row];
            switch (col)
            {
				case "Lv": return data.Lv;
				case "Exp": return data.Exp;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             ExpTableProb data = this[row];
            switch (col)
            {
				case "Lv": return data.Lv;
				case "Exp": return data.Exp;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            ExpTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.Lv;
				case 1: return data.Exp;

                default: return null;
            }
        }
    }

    public ExpTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public ExpTableProb this[int index]
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
public class ExpTableProb : GameDataProb
{
		[JsonProperty] public readonly int Lv;
	[JsonProperty] public readonly string Exp;

}
