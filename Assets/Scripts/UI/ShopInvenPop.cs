using GB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopInvenPop : UIScreen
{
    public GameObject shopItemPrefab;
    public static bool isActive { get; private set; } = false;
    private int gw = 10; //기본 넓이 10칸
    private int gh = 12; //기본 높이 12칸
    private List<List<InvenGrid>> grids;
    public RectTransform content; // ScrollView의 Content 오브젝트
    public Sprite gridSpr;  // 10칸짜리(640x64) 그리드 이미지
    public List<ItemData> ItemList = new List<ItemData>();
    public class ItemPos
    {
        public ItemData itemData;
        public int x;
        public int y;
    }
    private void Awake()
    {
        Regist();
        RegistButton();
        InitGrid();
    }
    
    private void InitGrid()
    {
        grids = new List<List<InvenGrid>>();
        for(int y = 0; y < gh; y++)
        {
            List<InvenGrid> row = new List<InvenGrid>();
            for(int x = 0; x < gw; x++)
            {
                row.Add(new InvenGrid { x = x, y = y, slotId = -1 });
            }
            grids.Add(row);
        }
    }
    
    private void OnEnable()
    {
        Presenter.Bind("ShopInvenPop",this);
        isActive = true;
    }
    private void OnDisable() 
    {
        Presenter.UnBind("ShopInvenPop", this);
        isActive = false;
    }

    public void RegistButton()
    {
        foreach(var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key);});
    }

    public void OnButtonClick(string key)
    {
        switch (key)
        {
            case "ShopInvenPopClose":
                UIManager.ClosePopup("ShopInvenPop");
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {   
        string name = "";
        switch(key)
        {
            case "LoadSmith":
                name = "대장간";
                break;
            case "LoadTailor":
                name = "재봉사";
                break;
            case "LoadApothecary":
                name = "약제상";
                break;
        }
        mTexts["ShopName"].text = name;
        CreateGrid(data.Get<int>());
    }
    public void CreateGrid(int id)
    {
        // 그리드 초기화
        InitGrid();
        ItemList.Clear();
        List<ItemPos> itemPosList = new List<ItemPos>();
        var shopData = ShopManager.I.shopAllData[id];
        var items = shopData.items;
        foreach(var item in items)
        {
            ItemData data = ItemManager.I.ItemDataList[item.itemId].Clone();
            Vector2Int pos = ApplyGrid(data.ItemId, data.W, data.H);
            ItemPos itemPos = new ItemPos { itemData = data, x = pos.x, y = pos.y };
            ItemList.Add(data);
            itemPosList.Add(itemPos);
        }
        // 그리드 배경 행 이미지 생성
        DrawGrid();
        // 아이템 오브젝트 생성
        CreateShopItem(itemPosList);
        
    }
    public Vector2Int ApplyGrid(int slotId, int w, int h)
    {
        Vector2Int pos = FindAvailablePosition(w, h);
        PlaceItem(slotId, pos.x, pos.y, w, h);
        return pos;
    }
    
    private Vector2Int FindAvailablePosition(int w, int h)
    {
        // 항상 왼쪽 위(0,0)부터 오른쪽으로 차례대로 빈 공간 찾기
        for (int y = 0; y < gh; y++)
        {
            for (int x = 0; x < gw; x++)
            {
                if (CanPlaceItem(x, y, w, h))
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        // 배치할 공간이 없으면 그리드 확장
        return ExpandGridAndFindPos(w, h);
    }
    
    private Vector2Int ExpandGridAndFindPos(int w, int h)
    {
        // 필요한 만큼 그리드 높이 확장
        int requiredHeight = gh;
        while (requiredHeight < gh + h)
        {
            requiredHeight++;
        }
        
        // 그리드 확장
        ExpandGridHeight(requiredHeight - gh);
        
        // 확장된 그리드에서 다시 위치 찾기
        for (int y = gh - h; y < gh; y++)
        {
            for (int x = 0; x < gw; x++)
            {
                if (CanPlaceItem(x, y, w, h))
                {
                    UnityEngine.Debug.Log($"그리드 높이를 {gh}로 확장하여 아이템 배치");
                    return new Vector2Int(x, y);
                }
            }
        }
        
        // 여전히 배치할 수 없다면 (너무 큰 아이템인 경우)
        return new Vector2Int(-1, -1);
    }
    
    private void ExpandGridHeight(int additionalRows)
    {
        // 새로운 행들을 추가
        for (int i = 0; i < additionalRows; i++)
        {
            List<InvenGrid> newRow = new List<InvenGrid>();
            for (int x = 0; x < gw; x++)
            {
                newRow.Add(new InvenGrid { x = x, y = gh + i, slotId = -1 });
            }
            grids.Add(newRow);
        }
        
        // 그리드 높이 업데이트
        gh += additionalRows;
        UnityEngine.Debug.Log($"그리드 높이가 {gh}로 확장되었습니다.");
    }
    
    /// 지정된 위치에 아이템을 배치할 수 있는지 확인합니다.
    private bool CanPlaceItem(int startX, int startY, int w, int h)
    {
        // 그리드 범위를 벗어나는지 체크
        if (startX + w > gw || startY + h > gh)
            return false;
        // 해당 영역에 이미 다른 아이템이 있는지 체크
        for (int y = startY; y < startY + h; y++)
        {
            for (int x = startX; x < startX + w; x++)
            {
                if (grids[y][x].slotId != -1)
                    return false;
            }
        }
        return true;
    }
    /// 지정된 위치에 아이템을 실제로 배치합니다.
    private void PlaceItem(int slotId, int startX, int startY, int w, int h)
    {
        // 아이템이 차지하는 모든 칸에 slotId 설정
        for (int y = startY; y < startY + h; y++)
        {
            for (int x = startX; x < startX + w; x++)
            {
                grids[y][x].slotId = slotId;
            }
        }
    }
    private void DrawGrid()
    {
        // 그리드 바탕 그리기
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        float startY = -2f;
        for (int y = 0; y < gh; y++)
        {
            GameObject row = new GameObject($"GridRow_{y}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(content, false);
            Image img = row.GetComponent<Image>();
            img.sprite = gridSpr;
            img.SetNativeSize(); // 640x64
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(0, startY + (y * -64));
        }
        // Content 높이 자동 조정
        content.sizeDelta = new Vector2(content.sizeDelta.x, (gh * 64) + 6);
    }
    public void CreateShopItem(List<ItemPos> itemPosList)
    {
        foreach (var data in itemPosList)
        {
            Sprite iSpr = ResManager.GetSprite(data.itemData.Res);
            // 프리팹 인스턴스화
            GameObject shopItem = Instantiate(shopItemPrefab, content);
            
            int w = data.itemData.W * 64, h = data.itemData.H * 64;
            // RectTransform 설정
            RectTransform rt = shopItem.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(w, h);
            
            // 그리드 좌표에 맞게 위치 설정 (64는 한 칸의 픽셀 크기)
            rt.anchoredPosition = new Vector2(data.x * 64, -(data.y * 64));

            // 아이템 정보 저장 (필요시)
            shopItem.name = $"ShopItem_{data.itemData.ItemId}";
            shopItem.GetComponent<ShopItem>().SetItemData(data.itemData);
            shopItem.GetComponent<ShopItem>().SetItemImage(iSpr);
        }
    }
    public override void Refresh(){ }
}