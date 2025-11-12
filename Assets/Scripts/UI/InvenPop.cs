using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using GB;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class InvenPop : UIScreen
{
    class EdgePos
    {
        public float u, d, l, r;
        public EdgePos(float u, float d, float l, float r) { this.u = u; this.d = d; this.l = l; this.r = r; }
    }
    [SerializeField] private Transform slotParent; // 슬롯 부모
    [SerializeField] private Transform eqParent; // 장비 부모
    [SerializeField] private Transform itemParent; // 아이템 부모
    [SerializeField] public GameObject popPrefab; // 팝업 프리팹
    public Image[,] gridObj; // 그리드 게임오브젝트
    private List<List<InvenGrid>> pGrids; // 플레이어 그리드
    private List<GameObject> curItem = new List<GameObject>(); // 현재 선택된 아이템
    private bool moveOn = false; // 이동 중인지 체크
    private int curIdx = -1, curState = 0; // 현재 선택된 아이템 인덱스, 상태
    private ItemObj curItemObj; // 현재 선택된 아이템의 오브젝트 스크립트
    private string[] curEq; // 현재 선택된 장비
    private int curItemX, curItemY; // 현재 선택된 아이템의 위치
    private float[] popEdge = new float[4]; // 팝업 경계
    private float[] slotEdge = new float[4]; // 슬롯 경계
    private Dictionary<string, EdgePos> eqEdge = new Dictionary<string, EdgePos>(); // 장비 경계
    private Dictionary<string, Image> eqBox = new Dictionary<string, Image>(); // 장비 슬롯 박스 이미지
    private Dictionary<string, GameObject> eqMain = new Dictionary<string, GameObject>(); // 장비 슬롯 메인 이미지
    public Sprite gridSprite; // 클래스 멤버 변수로 캐싱
    public static bool isActive = false;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        InitGrid();
        LoadInven();
    }
    private void OnEnable()
    {
        Presenter.Bind("InvenPop", this);
        isActive = true;
        if (SkillPop.isActive)
            UIManager.ClosePopup("SkillPop");
    }
    private void OnDisable()
    {
        isActive = false;
        ResetAllEq();
        ResetAllGrids();
        if (moveOn)
        {
            //강제 팝업 종료시 대응
            moveOn = false;
            ReturnItem();
        }
        InitCurData();
        if (ShopInvenPop.isActive)
            UIManager.ClosePopup("ShopInvenPop");
        Presenter.UnBind("InvenPop", this);
    }
    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
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
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "ClickObj":
                if (!moveOn)
                {
                    InitCurData();
                    string[] str = data.Get<string>().Split('_');
                    int uid = int.Parse(str[0]), id = int.Parse(str[1]), type = int.Parse(str[2]);
                    CheckCurEq(id, type);
                    foreach (var v in curItem)
                    {
                        ItemObj iObj = v.GetComponent<ItemObj>();
                        if (iObj.uid == uid)
                        {
                            curIdx = curItem.IndexOf(v);
                            curItemObj = iObj;
                            curItemObj.transform.SetAsLastSibling();
                            curItemObj.transform.position = Input.mousePosition;
                            break;
                        }
                    }
                    moveOn = true;
                    //클릭된 아이템이 장착 중이였다면 장착 해제가 될텐데 그떄, 해당 장착칸이 비어있다면 초기화를 해준다.
                    if (curItemObj.eq != "")
                    {
                        if (PlayerManager.I.pData.EqSlot[curItemObj.eq] != null &&
                        PlayerManager.I.pData.EqSlot[curItemObj.eq].Uid == curItemObj.uid)
                        {
                            PlayerManager.I.TakeoffEq(curItemObj.eq);
                            eqMain[curItemObj.eq].SetActive(true);
                        }
                        curItemObj.eq = "";
                    }
                    CheckOverlapWithGrid();
                }
                else
                {
                    ResetAllGrids();
                    ResetAllEq();
                    moveOn = false;
                    switch (curState)
                    {
                        case 0:
                            MoveItem(curItemObj.x, curItemObj.y, curItemObj.itemData.W, curItemObj.itemData.H, curItemX, curItemY);
                            break;
                        case 1:
                            ChangeItem(curItemObj.x, curItemObj.y, curItemObj.itemData.W, curItemObj.itemData.H, curItemX, curItemY);
                            return;
                        case 2:
                            return; // 겹치는 아이템이 여러 개이며 이동이 불가능
                        case 3:
                            bool on = false;
                            float x = curItem[curIdx].transform.position.x, y = curItem[curIdx].transform.position.y;
                            for (int i = 0; i < curEq.Length; i++)
                            {
                                if (eqEdge[curEq[i]].u >= y && eqEdge[curEq[i]].d <= y && eqEdge[curEq[i]].l <= x && eqEdge[curEq[i]].r >= x)
                                {
                                    on = true;
                                    //선택 아이템 장착
                                    EquipItem(curEq[i], curItemObj.x, curItemObj.y, curItemObj.itemData.W, curItemObj.itemData.H);
                                    break;
                                }
                            }
                            if (!on) ReturnItem();
                            break; // 아이템 그리드 영역 밖에 있을때 또는 장비 칸 영역에 있을때
                    }
                }
                break;
            case "AddItem":
                ItemData addItem = data.Get<ItemData>();
                PlaceInvenItem(addItem.X, addItem.Y, addItem.W, addItem.H, addItem, false);
                break;
        }
    }
    public override void Refresh() { }
    private void InitCurData()
    {
        curEq = new string[] { }; curIdx = -1;
        curItemX = -1; curItemY = -1; curState = 0;
        if (curItemObj != null) curItemObj = null;
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
        slotEdge[0] = grid00.position.y + grid00.sizeDelta.y / 2; slotEdge[1] = grid99.position.y - grid99.sizeDelta.y / 2;
        slotEdge[2] = grid00.position.x - grid00.sizeDelta.x / 2; slotEdge[3] = grid99.position.x + grid99.sizeDelta.x / 2;

        RectTransform pop = popPrefab.GetComponent<RectTransform>();
        popEdge[0] = pop.position.y + pop.sizeDelta.y / 2; popEdge[1] = pop.position.y - pop.sizeDelta.y / 2;
        popEdge[2] = pop.position.x - pop.sizeDelta.x / 2; popEdge[3] = pop.position.x + pop.sizeDelta.x / 2;

        string[] eq = new string[] { "Hand1", "Hand2", "Armor", "Helmet", "Shoes", "Gloves", "Belt", "Cape", "Necklace", "Ring1", "Ring2" };
        foreach (var v in eq)
        {
            eqBox[v] = mGameObject[v].transform.Find("Box").GetComponent<Image>();
            eqMain[v] = mGameObject[v].transform.Find("Main").gameObject;
            RectTransform rt = eqBox[v].rectTransform;
            float w = rt.sizeDelta.x / 2, h = rt.sizeDelta.y / 2;
            eqEdge[v] = new EdgePos(rt.position.y + h, rt.position.y - h, rt.position.x - w, rt.position.x + w);
        }
    }
    private void LoadInven()
    {
        for (int i = 0; i < PlayerManager.I.pData.Inven.Count; i++)
        {
            ItemData item = PlayerManager.I.pData.Inven[i];
            PlaceInvenItem(item.X, item.Y, item.W, item.H, item, item.X == -1 && item.Y == -1);
        }
        curItemObj = null;
        curIdx = -1;
    }
    string GetEquipBody(int uId)
    {
        string result = "";
        string[] eq = new string[] { "Hand1", "Hand2", "Armor", "Helmet", "Shoes", "Gloves", "Belt", "Cape", "Necklace", "Ring1", "Ring2" };
        foreach (var v in eq)
        {
            if (PlayerManager.I.pData.EqSlot[v] != null && PlayerManager.I.pData.EqSlot[v].Uid == uId)
            {
                result = v;
                break;
            }
        }
        return result;
    }
    private void SetGridPos(RectTransform rect, int type, int x, int y, int w, int h)
    {
        if (x > -1 && y > -1)
        {
            float cx = gridObj[y, x].rectTransform.anchoredPosition.x + ((w - 1) * 32);
            float cy = gridObj[y, x].rectTransform.anchoredPosition.y - ((h - 1) * 32) + slotParent.localPosition.y;
            rect.anchoredPosition = new Vector2(cx, cy);
        }
        rect.sizeDelta = new Vector2(w * 64, h * 64);
        if (type == 0) rect.localScale = new Vector3(1, 1, 1);
    }
    private void PlaceInvenItem(int sx, int sy, int wid, int hei, ItemData item, bool isEq)
    {
        GameObject itemObj = Instantiate(ResManager.GetGameObject("ItemObj"), itemParent);
        itemObj.name = $"Item_{item.Name}";
        ItemObj ItemObj = itemObj.GetComponent<ItemObj>();
        ItemObj.SetItemData(item, sx, sy, 0);
        RectTransform rectTransform = itemObj.transform as RectTransform;
        SetGridPos(rectTransform, 0, sx, sy, wid, hei);
        curItem.Add(itemObj);
        if (isEq)
        {
            string eq = GetEquipBody(item.Uid);
            PlayerManager.I.pData.EqSlot[eq] = null;
            curItemObj = ItemObj;
            curIdx = curItem.Count - 1;
            EquipItem(eq, item.X, item.Y, item.W, item.H);
        }
        else
        {
            for (int y = sy; y < sy + hei; y++)
            {
                for (int x = sx; x < sx + wid; x++)
                {
                    pGrids[y][x].slotId = item.Uid;
                }
            }
        }
    }
    private void CheckOverlapWithGrid()
    {
        ResetAllGrids(); // 그리드 초기화

        RectTransform itemRect = curItem[curIdx].transform as RectTransform;
        float wid = itemRect.sizeDelta.x - 32, hei = itemRect.sizeDelta.y - 32;
        float minX = itemRect.position.x - wid / 2, minY = itemRect.position.y - hei / 2;
        float maxX = itemRect.position.x + wid / 2, maxY = itemRect.position.y + hei / 2;

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
                float gMinX = gRect.position.x - gw / 2, gMinY = gRect.position.y - gh / 2, gMaxX = gRect.position.x + gw / 2, gMaxY = gRect.position.y + gh / 2;
                isOverlapping = !(maxX < gMinX || minX > gMaxX || maxY < gMinY || minY > gMaxY);
                if (isOverlapping)
                {
                    gx = x; gy = y;
                    break;
                }
            }
            if (isOverlapping) break;
        }
        int cnt = 0;
        for (int h = 0; h < curItemObj.itemData.H; h++)
        {
            for (int w = 0; w < curItemObj.itemData.W; w++)
            {
                gridX[cnt] = gx + w;
                gridY[cnt] = gy + h;
                cnt++;

                if (gx + w > 9 || gy + h > 9) return; // 그리드 범위 초과시 리턴

                if (gridUid.IndexOf(PlayerManager.I.grids[gy + h][gx + w].slotId) == -1 &&
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
            switch (type)
            {
                case 0: gridObj[y, x].color = new Color(129 / 255f, 183 / 255f, 127 / 255f, 1); break; // 초록색
                case 1: gridObj[y, x].color = new Color(255 / 255f, 250 / 255f, 101 / 255f, 1); break; // 노란색
                case 2: gridObj[y, x].color = new Color(255 / 255f, 105 / 255f, 101 / 255f, 1); break; // 빨간색
            }
        }
        // 겹치는 그리드 색상 변경
        for (int i = 0; i < maxCnt; i++)
            ChangeGridColor(gridX[i], gridY[i], curState);
    }
    private void ResetAllGrids()
    {
        Color whiteColor = Color.white;
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (gridObj[y, x].color != whiteColor)
                    gridObj[y, x].color = whiteColor;
            }
        }
    }
    private void ResetAllEq()
    {
        Color grayColor = new Color(192 / 255f, 192 / 255f, 192 / 255f, 1);
        foreach (var v in eqBox)
        {
            if (v.Value.color != grayColor)
                v.Value.color = grayColor;
        }
    }
    private void CheckCurEq(int id, int type)
    {
        if (id > 60000)
        {
            curEq = new string[] { };
        }
        else if (id > 30000)
        {
            curEq = (type == 10) ? new string[] { "Hand2" } : new string[] { "Hand1", "Hand2" };
        }
        else
        {
            switch (type)
            {
                case 1: curEq = new string[] { "Armor" }; break;
                case 2: curEq = new string[] { "Helmet" }; break;
                case 3: curEq = new string[] { "Shoes" }; break;
                case 4: curEq = new string[] { "Gloves" }; break;
                case 5: curEq = new string[] { "Belt" }; break;
                case 6: curEq = new string[] { "Cape" }; break;
                case 7: curEq = new string[] { "Ring1", "Ring2" }; break;
                case 8: curEq = new string[] { "Necklace" }; break;
                case 9: curEq = new string[] { "Hand1", "Hand2" }; break; //방패
            }
        }

        foreach (var v in curEq)
            eqBox[v].color = new Color(129 / 255f, 183 / 255f, 127 / 255f, 1);
    }
    private void MoveItem(int sx, int sy, int w, int h, int ex, int ey)
    {
        ItemObj iObj = curItem[curIdx].GetComponent<ItemObj>();
        if (iObj.x > -1 && iObj.y > -1)
        {
            for (int y = sy; y < sy + h; y++)
            {
                for (int x = sx; x < sx + w; x++)
                    pGrids[y][x].slotId = -1;
            }
        }

        RectTransform rect = curItem[curIdx].transform as RectTransform;
        SetGridPos(rect, 1, ex, ey, w, h);

        int uid = curItemObj.itemData.Uid;
        for (int y = ey; y < ey + h; y++)
        {
            for (int x = ex; x < ex + w; x++)
                pGrids[y][x].slotId = uid;
        }
        iObj.x = ex; iObj.y = ey; iObj.eq = "";
    }
    private void ChangeItem(int sx, int sy, int w, int h, int ex, int ey)
    {
        int uId = 0;
        for (int y = ey; y < ey + h; y++)
        {
            for (int x = ex; x < ex + w; x++)
            {
                if (pGrids[y][x].slotId != -1)
                {
                    uId = pGrids[y][x].slotId;
                    break;
                }
            }
        }
        ItemObj otherItem = GetItem(uId);
        int ox = otherItem.x, oy = otherItem.y;
        int ow = otherItem.itemData.W, oh = otherItem.itemData.H;
        for (int y = oy; y < oy + oh; y++)
        {
            for (int x = ox; x < ox + ow; x++)
                pGrids[y][x].slotId = -1;
        }
        otherItem.x = -1; otherItem.y = -1; // 원래 위치에 있던 아이템을 이동한 아이템의 위치로 설정
        MoveItem(sx, sy, w, h, ex, ey); // 이동 예정인 아이템을 이동

        Presenter.Send("InvenPop", "ClickObj", $"{otherItem.uid}_{otherItem.itemData.ItemId}_{otherItem.itemData.Type}"); //send 하여 원래 위치한 아이템을 움직이도록 전달
    }
    private void ReturnItem()
    {
        RectTransform rect = curItem[curIdx].transform as RectTransform;
        SetGridPos(rect, 1, curItemObj.itemData.X, curItemObj.itemData.Y, curItemObj.itemData.W, curItemObj.itemData.H);
    }
    private void EquipItem(string eq, int sx, int sy, int w, int h)
    {
        ///한손이든 양손이든 한칸에만 적용시킬지 아니면 양손이면 두칸 한손이면 정해진 한칸 이렇게 적용할지 고민좀...ㅎㅎ
        int eqState = 0;
        // 0 : 비어있으며 해당 칸에만 적용되는 한손 무기 및 일반 장비
        // 1 : 비어있지 않으며 해당 장비칸에만 적용되는 한손 무기 및 일반 장비
        // 2 : 손1,손2만 해당 => 둘 다 비어있으며 해당 무기가 양손일 경우
        // 3 : 손1,손2만 해당 => 둘 중 한 곳이 한손무기이고 해당 무기가 양손일 경우(장착된 한손 무기를 제거하고 양손무기 장착)
        // 4 : 손1,손2만 해당 => 장착된 무기가 양손무기이고 해당 무기가 한손일 경우(장착된 양손 무기를 제거하고 해당 칸에 한손무기 장착)
        // 5 : 손1,손2만 해당 => 장착된 무기가 양손무기이고 해당 무기가 양손일 경우(장착된 양손 무기를 제거하고 해당 무기를 장착)
        // 6 : 손1,손2만 해당 => 장착된 무기가 한손무기 두 개이며 해당 무기가 양손일 경우(해당 무기를 장착하지 못하게 예외처리) 
        //손1, 손2 착용된 장비에 대한 체크
        switch (eq)
        {
            case "Hand1":
            case "Hand2":
                //해당 무기가 한손무기인지 양손무기인지 체크
                bool one = curItemObj.itemData.Both == 0;
                ItemData curSlot = PlayerManager.I.pData.EqSlot[eq]; //장착하려는 슬롯의 정보
                ItemData reSlot = PlayerManager.I.pData.EqSlot[eq == "Hand1" ? "Hand2" : "Hand1"];//반대쪽 슬롯의 정보
                if (curSlot == null)
                {
                    //해당 칸이 비어있을때
                    eqState = one ? 0 : (reSlot == null ? 2 : 3);//착용 예정 무기가 한손이면 0번, 양손이면 반대 손을 체크하여 비어있으면 2번, 비어있지 않으면 3번
                }
                else
                {
                    //해당 칸이 비어 있지 않을때
                    if (one)
                    {
                        //예정 무기가 한손일때
                        eqState = curSlot.Both == 0 ? 1 : 4; //착용 예정 무기가 한손이면 1번, 양손이면 4번
                    }
                    else
                    {
                        //예정 무기가 양손일때
                        eqState = reSlot == null ? 3 : (curSlot.Both == 1 ? 5 : 6); //착용 예정 무기가 양손이면 3번, 양손이면 5번, 양손이 아니면 둘 다 한손 무기이기에 착용 불가능으로 6번
                    }
                }
                break;
            default:
                eqState = PlayerManager.I.pData.EqSlot[eq] == null ? 0 : 1;
                break;
        }
        //포커싱 된 아이템의 좌표 및 장착 상태 설정
        ItemObj iObj = curItem[curIdx].GetComponent<ItemObj>();
        if (iObj.x > -1 && iObj.y > -1)
        {
            for (int y = sy; y < sy + h; y++)
            {
                for (int x = sx; x < sx + w; x++)
                    pGrids[y][x].slotId = -1;
            }
        }
        switch (eqState)
        {
            case 0:
                eqMain[eq].SetActive(false);
                PlaceSelectedItem(eq);
                break;
            case 1:
                SendMessageEqItem(eq);
                break;
            case 6:
                return;
            default:
                string otherEq = eq == "Hand1" ? "Hand2" : "Hand1";
                switch (eqState)
                {
                    case 2:
                        eqMain[eq].SetActive(false);
                        eqMain[otherEq].SetActive(false);
                        //
                        break;
                    case 3:
                        if (eqMain[eq].activeSelf)
                        {
                            eqMain[eq].SetActive(false);
                            SendMessageEqItem(eq);
                        }
                        else
                        {
                            eqMain[otherEq].SetActive(false);
                            SendMessageEqItem(otherEq);
                        }
                        //양손착용
                        break;
                    case 4:
                    case 5:
                        if (PlayerManager.I.pData.EqSlot[eq] != null)
                            SendMessageEqItem(eq);
                        else
                            SendMessageEqItem(otherEq);
                        if (eqState == 4)
                            eqMain[otherEq].SetActive(true);
                        //
                        break;
                }
                break;
        }
    }
    private void PlaceSelectedItem(string eq)
    {
        //포커싱 된 아이템의 위치 설정
        curItem[curIdx].transform.position = new Vector3(eqBox[eq].transform.position.x, eqBox[eq].transform.position.y, 0f);
        ItemObj iObj = curItem[curIdx].GetComponent<ItemObj>();
        iObj.x = -1; iObj.y = -1; iObj.eq = eq;
        PlayerManager.I.ApplyEqSlot(eq, curItemObj.itemData);
    }
    private ItemObj GetItem(int id)
    {
        foreach (var v in curItem)
        {
            ItemObj iObj = v.GetComponent<ItemObj>();
            if (iObj.uid == id) return iObj;
        }
        return null;
    }
    private void SendMessageEqItem(string eq)
    {
        int uid = PlayerManager.I.pData.EqSlot[eq].Uid;
        ItemObj item = GetItem(uid);
        string str = $"{item.uid}_{item.itemData.ItemId}_{item.itemData.Type}";
        PlayerManager.I.TakeoffEq(eq); //장착 중이던 장비를 해제시킴
        PlaceSelectedItem(eq);
        Presenter.Send("InvenPop", "ClickObj", str);
    }
    private void Update()
    {
        if (moveOn)
        {
            //추후에 키보드 입력을 추가하여 아이템이 회전되도록 구현 예정
            curItem[curIdx].transform.position = Input.mousePosition; // 아이템 위치 업데이트
            float x = curItem[curIdx].transform.position.x, y = curItem[curIdx].transform.position.y;
            if (x <= popEdge[2] || x >= popEdge[3] || y <= popEdge[1] || y >= popEdge[0])
            {
                curState = 3;
            }
            else
            {
                if (y <= slotEdge[0] && y >= slotEdge[1] && x >= slotEdge[2] && x <= slotEdge[3])
                {
                    CheckOverlapWithGrid();
                }
                else
                {
                    curState = 3;
                    ResetAllGrids();
                }
            }
        }
    }
}

// private bool CanPlaceItem(int sx, int sy, int wid, int hei)
// {
//     // 그리드 범위 체크
//     if (sx + wid > 10 || sy + hei > 10)
//         return false;
//     // 해당 영역이 비어있는지 검사
//     for (int y = sy; y < sy + hei; y++)
//     {
//         for (int x = sx; x < sx + wid; x++)
//         {
//             if (pGrids[y][x].slotId != -1)
//                 return false;
//         }
//     }
//     return true;
// }