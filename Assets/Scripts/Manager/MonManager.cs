using GB;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MonManager : AutoSingleton<MonManager>
{
    private MonTable _monTable;
    public MonTable MonTable => _monTable ?? (_monTable = GameDataManager.GetTable<MonTable>());
    public Dictionary<int, MonData> MonDataList = new Dictionary<int, MonData>();
    // public List<int> BattleMonList = new List<int>();
    // public List<int> BattleMonGrpUid = new List<int>();
    public void LoadMonManager()
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
            mon.W, mon.H, mon.SdwScr, mon.GgY, mon.Drop);
            // mData.GainExp = GsManager.I.GetGainExp(mData.MaxHP, mData.SP, mData.MP, mData.STR, mData.AGI, mData.INT, mData.CHA, mData.LUK);
            MonDataList[id] = mData;
        }
    }
    //VIT_END_STR_AGI_FOR_INT_CHA_LUK
    private MonData CreateMonData(int id, int monType, string name,
    int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK,
    int w, int h, float sdwScr, float ggY, string drop)
    {
        string[] dropArr = drop.Split('/');
        List<MonData.DropData> dList = new List<MonData.DropData>();
        foreach (var d in dropArr)
        {
            string[] arr = d.Split('_');
            int itemId = int.Parse(arr[0]);
            int rate = int.Parse(arr[1]);
            int val = arr.Length > 2 ? int.Parse(arr[2]) : 0;
            dList.Add(new MonData.DropData(itemId, rate, val));
        }
        int hp = VIT * SV.HpVal, sp = END * SV.SpVal, mp = INT * SV.MpVal;
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
            Lv = GsManager.I.GetLv(VIT, END, STR, AGI, FOR, INT, CHA, LUK),

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

            GainExp = GsManager.I.GetGainExp(hp, sp, mp, STR, AGI, INT, CHA, LUK),
            DropList = dList
        };
    }
}
