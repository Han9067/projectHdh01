using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.UI;

public class InvenPop : UIScreen
{
    [SerializeField] private Transform slotParent; // Slot 게임오브젝트
    [SerializeField] private Transform eqParent; // 장비 게임오브젝트
    [SerializeField] private GameObject itemPrefab;
    public GameObject[,] gridObj;
    private List<List<InvenGrid>> pGrids;
    private List<GameObject> curItem = new List<GameObject>();
    private bool MoveOn = false;
    private int curIdx = 0;
    private int curType = 0;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        InitGrid();
        LoadPlayerInven();
    }
    private void OnEnable()
    {
        Presenter.Bind("InvenPop",this);
    }
    private void OnDisable() 
    {
        Presenter.UnBind("InvenPop", this);
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
            case "Close":
                if (ShopInvenPop.isActive)
                {
                    UnityEngine.Debug.Log("ShopInvenPop이 활성화되어 있어서 InvenPop을 닫을 수 없습니다.");
                    return;
                }
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data){
        switch(key)
        {
            case "ClickObj":
                if(!MoveOn)
                {
                    string[] str = data.Get<string>().Split('_');
                    int xx = int.Parse(str[0]);
                    int yy = int.Parse(str[1]);
                    foreach(var v in curItem)
                    {
                        if(v.GetComponent<InvenItemObj>().x == xx && v.GetComponent<InvenItemObj>().y == yy)
                        {
                            curIdx = curItem.IndexOf(v);
                            break;
                        }
                    }
                    MoveOn = true;
                }
                else
                {
                    MoveOn = false;
                }
                break;
        }
    }
    public override void Refresh(){}
    private void InitGrid()
    {
        // 10x10 배열 초기화
        gridObj = new GameObject[10, 10];
        // grid_0_0부터 grid_9_9까지 찾아서 배열에 할당
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                string gridName = $"grid_{y}_{x}";
                Transform grid = slotParent.Find(gridName);
                gridObj[y, x] = grid.gameObject; // 그리드 이미지 배열에 할당
            }
        }
        pGrids = PlayerManager.I.grids;
    }
    private void LoadPlayerInven()
    {
        for(int i = 0; i < PlayerManager.I.pData.Inven.Count; i++)
        {
            // 현재 아이템의 크기 정보 가져오기 (예시)
            int w = PlayerManager.I.pData.Inven[i].W;  // 아이템 가로 크기
            int h = PlayerManager.I.pData.Inven[i].H; // 아이템 세로 크기
            
            // 아이템을 배치할 수 있는 위치 찾기
            bool placed = false;
            for (int gy = 0; gy < 10 && !placed; gy++)
            {
                for (int gx = 0; gx < 10 && !placed; gx++)
                {
                    // 현재 위치에서 아이템이 들어갈 수 있는지 검사
                    if (CanPlaceItem(gx, gy, w, h))
                    {
                        // 아이템 배치
                        PlaceItem(gx, gy, w, h, PlayerManager.I.pData.Inven[i]);
                        placed = true;
                    }
                }
            }
        }
    }
    private bool CanPlaceItem(int sx, int sy, int wid, int hei)
    {
        // 그리드 범위 체크
        if (sx + wid > 10 || sy + hei > 10)
            return false;
        
        // 해당 영역이 비어있는지 검사
        for (int y = sy; y < sy + hei; y++)
        {
            for (int x = sx; x < sx + wid; x++)
            {
                if(pGrids[y][x].slotId != -1)
                    return false;
            }
        }
        return true;
    }
    private void PlaceItem(int sx, int sy, int wid, int hei, ItemData item)
    {
        // itemPrefab을 인스턴스화
        GameObject itemObj = Instantiate(itemPrefab, eqParent);
        // 오브젝트 이름 설정
        itemObj.name = $"Item_{item.Name}";
        // InvenItemObj 스크립트 가져오기
        InvenItemObj invenItemObj = itemObj.GetComponent<InvenItemObj>();
        invenItemObj.SetItemData(item, sx, sy);
        // RectTransform 설정
        RectTransform rectTransform = itemObj.GetComponent<RectTransform>();
        float cx = gridObj[sy, sx].GetComponent<RectTransform>().anchoredPosition.x + ((wid - 1) * 32);
        float cy = gridObj[sy, sx].GetComponent<RectTransform>().anchoredPosition.y - ((hei - 1) * 32);
        // 그리드 크기에 맞춰 위치와 크기 설정
        rectTransform.anchoredPosition = new Vector2(cx, cy);
        rectTransform.sizeDelta = new Vector2(wid * 64, hei * 64);
        rectTransform.localScale = new Vector3(1, 1, 1);

        curItem.Add(itemObj);

        // 그리드 영역 체크
        for(int y = sy; y < sy + hei; y++)
        {
            for(int x = sx; x < sx + wid; x++)
            {
                pGrids[y][x].slotId = item.itemId;
            }
        }
    }
    private void CheckOverlapWithGrid()
    {
        resetAllGrids();

        
        RectTransform itemRect = curItem[curIdx].transform as RectTransform;
        float wid = itemRect.sizeDelta.x - 32, hei = itemRect.sizeDelta.y - 32;
        float minX = itemRect.position.x - wid/2, minY = itemRect.position.y - hei/2;
        float maxX = itemRect.position.x + wid/2, maxY = itemRect.position.y + hei/2;

        InvenItemObj itemObj = curItem[curIdx].GetComponent<InvenItemObj>();
        int myId = itemObj.itemData.itemId, maxCnt = itemObj.itemData.W * itemObj.itemData.H;
        int gx = 0, gy = 0;
        int[] gridX = new int[maxCnt], gridY = new int[maxCnt];
        List<int> gridItemId = new List<int>();
        for (int y = 0; y < 10; y++)
        {
            bool isOverlapping = false;
            for (int x = 0; x < 10; x++)
            {
                RectTransform gRect = gridObj[y, x].transform as RectTransform;
                float gw = gRect.sizeDelta.x, gh = gRect.sizeDelta.y;
                float gMinX = gRect.position.x - gw/2, gMinY = gRect.position.y - gh/2, gMaxX = gRect.position.x + gw/2, gMaxY = gRect.position.y + gh/2;
                isOverlapping = !(maxX < gMinX || minX > gMaxX || maxY < gMinY || minY > gMaxY);
                if (isOverlapping)
                {
                    gx = x;gy = y;
                    break;
                }
            }
            if (isOverlapping) break;
        }
        int cnt = 0;
        for(int h = 0; h < itemObj.itemData.H; h++)
        {
            for(int w = 0; w < itemObj.itemData.W; w++)
            {
                gridX[cnt] = gx + w;
                gridY[cnt] = gy + h;
                cnt++;
                
                if(gridItemId.IndexOf(PlayerManager.I.grids[gy + h][gx + w].slotId) == -1 && 
                PlayerManager.I.grids[gy + h][gx + w].slotId != -1 &&
                PlayerManager.I.grids[gy + h][gx + w].slotId != myId)
                {
                    gridItemId.Add(PlayerManager.I.grids[gy + h][gx + w].slotId);
                }
            }
        }
        // 색상 결정
        curType = gridItemId.Count == 0 ? 0 : (gridItemId.Count > 1 ? 2 : 1);
        // 색상 변경 함수
        void ChangeGridColor(int x, int y, int type)
        {
            var grid = gridObj[y, x].GetComponent<GridObj>();
            switch (type)
            {
                case 0: grid.ChangeToGreen(); break;
                case 1: grid.ChangeToYellow(); break;
                case 2: grid.ChangeToRed(); break;
            }
        }
        // 겹치는 그리드 색상 변경
        for(int i = 0; i < maxCnt; i++)
            ChangeGridColor(gridX[i], gridY[i], curType);
    }
    private void resetAllGrids()
    {
        for(int y = 0; y < 10; y++)
        {
            for(int x = 0; x < 10; x++)
                gridObj[y, x].GetComponent<GridObj>().ChangeToWhite();
        }
    }
    private void Update()
    {
        if(MoveOn)
        {
            curItem[curIdx].transform.position = Input.mousePosition;
            CheckOverlapWithGrid();
        }
    }
}