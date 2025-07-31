using GB;
using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : AutoSingleton<PlayerManager>
{

    [Header("플레이어 데이터")]
    public int currentCity = 0;
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
        // UnityEngine.Debug.Log(eq + " 장착");
        pData.EqSlot[eq] = data;
        CalcPlayerStat();
        CheckCharInfoPop();
    }
    public void TakeoffEq(string eq)
    {
        // UnityEngine.Debug.Log(eq + " 해제");
        pData.EqSlot[eq] = null;
        CalcPlayerStat();
        CheckCharInfoPop();
    }
    private void CheckCharInfoPop()
    {
        if(GameObject.Find("CharInfoPop") && GameObject.Find("CharInfoPop").GetComponent<CharInfoPop>().isActive)
            GameObject.Find("CharInfoPop").GetComponent<CharInfoPop>().UpdateCharInfo();
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
        Presenter.Send("WorldMainUI", "UpdateGoldTxt", pData.Silver.ToString());
    }
    public void DummyPlayerData()
    {
        if (pData == null)
            pData = new PlayerData();
        pData.Name = "앨런";
        pData.Age = 17;
        pData.Gender = 0;
        pData.Silver = 20000;
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

        ItemManager.I.CreateInvenItem(30001, 0, 0);
        ItemManager.I.CreateInvenItem(30002, 1, 0);
        ItemManager.I.CreateInvenItem(10001, 3, 0);

        CalcPlayerStat();
        Presenter.Send("WorldMainUI", "UpdateGoldTxt", pData.Silver.ToString());
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
        //////
        string[] eq = new string[] {"Hand1", "Hand2", "Armor", "Shoes", "Helmet", "Gloves", "Belt", "Cape", "Necklace", "Ring1", "Ring2"};
        foreach(string e in eq)
        {
            if(pData.EqSlot[e] != null)
            {
                switch(e)
                {
                    case "Hand1":
                    case "Hand2":
                        // Debug.Log(pData.EqSlot[e].Val);
                        pData.Att += pData.EqSlot[e].Val;
                        break;
                    case "Necklace":
                    case "Ring1":
                    case "Ring2":
                        break;
                    default:
                        pData.Def += pData.EqSlot[e].Val;
                        break;
                }
            }
        }
    }
    public Vector2 CanAddItem(int w, int h){
        // Debug.Log(w + "   " + h);
        for(int y = 0; y < 10; y++){
            if(y + h > 10)break;
            for(int x = 0; x < 10; x++){
                if(x + w > 10)break;
                bool isAdd = true;
                for(int i = y; i < y + h; i++){
                    for(int j = x; j < x + w; j++){
                        if(grids[i][j].slotId != -1){
                            isAdd = false;
                            break;
                        }
                    }
                }
                if(isAdd)return new Vector2(x, y);
            }
        }
        return new Vector2(-1, -1);
        //현재 버그있음
        //추후에는 빈칸일때 회전된 상태로도 검색하는 기능도 추가해야함
    }
}
