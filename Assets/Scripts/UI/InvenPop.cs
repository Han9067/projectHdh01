using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.UI;

public class InvenPop : UIScreen
{
    class EdgePos{
        public float u, d, l, r;
        public EdgePos(float u, float d, float l, float r){this.u = u;this.d = d;this.l = l;this.r = r;}
    }
    [SerializeField] private Transform slotParent; // Slot 게임오브젝트
    [SerializeField] private Transform eqParent; // 장비 게임오브젝트
    [SerializeField] private Transform itemParent; // 아이템 게임오브젝트
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject popPrefab;
    public GameObject[,] gridObj;
    private List<List<InvenGrid>> pGrids;
    private List<GameObject> curItem = new List<GameObject>();
    private bool MoveOn = false;
    private int curIdx = 0, curState = 0;
    private int edgeCnt = 0;
    private string[] curEq;
    private float[] popEdge = new float[4];
    private float[] slotEdge = new float[4];
    private Dictionary<string,EdgePos> eqEdge = new Dictionary<string,EdgePos>();
    private void Awake()
    {
        Regist();
        RegistButton();
        RegistEqEdge();
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
    public void RegistEqEdge()
    {
        string[] str = {"Hand1Box","Hand2Box","ArmorBox","ShoesBox","CapeBox","HelmetBox","GlovesBox","BeltBox","NeckBox","Ring1Box","Ring2Box"};
        foreach(var v in str)
        {
            RectTransform rt = mGameObject[v].transform.GetComponent<RectTransform>();
            float w = rt.sizeDelta.x / 2, h = rt.sizeDelta.y / 2;
            eqEdge[v] = new EdgePos(rt.position.y + h, rt.position.y - h, rt.position.x - w, rt.position.x + w);
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
                if(!MoveOn)
                {
                    string[] str = data.Get<string>().Split('_');
                    int xx = int.Parse(str[0]), yy = int.Parse(str[1]);
                    int id = int.Parse(str[2]), type = int.Parse(str[3]);
                    if(id > 40000){
                        edgeCnt = 0;
                    }else if(id < 20000){
                        switch(type){
                            case 10:
                                curEq = new string[] {"Hand2"};
                                edgeCnt = 1;
                                break;
                            default:
                                curEq = new string[] {"Hand1","Hand2"};
                                edgeCnt = 2;
                                break;
                        }
                    }else{
                        edgeCnt = 1;
                        switch(type){
                            case 1:curEq = new string[] {"Armor"};break;
                            case 2:curEq = new string[] {"Helmet"};break;
                            case 3:curEq = new string[] {"Shoes"};break;
                            case 4:curEq = new string[] {"Gloves"};break;
                            case 5:curEq = new string[] {"Belt"};break;
                            case 6:curEq = new string[] {"Cape"};break;
                            case 7:curEq = new string[] {"Ring1","Ring2"};break;
                            case 8:curEq = new string[] {"Neck"};break;
                            case 9:
                                curEq = new string[] {"Hand1","Hand2"};
                                edgeCnt = 2;
                                break; //방패
                        }
                    }
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
                    resetAllGrids();
                    MoveOn = false;
                    // Debug.Log(curState);
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
        RectTransform grid00 = gridObj[0, 0].GetComponent<RectTransform>();
        RectTransform grid99 = gridObj[9, 9].GetComponent<RectTransform>();

        slotEdge[0] = grid00.position.y + grid00.sizeDelta.y / 2;slotEdge[1] = grid99.position.y - grid99.sizeDelta.y / 2;
        slotEdge[2] = grid00.position.x - grid00.sizeDelta.x / 2;slotEdge[3] = grid99.position.x + grid99.sizeDelta.x / 2;

        RectTransform pop = popPrefab.GetComponent<RectTransform>();
        popEdge[0] = pop.position.y + pop.sizeDelta.y / 2;popEdge[1] = pop.position.y - pop.sizeDelta.y / 2;
        popEdge[2] = pop.position.x - pop.sizeDelta.x / 2;popEdge[3] = pop.position.x + pop.sizeDelta.x / 2;
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
        GameObject itemObj = Instantiate(itemPrefab, itemParent);
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
                pGrids[y][x].slotId = item.ItemId;
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
        int myId = itemObj.itemData.ItemId, maxCnt = itemObj.itemData.W * itemObj.itemData.H;
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

                if(gx + w > 9 || gy + h > 9)return; // 그리드 범위 초과시 리턴

                if(gridItemId.IndexOf(PlayerManager.I.grids[gy + h][gx + w].slotId) == -1 && 
                PlayerManager.I.grids[gy + h][gx + w].slotId != -1 &&
                PlayerManager.I.grids[gy + h][gx + w].slotId != myId)
                {
                    gridItemId.Add(PlayerManager.I.grids[gy + h][gx + w].slotId);
                }
            }
        }
        // 색상 결정
        curState = gridItemId.Count == 0 ? 0 : (gridItemId.Count > 1 ? 2 : 1);
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
            ChangeGridColor(gridX[i], gridY[i], curState);
    }
    private void resetAllGrids()
    {
        for(int y = 0; y < 10; y++)
        {
            for(int x = 0; x < 10; x++)
                gridObj[y, x].GetComponent<GridObj>().ChangeToWhite();
        }
    }
    private void resetAllEq()
    {
        foreach(var v in curEq)
        {
            
        }
    }
    private void Update()
    {
        if(MoveOn)
        {
            curItem[curIdx].transform.position = Input.mousePosition; // 아이템 위치 업데이트

            if(curItem[curIdx].transform.position.x <= popEdge[2] || curItem[curIdx].transform.position.x >= popEdge[3] ||
            curItem[curIdx].transform.position.y <= popEdge[1] || curItem[curIdx].transform.position.y >= popEdge[0])
            {
                Debug.Log("outside pop");
            }else{
                float x = curItem[curIdx].transform.position.x, y = curItem[curIdx].transform.position.y;
                if(y <= slotEdge[0] && y >= slotEdge[1] && x >= slotEdge[2] && x <= slotEdge[3])
                {
                    CheckOverlapWithGrid();
                }else{
                    bool on = false;
                    for(int i = 0; i < edgeCnt; i++)
                    {
                        if(curItem[curIdx].transform.position.y >= eqEdge[curEq[i]].d && curItem[curIdx].transform.position.y <= eqEdge[curEq[i]].u &&
                        curItem[curIdx].transform.position.x >= eqEdge[curEq[i]].l && curItem[curIdx].transform.position.x <= eqEdge[curEq[i]].r)
                        {
                            on = true;
                            Debug.Log("on");
                            break;
                        }
                    }
                    if(!on)
                        resetAllGrids();
                }
            }
        }
    }
}