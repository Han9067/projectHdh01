using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class QuestTable  : GameData
{	
	 [JsonProperty] public QuestTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, QuestTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <QuestTable> (json);
        QuestTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, QuestTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].QuestID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "QuestID": return true;
				case "Name": return true;
				case "Type": return true;
				case "Days": return true;
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
            QuestTableProb data = this[row];
            switch (col)
            {
				case "QuestID": return data.QuestID;
				case "Name": return data.Name;
				case "Type": return data.Type;
				case "Days": return data.Days;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


    public object this[string row, string col]
    {
        get
        {
             QuestTableProb data = this[row];
            switch (col)
            {
				case "QuestID": return data.QuestID;
				case "Name": return data.Name;
				case "Type": return data.Type;
				case "Days": return data.Days;
				case "Note": return data.Note;


                default: return null;
            }
        }
    }


	 public object this[int row, int col]
    {
        get
        {
            QuestTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.QuestID;
				case 1: return data.Name;
				case 2: return data.Type;
				case 3: return data.Days;
				case 4: return data.Note;

                default: return null;
            }
        }
    }

    public QuestTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public QuestTableProb this[int index]
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
public class QuestTableProb : GameDataProb
{
		[JsonProperty] public readonly int QuestID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly int Type;
	[JsonProperty] public readonly int Days;
	[JsonProperty] public readonly string Note;

}
