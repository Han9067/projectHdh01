using GB;
using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : AutoSingleton<PlayerManager>
{

    [Header("플레이어 데이터")]
    public PlayerData pData;
    public List<List<InvenGrid>> grids;
    protected void Awake()
    {
        if(I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        InitGrid();
    }
    // 인벤토리 그리드 초기화
    private void InitGrid()
    {
        grids = new List<List<InvenGrid>>();
        for(int y = 0; y < 10; y++)
        {
            List<InvenGrid> row = new List<InvenGrid>();
            for(int x = 0; x < 10; x++)
            {
                row.Add(new InvenGrid { x = x, y = y, slotId = -1 });
            }
            grids.Add(row);
        }
    }
    public void ApplyEqSlot(string eq, ItemData data)
    {
        pData.EqSlot[eq] = data;
        // Debug.Log(eq + " " + data.Name);
        //아이템의 능력치를 캐릭터에 부여 또는 계산하여 적용

        //또한 캐릭터의 무기, 갑옷, 투구 착용시 캐릭터의 외형이 변경되는것을 구현해야함
        // pData.WpState = 0;
        // if(eq == "Hand1" || eq == "Hand2")
        // {   
        //     // //1,3,5,9 한손 || 2,4,6,7,8 양손
        //     // switch(data.Type)
        //     // {
        //     //     case 1:
        //     //     case 3:
        //     //     case 5:
        //     //     case 9:
        //     //         pData.WpState = 1;
        //     //         break;
        //     //     case 2:
        //     //     case 4:
        //     //     case 6:
        //     //     case 7:
        //     //     case 8:
        //     //         break;
        //     // }
        // }
        //WpState; // 0: 무기 없음, 1: 손1에만 무기, 2: 손2에만 무기, 3: 손1,손2 각각 다른 무기 착용, 4: 양손
    }
    public void TakeoffEq(string eq)
    {
        pData.EqSlot[eq] = null;
        // if(eq == "Hand1" || eq == "Hand2")
        //     CheckPlayerHands();
    }
    private void CheckPlayerHands(string hand, ItemData data)
    {
        
        // switch(data.Type)
            // {
            //     case 1:
            //     case 3:
            //     case 5:
            //     case 9:
            //         pData.WpState = 1;
            //         break;
            //     case 2:
            //     case 4:
            //     case 6:
            //     case 7:
            //     case 8:
            //         break;
            // }
    }
    
    // 플레이어 데이터 초기화
    public void ApplyPlayerData(PlayerData data)
    {
        pData = new PlayerData();
        pData.Name = data.Name;
        pData.Age = data.Age;
        pData.Gender = data.Gender;
        pData.Silver = data.Silver;
        pData.Lv = data.Lv;
        pData.Exp = data.Exp;
        pData.NextExp = data.NextExp;
        pData.HP = data.HP;pData.MP = data.MP;pData.SP = data.SP;
        pData.AddHP = data.AddHP;pData.AddMP = data.AddMP;pData.AddSP = data.AddSP;

        pData.VIT = data.VIT;pData.END = data.END;pData.STR = data.STR;pData.AGI = data.AGI;pData.FOR = data.FOR;pData.INT = data.INT;pData.CHA = data.CHA;pData.LUK = data.LUK;
        
        pData.Inven = data.Inven;

        pData.Skin = data.Skin;
        pData.Face = data.Face;
        pData.Eyebrow = data.Eyebrow;
        pData.Eye = data.Eye;
        pData.EyeColor = data.EyeColor;
        pData.Ear = data.Ear;
        pData.Nose = data.Nose;
        pData.Mouth = data.Mouth;
        pData.Hair = data.Hair;
        pData.HairColor = data.HairColor;

        CalcPlayerStat();
    }
    public void DummyPlayerData()
    {
        if (pData == null)
            pData = new PlayerData();
        pData.Name = "앨런";
        pData.Age = 17;
        pData.Gender = 0;
        pData.Silver = 1000;
        pData.Lv = 1;
        pData.Exp = 0;
        pData.NextExp = 100;
        pData.HP = 100;pData.MP = 100;pData.SP = 100;
        pData.AddHP = 0;pData.AddMP = 0;pData.AddSP = 0;
        pData.VIT = 10;pData.END = 10;pData.STR = 10;pData.AGI = 10;pData.FOR = 10;pData.INT = 10;pData.CHA = 10;pData.LUK = 10;

        pData.Skin = 2; pData.Face = 1;
        pData.Eyebrow = 1; pData.Eye = 1;
        pData.EyeColor = 1; pData.Ear = 1;
        pData.Nose = 1; pData.Mouth = 1;
        pData.Hair = 1; pData.HairColor = 1;

        ItemManager.I.CreateItem(20001, 0, 0);
        ItemManager.I.CreateItem(10001, 3, 0);
        ItemManager.I.CreateItem(20002, 1, 0);

        CalcPlayerStat();
    }
    private void CalcPlayerStat()
    {
        pData.MaxHP = pData.VIT * 10 + pData.AddHP;
        pData.MaxMP = pData.INT * 10 + pData.AddMP;
        pData.MaxSP = pData.END * 10 + pData.AddSP;
        if(pData.HP > pData.MaxHP)pData.HP = pData.MaxHP;
        if(pData.MP > pData.MaxMP)pData.MP = pData.MaxMP;
        if(pData.SP > pData.MaxSP)pData.SP = pData.MaxSP;

        pData.Att = pData.STR * 2;
        pData.Def = pData.VIT;
        pData.Crt = 50 + (pData.LUK * 2);
        pData.CrtRate = pData.LUK;
        pData.Acc = 80 + pData.AGI;
        pData.Dod = 10 + pData.AGI;
    }
}
