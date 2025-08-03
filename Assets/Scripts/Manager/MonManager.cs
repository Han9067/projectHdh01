using GB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class MonManager : AutoSingleton<MonManager>
{
    // class MonGrpInfo
    // {
    //     public int MonId;
    //     public List<int> Party;
    //     public float x, y;
    // }
    private MonTable _monTable;
    public MonTable MonTable => _monTable ?? (_monTable = GameDataManager.GetTable<MonTable>());
    public Dictionary<int, MonData> MonDataList = new Dictionary<int, MonData>();
    public List<int> BattleMonList = new List<int>();
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
            MonData mData = CreateMonData(id, mon.Name, 
            int.Parse(stat[0]), int.Parse(stat[1]), int.Parse(stat[2]), int.Parse(stat[3]), int.Parse(stat[4]), int.Parse(stat[5]), int.Parse(stat[6]), int.Parse(stat[7]));
            
            mData.Lv = 1; mData.Exp = 0; mData.NextExp = 100;
            mData.HP = mData.VIT * 10; mData.MaxHP = mData.HP;
            mData.MP = mData.INT * 10; mData.MaxMP = mData.MP;
            mData.SP = mData.END * 10; mData.MaxSP = mData.SP;

            mData.Att = mData.STR * 2;
            mData.Def = mData.VIT;
            mData.Crt = 50 + (mData.LUK * 2);
            mData.CrtRate = mData.LUK;
            int agi = mData.AGI / 4; //mData.AGI / 4 * 2
            mData.Hit = 60 + agi;
            mData.Eva = 10 + agi;

            MonDataList[id] = mData;
        }
    }
    //VIT_END_STR_AGI_FOR_INT_CHA_LUK
    private MonData CreateMonData(int id, string name, int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK)
    {
        return new MonData { MonId = id, Name = name, VIT = VIT, END = END, STR = STR, AGI = AGI, FOR = FOR, INT = INT, CHA = CHA, LUK = LUK };
    }

    public string GetAroundMon(float x, float y, int n, List<int> grp)
    {
        BattleMonList.Clear();
        string str = "";
        foreach(var mon in grp)
        {
            BattleMonList.Add(mon);
            str += mon + "_";
        }
        GameObject[] allMon = GameObject.FindGameObjectsWithTag("Monster");
        for(int i = 0; i < allMon.Length; i++)
        {
            if(n == i)continue;
            if(Vector2.Distance(allMon[i].transform.position, new Vector2(x, y)) < 10f)
            {
                wMon mon = allMon[i].GetComponent<wMon>();
                foreach(var m in mon.monGrp)
                {
                    BattleMonList.Add(m);
                    str += m + "_";
                }
            }
        }
        return str;
    }
}
