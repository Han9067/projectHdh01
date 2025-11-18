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
    public List<int> RewardItemIdList = new List<int>();
    public void LoadItemManager()
    {
        LoadEqData();
        LoadWpData();
        LoadItemData();
    }
    private void LoadEqData()
    {
        foreach (var eq in EqTable.Datas)
        {
            ItemDataList[eq.ItemID] = CreateItemData(eq.ItemID, eq.Name, eq.Type, eq.Price, eq.AttKey, eq.AttVal, eq.W, eq.H, eq.Res, eq.Dur);
        }
    }
    private void LoadWpData()
    {
        foreach (var wp in WpTable.Datas)
        {
            ItemDataList[wp.ItemID] = CreateItemData(wp.ItemID, wp.Name, wp.Type, wp.Price, wp.AttKey, wp.AttVal, wp.W, wp.H, wp.Res, wp.Dur, wp.Both);
            // ItemDataList[wp.ItemID].Both = wp.Both;
        }
    }
    private void LoadItemData()
    {
        foreach (var item in ItemTable.Datas)
        {
            ItemDataList[item.ItemID] = CreateItemData(item.ItemID, item.Name, item.Type, item.Price, item.AttKey, item.AttVal, item.W, item.H, item.Res, 0);
        }
    }
    private ItemData CreateItemData(int id, string name, int type, int price, string keys, string vals, int w, int h, string res, int dur, int both = 0)
    {
        string[] kArr = keys.Split('_');
        string[] vArr = vals.Split('_');
        Dictionary<int, int> att = new Dictionary<int, int>();
        for (int i = 0; i < kArr.Length; i++)
            att[int.Parse(kArr[i])] = int.Parse(vArr[i]);

        return new ItemData { ItemId = id, Name = name, Type = type, Price = price, Att = att, W = w, H = h, Res = res, Dur = dur, X = 0, Y = 0, Dir = 0, Grade = 1, Both = both };
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
    public static int GetGradeIndex(int gradeValue)
    {
        // 0~99로 클램프
        gradeValue = Mathf.Clamp(gradeValue, 0, 99);

        // 0~1로 정규화
        float t = gradeValue / 99f;

        // 저등급에 오래 머물도록 제곱
        float expectedIndex = 9f * t * t; // 0(H) ~ 9(SS)
        float spread = 3f;

        float[] weights = new float[10];

        // 1) 기대 인덱스 주변으로 가중치 계산
        for (int i = 0; i < 10; i++)
        {
            float d = Mathf.Abs(i - expectedIndex);
            float w = 1f - d / spread;
            if (w < 0f) w = 0f;
            weights[i] = w;
        }

        // 2) 정규화해서 확률(%) 구하기
        float sumW = 0f;
        for (int i = 0; i < 10; i++)
            sumW += weights[i];

        float[] percents = new float[10];

        if (sumW > 0f)
        {
            for (int i = 0; i < 10; i++)
                percents[i] = weights[i] / sumW * 100f;
        }
        else
            percents[0] = 100f;

        // 3) 소수 첫째 자리까지 반올림 + 합 보정
        float sumRounded = 0f;
        for (int i = 0; i < 10; i++)
        {
            percents[i] = Mathf.Round(percents[i] * 10f) / 10f;
            sumRounded += percents[i];
        }

        int maxIdx = 0;
        for (int i = 1; i < 10; i++)
        {
            if (percents[i] > percents[maxIdx])
                maxIdx = i;
        }

        float diff = 100f - sumRounded;
        percents[maxIdx] += diff;
        percents[maxIdx] = Mathf.Clamp(percents[maxIdx], 0f, 100f);

        // 4) 실제 등급 하나 뽑기 (0~9)
        float r = Random.Range(0f, 100f);
        float acc = 0f;

        for (int i = 0; i < 10; i++)
        {
            acc += percents[i];
            if (r <= acc)
            {
                return i;   // 0=H, 1=G, ... 9=SS
            }
        }
        return 0;//H 등급 고정
    }
    public void CalcDropItem(int itemId, int rate)
    {
        int ran = Random.Range(0, 100);
        if (ran > rate)
            RewardItemIdList.Add(itemId);
    }
    public void TestDropItem()
    {
        RewardItemIdList.Clear();
        CalcDropItem(68001, 100);
        CalcDropItem(68001, 100);
        CalcDropItem(68001, 100);
        CalcDropItem(68001, 100);
        CalcDropItem(68001, 100);
        CalcDropItem(68002, 100);

        // UIManager.ShowPopup("BattleRewardPop");
        // Presenter.Send("BattleRewardPop", "SetReward");
    }
    public int GetUid()
    {
        return 10000000 + Random.Range(0, 89999999);
    }
}