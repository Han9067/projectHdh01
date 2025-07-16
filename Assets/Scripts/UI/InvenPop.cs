using System.Collections.Generic;
using System.Xml.Serialization;
using GB;
using UnityEngine;
using UnityEngine.UI;
public class InvenPop : UIScreen
{
    class EdgePos{
        public float u, d, l, r;
        public EdgePos(float u, float d, float l, float r){this.u = u;this.d = d;this.l = l;this.r = r;}
    }
    [SerializeField] private Transform slotParent; // 슬롯 부모
    [SerializeField] private Transform eqParent; // 장비 부모
    [SerializeField] private Transform itemParent; // 아이템 부모
    [SerializeField] private GameObject itemPrefab; // 아이템 프리팹
    [SerializeField] private GameObject popPrefab; // 팝업 프리팹
    public Image[,] gridObj; // 그리드 게임오브젝트
    private List<List<InvenGrid>> pGrids; // 플레이어 그리드
    private List<GameObject> curItem = new List<GameObject>(); // 현재 선택된 아이템
    private bool moveOn = false; // 이동 중인지 체크
    private int curIdx = 0, curState = 0; // 현재 선택된 아이템 인덱스, 상태
    private InvenItemObj curItemObj; // 현재 선택된 아이템의 오브젝트 스크립트
    private string[] curEq; // 현재 선택된 장비
    private int curItemX, curItemY; // 현재 선택된 아이템의 위치
    private float[] popEdge = new float[4]; // 팝업 경계
    private float[] slotEdge = new float[4]; // 슬롯 경계
    private Dictionary<string,EdgePos> eqEdge = new Dictionary<string,EdgePos>(); // 장비 경계
    private Sprite whiteGrid; // 클래스 멤버 변수로 캐싱
    private Sprite grayGrid; // 클래스 멤버 변수로 캐싱
    private void Awake()
    {
        Regist();
        RegistButton();
        RegistEqEdge();
        whiteGrid = ResManager.GetSprite("ui_grid_white");
        grayGrid = ResManager.GetSprite("ui_grid_gray");
    }
    private void Start()
    {
        InitGrid();
        LoadInven();
    }
    private void OnEnable()
    {
        Presenter.Bind("InvenPop",this);
    }
    private void OnDisable() 
    {
        ResetAllEq();
        ResetAllGrids();
        if(moveOn){
            //강제 팝업 종료시 대응
            moveOn = false;
            ReturnItem();
        }
        InitCurData();
        Presenter.UnBind("InvenPop", this);
    }
    public void RegistButton()
    {
        foreach(var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key);});
    }
    public void RegistEqEdge()
    {
        foreach(var v in mImages)
        {
            RectTransform rt = v.Value.rectTransform;
            float w = rt.sizeDelta.x / 2, h = rt.sizeDelta.y / 2;
            eqEdge[v.Key] = new EdgePos(rt.position.y + h, rt.position.y - h, rt.position.x - w, rt.position.x + w);
        }
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
                if(!moveOn)
                {
                    if(curItemObj != null) curItemObj = null;
                    string[] str = data.Get<string>().Split('_');
                    int uid = int.Parse(str[0]), id = int.Parse(str[1]), type = int.Parse(str[2]);
                    CheckCurEq(id, type);
                    foreach(var v in curItem)
                    {
                        InvenItemObj iObj = v.GetComponent<InvenItemObj>();
                        if(iObj.uid == uid)
                        {
                            curIdx = curItem.IndexOf(v);
                            curItemObj = iObj;
                            break;
                        }
                    }
                    StateItems(0, curIdx);
                    moveOn = true;
                }
                else
                {   
                    ResetAllGrids();
                    ResetAllEq();
                    moveOn = false;
                    switch(curState)
                    {
                        case 0:
                            MoveItem(curItemObj.x, curItemObj.y, curItemObj.itemData.W, curItemObj.itemData.H, curItemX, curItemY);
                            InitCurData();
                            break;
                        case 1:
                            ChangeItem(curItemObj.x, curItemObj.y, curItemObj.itemData.W, curItemObj.itemData.H, curItemX, curItemY);
                            break;
                        case 2:
                            InitCurData();
                            return; // 겹치는 아이템이 여러 개이며 이동이 불가능
                        case 3:
                            bool on = false;
                            for(int i = 0; i < curEq.Length; i++)
                            {
                            }
                            if(!on) ReturnItem();
                            InitCurData();
                            break; // 아이템 그리드 영역 밖에 있을때 또는 장비 칸 영역에 있을때
                    }
                    // if(!UIManager.I.canClose) UIManager.I.canClose = true;
                }
                break;
        }
    }
    public override void Refresh(){}
    private void InitCurData()
    {
        curEq = new string[] {}; curIdx = -1;
        curItemX = -1; curItemY = -1; curState = 0;
        curItemObj = null;
        StateItems(1, 0);
    }
    private void InitGrid()
    {
        // 10x10 배열 초기화
        gridObj = new Image[10, 10];
        // grid_0_0부터 grid_9_9까지 찾아서 배열에 할당
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                string gridName = $"grid_{y}_{x}";
                Transform grid = slotParent.Find(gridName);
                gridObj[y, x] = grid.GetComponent<Image>(); // 그리드 이미지 배열에 할당
            }
        }
        pGrids = PlayerManager.I.grids;
        RectTransform grid00 = gridObj[0, 0].rectTransform;
        RectTransform grid99 = gridObj[9, 9].rectTransform;

        slotEdge[0] = grid00.position.y + grid00.sizeDelta.y / 2;slotEdge[1] = grid99.position.y - grid99.sizeDelta.y / 2;
        slotEdge[2] = grid00.position.x - grid00.sizeDelta.x / 2;slotEdge[3] = grid99.position.x + grid99.sizeDelta.x / 2;

        RectTransform pop = popPrefab.GetComponent<RectTransform>();
        popEdge[0] = pop.position.y + pop.sizeDelta.y / 2;popEdge[1] = pop.position.y - pop.sizeDelta.y / 2;
        popEdge[2] = pop.position.x - pop.sizeDelta.x / 2;popEdge[3] = pop.position.x + pop.sizeDelta.x / 2;
    }
    private void LoadInven()
    {
        for(int i = 0; i < PlayerManager.I.pData.Inven.Count; i++)
        {
            // 현재 아이템의 크기 정보 가져오기 (예시)
            int w = PlayerManager.I.pData.Inven[i].W;  // 아이템 가로 크기
            int h = PlayerManager.I.pData.Inven[i].H; // 아이템 세로 크기
            PlaceItem(PlayerManager.I.pData.Inven[i].X, PlayerManager.I.pData.Inven[i].Y, w, h, PlayerManager.I.pData.Inven[i]);
            //추후 아이템 중복에 대한 검사가 필요할것같음
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
    private void SetGridPos(RectTransform rect, int type, int x, int y, int w, int h)
    {
        float cx = gridObj[y, x].rectTransform.anchoredPosition.x + ((w - 1) * 32);
        float cy = gridObj[y, x].rectTransform.anchoredPosition.y - ((h - 1) * 32);
        rect.anchoredPosition = new Vector2(cx, cy);
        rect.sizeDelta = new Vector2(w * 64, h * 64);
        if(type == 0)rect.localScale = new Vector3(1, 1, 1);
    }
    private void PlaceItem(int sx, int sy, int wid, int hei, ItemData item)
    {
        // itemPrefab을 인스턴스화
        GameObject itemObj = Instantiate(itemPrefab, itemParent);
        // 오브젝트 이름 설정
        itemObj.name = $"Item_{item.Name}";
        // InvenItemObj 스크립트 가져오기
        InvenItemObj invenItemObj = itemObj.GetComponent<InvenItemObj>();
        invenItemObj.SetItemData(item, sx, sy);
        // RectTransform 설정
        RectTransform rectTransform = itemObj.transform as RectTransform;
        SetGridPos(rectTransform, 0, sx, sy, wid, hei);
        curItem.Add(itemObj);

        // 그리드 영역 체크
        for(int y = sy; y < sy + hei; y++)
        {
            for(int x = sx; x < sx + wid; x++)
            {
                pGrids[y][x].slotId = item.Uid;
            }
        }
    }
    private void CheckOverlapWithGrid()
    {
        ResetAllGrids(); // 그리드 초기화
        
        RectTransform itemRect = curItem[curIdx].transform as RectTransform;
        float wid = itemRect.sizeDelta.x - 32, hei = itemRect.sizeDelta.y - 32;
        float minX = itemRect.position.x - wid/2, minY = itemRect.position.y - hei/2;
        float maxX = itemRect.position.x + wid/2, maxY = itemRect.position.y + hei/2;

        int myUid = curItemObj.itemData.Uid, maxCnt = curItemObj.itemData.W * curItemObj.itemData.H;
        int gx = 0, gy = 0;
        int[] gridX = new int[maxCnt], gridY = new int[maxCnt];
        List<int> gridUid = new List<int>();
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
        for(int h = 0; h < curItemObj.itemData.H; h++)
        {
            for(int w = 0; w < curItemObj.itemData.W; w++)
            {
                gridX[cnt] = gx + w;
                gridY[cnt] = gy + h;
                cnt++;

                if(gx + w > 9 || gy + h > 9)return; // 그리드 범위 초과시 리턴

                if(gridUid.IndexOf(PlayerManager.I.grids[gy + h][gx + w].slotId) == -1 && 
                PlayerManager.I.grids[gy + h][gx + w].slotId != -1 &&
                PlayerManager.I.grids[gy + h][gx + w].slotId != myUid)
                {
                    gridUid.Add(PlayerManager.I.grids[gy + h][gx + w].slotId);
                }
            }
        }
        curItemX = gx; curItemY = gy; // 현재 선택된 아이템의 위치 설정
        curState = gridUid.Count == 0 ? 0 : (gridUid.Count > 1 ? 2 : 1); // 상태 설정
        void ChangeGridColor(int x, int y, int type)
        {
            switch(type)
            {
                case 0: gridObj[y, x].sprite = ResManager.GetSprite("ui_grid_green"); break;
                case 1: gridObj[y, x].sprite = ResManager.GetSprite("ui_grid_yellow"); break;
                case 2: gridObj[y, x].sprite = ResManager.GetSprite("ui_grid_red"); break;
            }
        }
        // 겹치는 그리드 색상 변경
        for(int i = 0; i < maxCnt; i++)
            ChangeGridColor(gridX[i], gridY[i], curState);
    }
    private void ResetAllGrids()
    {
        for(int y = 0; y < 10; y++)
        {
            for(int x = 0; x < 10; x++){
                if(gridObj[y, x].sprite != whiteGrid)
                    gridObj[y, x].sprite = whiteGrid;
            }
        }
    }
    private void ResetAllEq()
    {
        foreach(var v in mImages)
        {
            if(v.Value.sprite != grayGrid)
                v.Value.sprite = grayGrid;
        }
    }
    private void CheckCurEq(int id, int type)
    {
        if (id > 40000) {
            curEq = new string[] {};
        } else if (id > 20000) {
            curEq = (type == 10) ? new string[] { "Hand2Box" } : new string[] { "Hand1Box", "Hand2Box" };
        } else {
            switch (type) {
                case 1:  curEq = new string[] { "ArmorBox" }; break;
                case 2:  curEq = new string[] { "HelmetBox" }; break;
                case 3:  curEq = new string[] { "ShoesBox" }; break;
                case 4:  curEq = new string[] { "GlovesBox" }; break;
                case 5:  curEq = new string[] { "BeltBox" }; break;
                case 6:  curEq = new string[] { "CapeBox" }; break;
                case 7:  curEq = new string[] { "Ring1Box", "Ring2Box" }; break;
                case 8:  curEq = new string[] { "NeckBox" }; break;
                case 9:  curEq = new string[] { "Hand1Box", "Hand2Box" }; break; //방패
            }
        }
        foreach(var v in curEq)
            mImages[v].sprite = ResManager.GetSprite("ui_grid_green");
    }
    private void StateItems(int type, int n)
    {
        //type 0 : 블럭, 1 : 언블럭
        for(int i = 0; i < curItem.Count; i++)
            curItem[i].GetComponent<Image>().raycastTarget = type == 0 ? false : true;
        if(type == 0)
            curItem[n].GetComponent<Image>().raycastTarget = true;
    }
    private void MoveItem(int sx, int sy, int w, int h, int ex, int ey)
    {
        for(int y = sy; y < sy + h; y++)
        {
            for(int x = sx; x < sx + w; x++)
                pGrids[y][x].slotId = -1;
        }
        RectTransform rect = curItem[curIdx].transform as RectTransform;
        SetGridPos(rect, 1, ex, ey, w, h);

        int uid = curItemObj.itemData.Uid;
        for(int y = ey; y < ey + h; y++)
        {
            for(int x = ex; x < ex + w; x++)
                pGrids[y][x].slotId = uid;
        }
        InvenItemObj iObj = curItem[curIdx].GetComponent<InvenItemObj>();
        iObj.x = ex; iObj.y = ey;
    }

    private void ChangeItem(int sx, int sy, int w, int h, int ex, int ey)
    {
        int uid = pGrids[ey][ex].slotId;
        MoveItem(sx, sy, w, h, ex, ey);
        foreach(var v in curItem)
        {
            InvenItemObj iObj = v.GetComponent<InvenItemObj>();
            if(iObj.uid == uid)
            {
                iObj.DelaySendItemObj();
            }
        }
        // foreach(var v in curItem)
        // {
        //     InvenItemObj iObj = v.GetComponent<InvenItemObj>();
        //     if(iObj.uid == uid)
        //     {
        //         CheckCurEq(iObj.itemData.ItemId, iObj.itemData.Type);
        //         curIdx = curItem.IndexOf(v);
        //         curItemObj = iObj;
        //         break;
        //     }
        // }
        // StateItems(0, curIdx);  
    }
    private void ReturnItem()
    {
        RectTransform rect = curItem[curIdx].transform as RectTransform;
        SetGridPos(rect, 1, curItemObj.itemData.X, curItemObj.itemData.Y, curItemObj.itemData.W, curItemObj.itemData.H);
    }
    private void Update()
    {
        if(moveOn)
        {
            curItem[curIdx].transform.position = Input.mousePosition; // 아이템 위치 업데이트
            float x = curItem[curIdx].transform.position.x, y = curItem[curIdx].transform.position.y;
            if(x <= popEdge[2] || x >= popEdge[3] || y <= popEdge[1] || y >= popEdge[0])
            {
                curState = 3;
            }else{
                if(y <= slotEdge[0] && y >= slotEdge[1] && x >= slotEdge[2] && x <= slotEdge[3])
                {
                    CheckOverlapWithGrid();
                }else{
                    curState = 3;
                    ResetAllGrids();
                }
            }
        }
    }
}