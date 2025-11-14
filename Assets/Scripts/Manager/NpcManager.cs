using GB;
using System.Collections.Generic;
using System.Linq;

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
        foreach (var npc in NpcTable.Datas)
        {
            int[] stat = npc.Stat.Split('_').Select(int.Parse).ToArray(); //VIT_END_STR_AGI_FOR_INT_CHA_LUK
            int[] parts = npc.Parts.Split('_').Select(int.Parse).ToArray(); //Skin_Face_Eyebrow_Eye_EyeColor_Ear_Nose_Mouth_Hair_HairColor
            int[] eq = npc.Eq.Split('_').Select(int.Parse).ToArray(); //상하의_신발_투구_장갑_벨트_망토_목걸이_반지1_반지2
            int[] wp = npc.Wp.Split('_').Select(int.Parse).ToArray(); //손1_손2
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
            /////
            data.EqSlot["Armor"] = eq[0] == 0 ? null : ItemManager.I.ItemDataList[eq[0]];
            data.EqSlot["Shoes"] = eq[1] == 0 ? null : ItemManager.I.ItemDataList[eq[1]];
            data.EqSlot["Helmet"] = eq[2] == 0 ? null : ItemManager.I.ItemDataList[eq[2]];
            data.EqSlot["Gloves"] = eq[3] == 0 ? null : ItemManager.I.ItemDataList[eq[3]];
            data.EqSlot["Belt"] = eq[4] == 0 ? null : ItemManager.I.ItemDataList[eq[4]];
            data.EqSlot["Cape"] = eq[5] == 0 ? null : ItemManager.I.ItemDataList[eq[5]];
            data.EqSlot["Necklace"] = eq[6] == 0 ? null : ItemManager.I.ItemDataList[eq[6]];
            data.EqSlot["Ring1"] = eq[7] == 0 ? null : ItemManager.I.ItemDataList[eq[7]];
            data.EqSlot["Ring2"] = eq[8] == 0 ? null : ItemManager.I.ItemDataList[eq[8]];
            data.EqSlot["Hand1"] = wp[0] == 0 ? null : ItemManager.I.ItemDataList[wp[0]];
            data.EqSlot["Hand2"] = wp[1] == 0 ? null : ItemManager.I.ItemDataList[wp[1]];
            /////
            CalcNpcStat(data);

            data.Exp = 0;
            data.NextExp = GsManager.I.GetNextExp(data.Lv);
            // data.GainExp = GsManager.I.GetGainExp(data.HP, data.SP, data.MP, data.STR, data.AGI, data.INT, data.CHA, data.LUK);
            data.GainExp = 0;
            NpcDataList[id] = data;
        }
    }
    private void CalcNpcStat(NpcData npcData)
    {
        npcData.MaxHP = npcData.VIT * 4 + npcData.AddHP;
        npcData.MaxMP = npcData.INT * 4 + npcData.AddMP;
        npcData.MaxSP = npcData.END * 4 + npcData.AddSP;
        if (npcData.HP > npcData.MaxHP) npcData.HP = npcData.MaxHP;
        if (npcData.MP > npcData.MaxMP) npcData.MP = npcData.MaxMP;
        if (npcData.SP > npcData.MaxSP) npcData.SP = npcData.MaxSP;

        npcData.Att = npcData.STR * 2;
        npcData.Def = npcData.VIT;
        npcData.Crt = 50 + (npcData.LUK * 2);
        npcData.CrtRate = npcData.LUK;
        int agi = npcData.AGI / 4;
        npcData.Hit = 60 + agi;
        npcData.Eva = 10 + agi;
        //////
        string[] eq = new string[] { "Hand1", "Hand2", "Armor", "Shoes", "Helmet", "Gloves", "Belt", "Cape", "Necklace", "Ring1", "Ring2" };
        foreach (string e in eq)
        {
            if (npcData.EqSlot[e] != null)
            {
                switch (e)
                {
                    case "Hand1":
                    case "Hand2":
                        npcData.Att += npcData.EqSlot[e].Att[13]; // 공격력
                        break;
                    case "Necklace":
                    case "Ring1":
                    case "Ring2":
                        break;
                    default:
                        npcData.Def += npcData.EqSlot[e].Att[12]; // 방어력
                        break;
                }
            }
        }
    }
}
