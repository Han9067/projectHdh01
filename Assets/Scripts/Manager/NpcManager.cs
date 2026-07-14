using GB;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcManager : AutoSingleton<NpcManager>
{
    private NpcTable _npcTable;
    public NpcTable NpcTable => _npcTable ?? (_npcTable = GameDataManager.GetTable<NpcTable>());
    public Dictionary<int, NpcData> NpcDataList = new Dictionary<int, NpcData>();
    public void LoadNpcManager()
    {
        LoadNpcData();
    }
    private void LoadNpcData()
    {
        //나중에 NPC는 플레이어 데이터처럼 저장된 데이터가 있다면 거기서 불러오도록 해야함
        foreach (var npc in NpcTable.Datas)
        {
            int[] stat = npc.Stat.Split('_').Select(int.Parse).ToArray(); //VIT_END_STR_AGI_FOR_INT_CHA_LUK
            int[] parts = npc.Parts.Split('_').Select(int.Parse).ToArray(); //Skin_Face_Eyebrow_Eye_EyeColor_Ear_Nose_Mouth_Hair_HairColor
            int id = npc.NpcID;
            NpcData data = new NpcData();
            data.NpcId = id;
            data.Name = npc.Name; // 이름
            data.Age = npc.Age; // 나이
            data.Gen = npc.Gen; // 0: 남성, 1: 여성
            data.Fame = npc.Fame; // 명성
            data.Rls = 0; // 플레이어와의 친밀도
            data.VIT = stat[0]; data.END = stat[1]; data.STR = stat[2]; data.AGI = stat[3];
            data.FOR = stat[4]; data.INT = stat[5]; data.CHA = stat[6]; data.LUK = stat[7];
            data.Lv = GsManager.I.GetLv(data.VIT, data.END, data.STR, data.AGI, data.FOR, data.INT, data.CHA, data.LUK);
            /////
            data.Skin = parts[0]; data.Face = parts[1]; data.Eyebrow = parts[2]; data.Eye = parts[3]; data.EyeColor = parts[4];
            data.Ear = parts[5]; data.Nose = parts[6]; data.Mouth = parts[7]; data.Hair = parts[8]; data.HairColor = parts[9];
            data.Beard = parts[10]; data.BeardColor = parts[11];
            data.IsView = npc.IsView == 0;
            /////
            string[] eq = npc.Eq.Split('/'); //상하의_신발_투구_장갑_벨트_목걸이_반지1_반지2
            string[] eqName = new string[] { "Armor", "Shoes", "Helmet", "Gloves", "Belt", "Necklace", "Ring1", "Ring2" };
            foreach (string name in eqName)
                data.EqSlot[name] = null;
            for (int i = 0; i < eq.Length; i++)
            {
                string[] eqStr = eq[i].Split('_');
                if (eqStr[0] == "0") continue;
                string key = eqName[i];
                int eqId = int.Parse(eqStr[0]);
                data.EqSlot[key] = ItemManager.I.ItemDataList[eqId].Clone();
                if (eqStr.Length == 1) continue;
                data.EqSlot[key].PfmVal = int.Parse(eqStr[1]);
                int curG = ItemManager.I.GetCurItemGrade(data.EqSlot[key].Grade, data.EqSlot[key].PfmVal);
                int diff = curG - data.EqSlot[key].Grade;
                data.EqSlot[key].Grade = curG;
                data.EqSlot[key].Att[1] += diff; //추후에 향상값을 디테일하게 수정
                if (eqStr.Length == 2)
                {
                    data.EqSlot[key] = null;
                    continue;
                }
                //특성
                for (int j = 2; j < eqStr.Length; j++)
                {
                    string[] attStr = eqStr[j].Split('+');
                    int eqAttId = int.Parse(attStr[0]), eqAttVal = int.Parse(attStr[1]);
                    if (data.EqSlot[key].Att.ContainsKey(eqAttId))
                        data.EqSlot[key].Att[eqAttId] += eqAttVal;
                    else
                        data.EqSlot[key].Att.Add(eqAttId, eqAttVal);
                }
            }
            string[] wp = npc.Wp.Split('/'); //손1_손2
            string[] wpName = new string[] { "Hand1", "Hand2" };
            foreach (string name in wpName)
                data.EqSlot[name] = null;
            for (int i = 0; i < wp.Length; i++)
            {
                string[] wpStr = wp[i].Split('_');
                if (wpStr[0] == "0") continue;
                string key = wpName[i];
                int wpId = int.Parse(wpStr[0]);
                data.EqSlot[key] = ItemManager.I.ItemDataList[wpId].Clone();
                if (wpStr.Length == 1) continue;
                data.EqSlot[key].PfmVal = int.Parse(wpStr[1]);
                int curG = ItemManager.I.GetCurItemGrade(data.EqSlot[key].Grade, data.EqSlot[key].PfmVal);
                int diff = curG - data.EqSlot[key].Grade;
                data.EqSlot[key].Grade = curG;
                data.EqSlot[key].Att[2] += diff * 2; //추후에 향상값을 디테일하게 수정
                if (wpStr.Length == 2) continue;
                //특성
                for (int j = 2; j < wpStr.Length; j++)
                {
                    string[] attStr = wpStr[j].Split('+');
                    int wpAttId = int.Parse(attStr[0]), wpAttVal = int.Parse(attStr[1]);
                    if (data.EqSlot[key].Att.ContainsKey(wpAttId))
                        data.EqSlot[key].Att[wpAttId] += wpAttVal;
                    else
                        data.EqSlot[key].Att.Add(wpAttId, wpAttVal);
                }
            }
            CalcNpcStat(data);

            data.NextExp = GsManager.I.GetNextExp(data.Lv);
            data.GainExp = GsManager.I.GetGainExp(data.HP, data.SP, data.MP, data.STR, data.AGI, data.INT, data.CHA, data.LUK);
            data.Exp = Random.Range(0, data.GainExp);
            NpcDataList[id] = data;

            data.CityId = npc.City;
        }
    }
    private void CalcNpcStat(NpcData npcData)
    {
        npcData.MaxHP = npcData.VIT * SV.HpVal + npcData.AddHP;
        npcData.MaxMP = npcData.INT * SV.MpVal + npcData.AddMP;
        npcData.MaxSP = npcData.END * SV.SpVal + npcData.AddSP;
        if (npcData.HP > npcData.MaxHP) npcData.HP = npcData.MaxHP;
        if (npcData.MP > npcData.MaxMP) npcData.MP = npcData.MaxMP;
        if (npcData.SP > npcData.MaxSP) npcData.SP = npcData.MaxSP;

        npcData.Att = npcData.STR * 2;
        npcData.MAtt = npcData.INT * 2;
        npcData.Def = npcData.VIT;
        npcData.MDef = (int)(npcData.VIT * 0.5f);
        npcData.Crt = 50 + (npcData.LUK * 2);
        npcData.CrtRate = npcData.LUK;
        int agi = npcData.AGI / 4;
        npcData.Hit = 60 + agi;
        npcData.Eva = 10 + agi;
        npcData.Rng = npcData.EqSlot["Hand1"] != null ? npcData.EqSlot["Hand1"].Rng : 1;
        //////
        string[] eq = new string[] { "Hand1", "Hand2", "Armor", "Shoes", "Helmet", "Gloves", "Belt", "Necklace", "Ring1", "Ring2" };
        foreach (string e in eq)
        {
            if (npcData.EqSlot[e] == null) continue;
            foreach (var att in npcData.EqSlot[e].Att)
            {
                switch (att.Key)
                {
                    case 1:
                    case 20:
                        npcData.Def += att.Value; // 방어력
                        break;
                    case 2:
                    case 21:
                        npcData.Att += att.Value; // 공격력
                        break;
                }
            }
        }
        npcData.AtkType = npcData.EqSlot["Hand1"] != null && npcData.EqSlot["Hand1"].Hand == 2 ? 1 : 0;
    }
    public void AddNpcRls(int npcId, int val)
    {
        NpcDataList[npcId].Rls += val;
    }
    public List<NpcData> GetCityNpcList(int cityId)
    {
        List<NpcData> npcDataList = new List<NpcData>();
        foreach (var npc in NpcDataList)
        {
            if (npc.Key > 1000 && npc.Value.CityId == cityId)
                npcDataList.Add(npc.Value);
        }
        return npcDataList;
    }
}
