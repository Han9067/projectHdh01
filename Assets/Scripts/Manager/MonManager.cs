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
            mon.W, mon.H, mon.Rng, mon.SdwScr, mon.GgY, mon.Drop);
            // mData.GainExp = GsManager.I.GetGainExp(mData.MaxHP, mData.SP, mData.MP, mData.STR, mData.AGI, mData.INT, mData.CHA, mData.LUK);
            MonDataList[id] = mData;
        }
    }
    //VIT_END_STR_AGI_FOR_INT_CHA_LUK
    private MonData CreateMonData(int id, int monType, string name,
    int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK,
    int w, int h, int rng, float sdwScr, float ggY, string drop)
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
        int hp = VIT * SV.HpVal, sp = END * SV.SpVal, mp = FOR * SV.MpVal;
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
            MAtt = INT * 2,
            Def = VIT,
            MDef = (int)(VIT * 0.5f),
            Crt = 50 + (LUK * 2),
            CrtRate = LUK + AGI,
            Hit = 60 + (AGI / 4),
            Eva = 10 + (AGI / 4),

            Rng = rng,
            AtkType = 0,

            GainExp = GsManager.I.GetGainExp(hp, sp, mp, STR, AGI, INT, CHA, LUK),
            DropList = dList
        };
    }

    public List<int> GetMonWp(int id)
    {
        switch (id)
        {
            case 1:
                return new List<int> { 30003, 56001 };
            case 2:
                return new List<int> { 34003, 56001 };
            case 3:
                return new List<int> { 38001, 56001 };
            case 4:
                return new List<int> { 50002, 54001 };
            case 11:
                return new List<int> { 30022, 50021 };
            case 12:
                return new List<int> { 34022, 50021 };
            case 13:
                return new List<int> { 38021, 50021 };
            case 14:
                return new List<int> { 50021, 54001 };
            case 21:
                return new List<int> { 30024, 50021 };
            case 22:
                return new List<int> { 34022, 50021 };
            case 23:
                return new List<int> { 38023, 50021 };
            case 24:
                return new List<int> { 32002 };
            case 25:
                return new List<int> { 34021 };
            case 26:
                return new List<int> { 40002 };
            case 31:
                return new List<int> { 32024 };
            case 32:
                return new List<int> { 34024 };
            case 33:
                return new List<int> { 40002 };
            case 41:
                return new List<int> { 32042 };
            case 42:
                return new List<int> { 37002 };
            case 43:
                return new List<int> { 40021 };
            default:
                return null;
        }
    }
    public Dictionary<string, int> GetHumanMonPreset(int mId)
    {
        //60번대->일반 도적, 80번대->도적두목(중보~보스 수준)
        switch (mId)
        {
            case 61:
                //Random.Range(1, 5)
                //가죽갑옷에 노헬멧//근접 무기는 H급->30003,34003,38001에 방패 56001 //원거리는 50002
                return new Dictionary<string, int> { { "Armor", 61 }, { "Weapon", 1 } };
            case 62:
                //갬비슨갑옷에 갬비슨헬멧//무기는 G급//근접 무기는 H급->30022,34022,38021 방패 56002 //원거리는 50021
                return new Dictionary<string, int> { { "Armor", 62 }, { "Weapon", Random.Range(11, 15) } };
            case 63:
                //사슬세트//무기는 F급 ->여기는 근접만 있음 근접 무기 H~F급 -> 30024,34022,38023 방패 56021
                return new Dictionary<string, int> { { "Armor", 63 }, { "Weapon", Random.Range(21, 27) } };
            case 81:
                //사슬세트//무기는 양손 G급->32002,34021,40002
                return new Dictionary<string, int> { { "Armor", 81 }, { "Weapon", Random.Range(25, 27) } };
            case 82:
                //브리간딘세트// 무기는 양손 F급 -> 32024,34024,40002
                return new Dictionary<string, int> { { "Armor", 82 }, { "Weapon", Random.Range(31, 34) } };
            case 83:
                //판금갑옷세트// 무기는 E 32042, 37002, 40021
                return new Dictionary<string, int> { { "Armor", 83 }, { "Weapon", Random.Range(41, 44) } };
            default:
                return new Dictionary<string, int>();
        }
    }
}
