using GB;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MonManager : AutoSingleton<MonManager>
{
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
        foreach (var mon in MonTable.Datas)
        {
            string[] stat = mon.Stat.Split('_');
            int id = mon.MonID;
            MonData mData = CreateMonData(id, mon.Type, mon.Name,
            int.Parse(stat[0]), int.Parse(stat[1]), int.Parse(stat[2]), int.Parse(stat[3]), int.Parse(stat[4]),
            int.Parse(stat[5]), int.Parse(stat[6]), int.Parse(stat[7]),
            mon.W, mon.H, mon.OffX, mon.OffY, mon.SdwScr, mon.SdwY);
            MonDataList[id] = mData;
        }
    }
    //VIT_END_STR_AGI_FOR_INT_CHA_LUK
    private MonData CreateMonData(int id, int monType, string name,
    int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK,
    float w, float h, float offX, float offY, float sdwScr, float sdwY)
    {
        return new MonData
        {
            MonId = id,
            MonType = monType,
            Name = name,
            VIT = VIT,
            END = END,
            STR = STR,
            AGI = AGI,
            FOR = FOR,
            INT = INT,
            CHA = CHA,
            LUK = LUK,
            W = w,
            H = h,
            OffX = offX,
            OffY = offY,
            SdwScr = sdwScr,
            SdwY = sdwY,
            Lv = LevelData.I.GetLv(VIT, END, STR, AGI, FOR, INT, CHA, LUK)
        };
    }

    public string GetAroundMon(float x, float y, int n, List<int> grp)
    {
        BattleMonList.Clear();
        string str = "";
        foreach (var mon in grp)
        {
            BattleMonList.Add(mon);
            str += mon + "_";
        }
        GameObject[] allMon = GameObject.FindGameObjectsWithTag("Monster");
        for (int i = 0; i < allMon.Length; i++)
        {
            if (n == i) continue;
            if (Vector2.Distance(allMon[i].transform.position, new Vector2(x, y)) < 10f)
            {
                wMon mon = allMon[i].GetComponent<wMon>();
                foreach (var m in mon.monGrp)
                {
                    BattleMonList.Add(m);
                    str += m + "_";
                }
            }
        }
        return str;
    }

    public void TestCreateMon()
    {
        BattleMonList.Clear();
        BattleMonList.Add(1);
        BattleMonList.Add(1);
        // BattleMonList.Add(1);
        // BattleMonList.Add(1);
    }
}
