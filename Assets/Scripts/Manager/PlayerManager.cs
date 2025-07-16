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
    // 플레이어 데이터 초기화
    public void ApplyPlayerData(PlayerData data)
    {
        pData = new PlayerData();
        pData.Name = data.Name;
        pData.Age = data.Age;
        pData.Gender = data.Gender;
        pData.Sylin = data.Sylin;
        pData.Lv = data.Lv;
        pData.Exp = data.Exp;
        pData.NextExp = data.NextExp;
        pData.HP = data.HP;
        pData.MP = data.MP;
        pData.SP = data.SP;
        pData.VIT = data.VIT;
        pData.END = data.END;
        pData.STR = data.STR;
        pData.AGI = data.AGI;
        pData.FOR = data.FOR;
        pData.INT = data.INT;
        pData.CHA = data.CHA;
        pData.LUK = data.LUK;
        pData.Inven = data.Inven;
    }
    public void DummyPlayerData()
    {
        if (pData == null)
            pData = new PlayerData();
        pData.Name = "앨런";
        pData.Age = 17;
        pData.Gender = 0;
        pData.Sylin = 1000;
        pData.Lv = 1;
        pData.Exp = 0;
        pData.NextExp = 100;
        pData.HP = 50;
        pData.MP = 50;
        pData.SP = 50;
        pData.VIT = 10;
        pData.END = 10;
        pData.STR = 10;
        pData.AGI = 10;
        pData.FOR = 10;
        pData.INT = 10;
        pData.CHA = 10;
        pData.LUK = 10;

        ItemManager.I.CreateItem(20001, 0, 0);
        ItemManager.I.CreateItem(10001, 3, 0);
    }
}
