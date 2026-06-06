using System.Collections.Generic;
using System;
using Newtonsoft.Json;


[Serializable]
public class EventTable  : GameData
{	
	 [JsonProperty] public EventTableProb[] Datas{get; private set;}
	 IReadOnlyDictionary<string, EventTableProb> _DicDatas;

	public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject <EventTable> (json);
        EventTableProb[] arr = data.Datas;
        Datas = arr;

		var dic = new Dictionary<string, EventTableProb>();

        for (int i = 0; i < Datas.Length; ++i)
            dic[Datas[i].EventID.ToString()] = Datas[i];

        _DicDatas = dic;

    }

	public bool ContainsColumnKey(string name)
    {
        switch (name)
        {
				case "EventID": return true;
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
            EventTableProb data = this[row];
            switch (col)
            {
				case "EventID": return data.EventID;
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
             EventTableProb data = this[row];
            switch (col)
            {
				case "EventID": return data.EventID;
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
            EventTableProb data = Datas[row];

            switch (col)
            {
				case 0: return data.EventID;
				case 1: return data.Name;
				case 2: return data.Note;

                default: return null;
            }
        }
    }

    public EventTableProb this[string name]
    {
        get
        {
            return _DicDatas[name];
        }
    }


    public EventTableProb this[int index]
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
public class EventTableProb : GameDataProb
{
		[JsonProperty] public readonly int EventID;
	[JsonProperty] public readonly string Name;
	[JsonProperty] public readonly string Note;

}
