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
            ItemDataList[eq.ID] = CreateItemData(eq.ID, eq.Name, eq.Type, eq.Price, eq.Val, eq.W, eq.H, eq.Res, eq.Dur, $"Images/Item/Eq/{eq.Res}");
        }
    }
    private void LoadWpData()
    {
        foreach (var wp in WpTable.Datas)
        {
            ItemDataList[wp.ID] = CreateItemData(wp.ID, wp.Name, wp.Type, wp.Price, wp.Val, wp.W, wp.H, wp.Res, wp.Dur, $"Images/Item/Wp/{wp.Res}");
            ItemDataList[wp.ID].Both = wp.Both;
        }
    }
    private void LoadItemData()
    {
        foreach (var item in ItemTable.Datas)
        {
            ItemDataList[item.ID] = CreateItemData(item.ID, item.Name, item.Type, item.Price, item.Val, item.W, item.H, item.Res, 0, $"Images/Item/Ect/{item.Res}");
        }
    }
    private ItemData CreateItemData(int id, string name, int type, int price, int val, int w, int h, string res, int dur, string path)
    {
        return new ItemData { ItemId = id, Name = name, Type = type, Price = price, Val = val, W = w, H = h, Res = res, Dur = dur, Path = path, X = 0, Y = 0, Dir = 0 };
    }
    public void CreateItem(int id, int x, int y)
    {
        ItemData item = ItemDataList[id].Clone();
        item.X = x;
        item.Y = y;
        item.Uid = GetUid();
        PlayerManager.I.pData.Inven.Add(item);
    }
    public int GetUid()
    {
        return 10000000 + Random.Range(0, 89999999);
    }
}