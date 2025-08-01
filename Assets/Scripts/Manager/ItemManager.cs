using GB;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class ItemManager : AutoSingleton<ItemManager>
{
    private WpTable _wpTable;
    private EqTable _eqTable;
    private ItemTable _itemTable;
    public WpTable WpTable => _wpTable ?? (_wpTable = GameDataManager.GetTable<WpTable>());
    public EqTable EqTable => _eqTable ?? (_eqTable = GameDataManager.GetTable<EqTable>());
    public ItemTable ItemTable => _itemTable ?? (_itemTable = GameDataManager.GetTable<ItemTable>());
    public Dictionary<int, ItemData> ItemDataList = new Dictionary<int, ItemData>();
    // 필요시 아이템 관련 메서드 추가 가능

    private void Awake()
    {
        LoadEqData();
        LoadWpData();
        LoadItemData();
    }
    private void LoadEqData()
    {
        foreach (var eq in EqTable.Datas)
        {
            ItemDataList[eq.ItemID] = CreateItemData(eq.ItemID, eq.Name, eq.Type, eq.Price, eq.Val, eq.W, eq.H, eq.Res, eq.Dur);
        }
    }
    private void LoadWpData()
    {
        foreach (var wp in WpTable.Datas)
        {
            ItemDataList[wp.ItemID] = CreateItemData(wp.ItemID, wp.Name, wp.Type, wp.Price, wp.Val, wp.W, wp.H, wp.Res, wp.Dur);
            ItemDataList[wp.ItemID].Both = wp.Both;
            ItemDataList[wp.ItemID].H1AX = wp.H1AX; ItemDataList[wp.ItemID].H1AY = wp.H1AY; //hand1A의 x,y 좌표
            ItemDataList[wp.ItemID].H1BX = wp.H1BX; ItemDataList[wp.ItemID].H1BY = wp.H1BY; //hand1B의 x,y 좌표
            ItemDataList[wp.ItemID].H2X = wp.H2X; ItemDataList[wp.ItemID].H2Y = wp.H2Y; //hand2의 x,y 좌표
            ItemDataList[wp.ItemID].BX = wp.BX; ItemDataList[wp.ItemID].BY = wp.BY; //both의 x,y 좌표
        }
    }
    private void LoadItemData()
    {
        foreach (var item in ItemTable.Datas)
        {
            ItemDataList[item.ItemID] = CreateItemData(item.ItemID, item.Name, item.Type, item.Price, item.Val, item.W, item.H, item.Res, 0);
        }
    }
    private ItemData CreateItemData(int id, string name, int type, int price, int val, int w, int h, string res, int dur)
    {
        return new ItemData { ItemId = id, Name = name, Type = type, Price = price, Val = val, W = w, H = h, Res = res, Dur = dur, X = 0, Y = 0, Dir = 0 };
    }
    public void CreateInvenItem(int id, int x, int y)
    {
        ItemData item = ItemDataList[id].Clone();
        item.X = x;
        item.Y = y;
        item.Uid = GetUid();
        PlayerManager.I.pData.Inven.Add(item);
        //추후에는 특수능력 또는 추가 능력치 붙는 아이템에 대한 대응도 해야함.
        //고민중인건 매개변수에 배열을 두개 넣어서 한개는 능력치 값 id를 넣고 다른 하나는 능력치의 값을 적용할까함
    }
    public int GetUid()
    {
        return 10000000 + Random.Range(0, 89999999);
    }
}