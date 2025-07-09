using GB;

public class ItemManager : AutoSingleton<ItemManager>
{
    private WpTable _wpTable;
    private EqTable _eqTable;
    private ItemTable _itemTable;
    public WpTable WpTable => _wpTable ?? (_wpTable = GameDataManager.GetTable<WpTable>());
    public EqTable EqTable => _eqTable ?? (_eqTable = GameDataManager.GetTable<EqTable>());
    public ItemTable ItemTable => _itemTable ?? (_itemTable = GameDataManager.GetTable<ItemTable>());

    // 필요시 아이템 관련 메서드 추가 가능
    public object GetItemInfo(string id, int type)
    {
        ItemInfo info = new ItemInfo();
        switch (type)
        {
            case 1: // EqTable (장비)
                var eq = EqTable[id];
                if (eq != null)
                {
                    info.itemId = eq.ID;
                    info.Name = eq.Name;
                    info.Type = eq.Type;
                    info.Price = eq.Price;
                    info.Val = eq.Val;
                    info.W = eq.W;
                    info.H = eq.H;
                    info.Res = eq.Res;
                    info.Dur = eq.Dur; // 장비는 Dur 포함
                }
                break;
            case 2: // WpTable (무기)
                var wp = WpTable[id];
                if (wp != null)
                {
                    info.itemId = wp.ID;
                    info.Name = wp.Name;
                    info.Type = wp.Type;
                    info.Price = wp.Price;
                    info.Val = wp.Val;
                    info.W = wp.W;
                    info.H = wp.H;
                    info.Res = wp.Res;
                    info.Dur = wp.Dur; // 무기도 Dur 포함
                }
                break;
            case 3: // ItemTable (일반 아이템)
                var item = ItemTable[id];
                if (item != null)
                {
                    info.itemId = item.ID;
                    info.Name = item.Name;
                    info.Type = item.Type;
                    info.Price = item.Price;
                    info.Val = item.Val;
                    info.W = item.W;
                    info.H = item.H;
                    info.Res = item.Res;
                    info.Dur = 0; // 일반 아이템은 Dur 없으므로 0으로 설정
                }
                break;
        }
        return info;
    }
}