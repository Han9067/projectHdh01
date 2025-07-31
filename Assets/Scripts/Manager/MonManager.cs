using GB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class MonManager : AutoSingleton<MonManager>
{
    private MonTable _monTable;
    public MonTable MonTable => _monTable ?? (_monTable = GameDataManager.GetTable<MonTable>());
    public Dictionary<int, MonData> MonDataList = new Dictionary<int, MonData>();
    public List<GameObject> MonGrpList = new List<GameObject>();
    private void Awake()
    {
        LoadMonData();
    }
    private void LoadMonData()
    {
        foreach(var mon in MonTable.Datas)
        {
            string[] stat = mon.Stat.Split('_');
            int id = mon.MonID;
            MonDataList[id] = CreateMonData(id, mon.Name, 
            int.Parse(stat[0]), int.Parse(stat[1]), int.Parse(stat[2]), int.Parse(stat[3]), int.Parse(stat[4]), int.Parse(stat[5]), int.Parse(stat[6]), int.Parse(stat[7]));
            MonDataList[id].Lv = 1;
            MonDataList[id].Exp = 0;
            MonDataList[id].NextExp = 100;
            MonDataList[id].HP = MonDataList[id].VIT * 10;
            MonDataList[id].MP = MonDataList[id].INT * 10;
            MonDataList[id].SP = MonDataList[id].END * 10;
            MonDataList[id].MaxHP = MonDataList[id].HP;
            MonDataList[id].MaxMP = MonDataList[id].MP;
            MonDataList[id].MaxSP = MonDataList[id].SP;

            MonDataList[id].Att = MonDataList[id].STR * 2;
            MonDataList[id].Def = MonDataList[id].VIT;
            MonDataList[id].Crt = 50 + (MonDataList[id].LUK * 2);
            MonDataList[id].CrtRate = MonDataList[id].LUK;
            MonDataList[id].Acc = 80 + MonDataList[id].AGI;
            MonDataList[id].Dod = 10 + MonDataList[id].AGI;
        }
    }
    //VIT_END_STR_AGI_FOR_INT_CHA_LUK
    private MonData CreateMonData(int id, string name, int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK)
    {
        return new MonData { MonId = id, Name = name, VIT = VIT, END = END, STR = STR, AGI = AGI, FOR = FOR, INT = INT, CHA = CHA, LUK = LUK };
    }

    public List<int> GetAroundMon(float x, float y, int n, List<int> grp)
    {
        List<int> monGrp = new List<int>();
        foreach(var mon in grp)
            monGrp.Add(mon);
        // float radius = 5f;
        for(int i = 0; i < MonGrpList.Count; i++)
        {
            if(n == i)continue;
            if(Vector2.Distance(MonGrpList[i].transform.position, new Vector2(x, y)) < 10f)
            {
                wMon mon = MonGrpList[i].GetComponent<wMon>();
                foreach(var m in mon.monGrp)
                {
                    monGrp.Add(m);
                }
            }
        }
        return monGrp;
    }
}
