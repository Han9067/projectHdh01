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
    public List<int> BattleMonGrpUid = new List<int>();
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
            mon.W, mon.H, mon.SdwScr, mon.GgY);
            // mData.GainExp = ObjLevelManager.I.GetGainExp(mData.MaxHP, mData.SP, mData.MP, mData.STR, mData.AGI, mData.INT, mData.CHA, mData.LUK);
            MonDataList[id] = mData;
        }
    }
    //VIT_END_STR_AGI_FOR_INT_CHA_LUK
    private MonData CreateMonData(int id, int monType, string name,
    int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK,
    int w, int h, float sdwScr, float ggY)
    {
        int hp = VIT * 4, sp = END * 4, mp = INT * 4;
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
            SdwScr = sdwScr,
            GgY = ggY,
            Lv = ObjLevelManager.I.GetLv(VIT, END, STR, AGI, FOR, INT, CHA, LUK),

            // HP/MP/SP 설정
            HP = hp,
            SP = sp,
            MP = mp,
            MaxHP = hp,
            MaxSP = sp,
            MaxMP = mp,

            // 전투 스탯 계산 및 설정 (추가 필요!)
            Att = STR * 2,
            Def = VIT,
            Crt = 50 + (LUK * 2),
            CrtRate = LUK + AGI,
            Hit = 60 + (AGI / 4),
            Eva = 10 + (AGI / 4),

            GainExp = ObjLevelManager.I.GetGainExp(hp, sp, mp, STR, AGI, INT, CHA, LUK)
        };
    }

    public string GetAroundMon(List<int> grp, int uid, float x, float y, int n)
    {
        BattleMonList.Clear(); //전투에 참여하는 몬스터 ID
        BattleMonGrpUid.Clear(); //전투에 참여하는 몬스터의 파티 or 그룹 UID
        string str = "";
        foreach (var m in grp)
        {
            BattleMonList.Add(m);
            str += m + "_";
        }
        BattleMonGrpUid.Add(uid);
        // GameObject[] allMon = GameObject.FindGameObjectsWithTag("Monster");
        // for (int i = 0; i < allMon.Length; i++)
        // {
        //     if (n == i) continue;
        //     if (Vector2.Distance(allMon[i].transform.position, new Vector2(x, y)) < 10f)
        //     {
        //         wMon mon = allMon[i].GetComponent<wMon>();
        //         foreach (var m in mon.monGrp)
        //         {
        //             BattleMonList.Add(m);
        //             str += m + "_";
        //         }
        //     }
        // }
        str = str.TrimEnd('_');
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
