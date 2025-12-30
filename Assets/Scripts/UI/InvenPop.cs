using GB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class InvenPop : UIScreen
{
    private Image[,] ivGridImg, rwdGridImg;
    private RectTransform[,] ivGridRect, rwdGridRect;
    [SerializeField] private List<ItemObj> itemList = new List<ItemObj>(); //item object 인벤토리 아이템 리스트
    private List<List<InvenGrid>> pGrids, rwdGrids; //플레이어 인벤토리 그리드
    private Transform ivSlot, iPrt, eqPrt, rwdSlot;
    public static bool isActive = false, moveOn = false, isLoadRwd = false, isLoadInven = false;
    public static string[] curEq;
    public static int posType = 0;
    private int curState = 0, curItemX = -1, curItemY = -1;
    private RectTransform curItemRect;
    private ItemObj curItem;
    private Dictionary<string, EqSlot> eqSlots = new Dictionary<string, EqSlot>();
    private Vector2 ivPos, rwdPos, itemBasePos;
    private Color gridColor, greenColor, yellowColor, redColor;
    private void Awake()
    {
        Regist();
        RegistButton();
        InitColor();
        mGameObject["RwdPop"].SetActive(false);
    }
    private void Start()
    {
        ivSlot = mGameObject["IvSlot"].transform;
        iPrt = mGameObject["Item"].transform;
        eqPrt = mGameObject["Eq"].transform;
        pGrids = PlayerManager.I.grids;
        ivPos = new Vector2(ivSlot.position.x - 288, ivSlot.position.y + 288);
        InitGrid();
        LoadInven();
        isLoadInven = true;
    }
    private void InitColor()
    {
        gridColor = new Color(220 / 255f, 185 / 255f, 155 / 255f, 1);
        greenColor = new Color(129 / 255f, 183 / 255f, 127 / 255f, 1);
        yellowColor = new Color(255 / 255f, 250 / 255f, 101 / 255f, 1);
        redColor = new Color(255 / 255f, 105 / 255f, 101 / 255f, 1);
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
                if (mGameObject["RwdPop"].activeSelf)
                    CloseRwdPop();
                else
                    Close();
                break;
            case "Complete":
                CloseRwdPop();
                break;
            case "Check":
                mGameObject["RwdCheck"].SetActive(!mGameObject["RwdCheck"].activeSelf);
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "ClickItem":
                if (!moveOn)
                    SelectItemObj(data.Get<int>());
                break;
            case "EquipItem":
                EquipItem(data.Get<string>(), curItem);
                break;
            case "AddItem":

                break;
            case "ResetAllGrids":
                ResetAllGrids();
                break;
            case "OpenRwdPop":
                StartCoroutine(WaitLoadInven(isLoadInven ? 0f : 0.1f));
                break;
            case "DeleteItem":
                DeleteItem(data.Get<int>());
                break;
        }
    }
    private void InitGrid()
    {
        ivGridImg = new Image[10, 10];
        ivGridRect = new RectTransform[10, 10];
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                string gName = $"grid_{y}_{x}";
                var obj = ivSlot.Find(gName);
                ivGridImg[y, x] = obj.GetComponent<Image>();
                ivGridRect[y, x] = obj.transform as RectTransform;
            }
        }
        string[] eq = new string[] { "Hand1", "Hand2", "Armor", "Helmet", "Shoes", "Gloves", "Belt", "Cape", "Necklace", "Ring1", "Ring2" };
        for (int i = 0; i < eq.Length; i++)
        {
            string eqName = eq[i];
            eqSlots[eqName] = eqPrt.Find(eqName).GetComponent<EqSlot>();
        }
    }
    private IEnumerator WaitLoadInven(float time)
    {
        yield return new WaitForSecondsRealtime(time); //timescale 0으로 설정을 무시하고 진행시킴
        if (!isLoadRwd)
        {
            isLoadRwd = true;
            SetRwdPop();
        }
        mGameObject["RwdPop"].SetActive(true);
        LoadRwd();
    }
    private void SetRwdPop()
    {
        rwdGrids = new List<List<InvenGrid>>();
        for (int y = 0; y < 10; y++)
        {
            List<InvenGrid> row = new List<InvenGrid>();
            for (int x = 0; x < 10; x++)
            {
                row.Add(new InvenGrid { x = x, y = y, slotId = -1 });
            }
            rwdGrids.Add(row);
        }

        rwdSlot = mGameObject["RwdSlot"].transform;
        rwdPos = new Vector2(rwdSlot.position.x - 288, rwdSlot.position.y + 288);
        rwdGridImg = new Image[10, 10];
        rwdGridRect = new RectTransform[10, 10];
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                string gName = $"grid_{y}_{x}";
                var obj = rwdSlot.Find(gName);
                rwdGridImg[y, x] = obj.GetComponent<Image>();
                rwdGridRect[y, x] = obj.transform as RectTransform;
            }
        }
    }
    private void LoadInven()
    {
        for (int i = 0; i < PlayerManager.I.pData.Inven.Count; i++)
        {
            ItemData item = PlayerManager.I.pData.Inven[i];
            PlaceInvenItem(item.X, item.Y, item.W, item.H, item, item.X == -1 && item.Y == -1);
        }
        curItem = null;
    }
    private void LoadRwd()
    {
        List<int> list = ItemManager.I.RewardItemIdList;
        for (int i = 0; i < list.Count; i++)
        {
            ItemData item = ItemManager.I.ItemDataList[list[i]].Clone();
            item.Uid = ItemManager.I.GetUid();//보상 아이템은 Uid를 자동으로 부여하지 않아서 수동으로 부여
            PlaceRwdItem(item);
        }
    }
    private void SelectItemObj(int uid, bool isEq = false)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].uid == uid)
            {
                itemList[i].gameObject.transform.SetAsLastSibling();
                curItem = itemList[i];
                curItemRect = curItem.transform as RectTransform;
                itemBasePos = curItem.transform.position;
            }
        }
        StateItemRaycast(false);
        curItem.SetBgAlpha(0f);
        moveOn = true;
        CheckCurEq(curItem.itemData.ItemId, curItem.itemData.Type);
        if (curItem.x > -1 && curItem.y > -1)
        {
            InitItemGrid(curItem.x, curItem.y, curItem.itemData.W, curItem.itemData.H);
            curItem.x = -1; curItem.y = -1;
            CheckGrids(curItem.iType);
        }
        else
        {
            if (isEq) return;
            if (curItem.itemData.ItemId > 60000) return; //장착 아이템이 아닌 모든 아이템들은 스킵
            //장착 중이던 아이템일 경우 장착 해제시켜야함
            string eq = GetEquipBody(curItem.itemData.Uid);
            eqSlots[eq].StateMain(true);
            PlayerManager.I.TakeoffEq(eq);
        }
    }
    private void PlaceInvenItem(int sx, int sy, int wid, int hei, ItemData item, bool isEq)
    {
        GameObject obj = Instantiate(ResManager.GetGameObject("ItemObj"), iPrt);
        obj.name = $"Item_{item.Uid}";
        ItemObj itemObj = obj.GetComponent<ItemObj>();
        itemObj.SetItemData(item, sx, sy, 0);
        itemList.Add(itemObj);

        RectTransform rect = obj.transform as RectTransform;
        rect.sizeDelta = new Vector2(wid * 64, hei * 64);
        rect.position = GetItemPos(0, sx, sy, wid, hei);

        if (isEq)
        {
            string eq = GetEquipBody(item.Uid);
            PlayerManager.I.pData.EqSlot[eq] = null;
            EquipItem(eq, itemObj);
        }
        else
        {
            for (int y = sy; y < sy + hei; y++)
            {
                for (int x = sx; x < sx + wid; x++)
                    pGrids[y][x].slotId = item.Uid;
            }
        }
    }
    private void PlaceRwdItem(ItemData item)
    {
        Vector2Int pos = GetRwdPos(item.W, item.H);
        if (pos.x == -1 && pos.y == -1) return; //추후 예외처리
        item.X = pos.x; item.Y = pos.y;
        GameObject obj = Instantiate(ResManager.GetGameObject("ItemObj"), iPrt);
        obj.name = $"Item_{item.Uid}";
        ItemObj itemObj = obj.GetComponent<ItemObj>();
        itemObj.SetItemData(item, pos.x, pos.y, 1);
        itemList.Add(itemObj);

        RectTransform rect = obj.transform as RectTransform;
        rect.sizeDelta = new Vector2(item.W * 64, item.H * 64);
        rect.position = GetItemPos(1, pos.x, pos.y, item.W, item.H);

        for (int y = pos.y; y < pos.y + item.H; y++)
        {
            for (int x = pos.x; x < pos.x + item.W; x++)
            {
                rwdGrids[y][x].slotId = item.Uid;
            }
        }
    }
    private void ReturnItem()
    {
        curItem.transform.position = itemBasePos;
        EndMovingItem();
    }
    private Vector3 GetItemPos(int type, int sx, int sy, int wid, int hei)
    {
        //type 0: 유저 인벤, 2: 보상 인벤
        Vector2 pos = type == 0 ? ivPos : rwdPos;
        int w = (wid - 1) * 32, h = (hei - 1) * 32;
        float x = pos.x + (sx * 64) + w, y = pos.y - (sy * 64) - h;
        return new Vector3(x, y, 0);
    }

    private void CheckGrids(int type)
    {
        // 아이템 위치에 따라 왼쪽 상단에 가장 근접한 그리드를 찾고 아이템 w,h 만큼 검색 후 겹치는 그리드 색상 변경
        float wid = curItemRect.sizeDelta.x - 32, hei = curItemRect.sizeDelta.y - 32;
        float minX = curItemRect.position.x - wid / 2, minY = curItemRect.position.y - hei / 2;
        float maxX = curItemRect.position.x + wid / 2, maxY = curItemRect.position.y + hei / 2;

        int myUid = curItem.itemData.Uid, maxCnt = curItem.itemData.W * curItem.itemData.H;
        int gx = 0, gy = 0;

        for (int y = 0; y < 10; y++)
        {
            bool isOverlapping = false;
            for (int x = 0; x < 10; x++)
            {
                RectTransform gRect = type == 0 ? ivGridRect[y, x] : rwdGridRect[y, x];
                float gMinX = gRect.position.x - 32, gMinY = gRect.position.y - 32, gMaxX = gRect.position.x + 32, gMaxY = gRect.position.y + 32;
                isOverlapping = !(maxX < gMinX || minX > gMaxX || maxY < gMinY || minY > gMaxY);
                if (isOverlapping)
                {
                    gx = x; gy = y;
                    break;
                }
            }
            if (isOverlapping) break;
        }
        if (gx == curItemX && gy == curItemY) return;
        ResetAllGrids();
        int[] gridX = new int[maxCnt], gridY = new int[maxCnt];
        List<int> gridUid = new List<int>();
        int cnt = 0;
        int cw = curItem.itemData.W, ch = curItem.itemData.H;
        if (gx + cw > 9) gx = 10 - cw;
        if (gy + ch > 9) gy = 10 - ch;
        for (int h = 0; h < ch; h++)
        {
            for (int w = 0; w < cw; w++)
            {
                gridX[cnt] = gx + w;
                gridY[cnt] = gy + h;
                cnt++;
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
        Image[,] gridImgs = type == 0 ? ivGridImg : rwdGridImg;
        void ChangeGridColor(int x, int y)
        {
            switch (curState)
            {
                case 0: gridImgs[y, x].color = greenColor; break; // 초록색
                case 1: gridImgs[y, x].color = yellowColor; break; // 노란색
                case 2: gridImgs[y, x].color = redColor; break; // 빨간색
            }
        }
        // 겹치는 그리드 색상 변경
        for (int i = 0; i < maxCnt; i++)
            ChangeGridColor(gridX[i], gridY[i]);
        foreach (var v in itemList)
        {
            if (v.x > -1 && v.y > -1)
            {
                if (gridUid.IndexOf(v.itemData.Uid) != -1)
                    v.SetBgAlpha(0f);
                else
                    v.SetBgAlpha(1f);
            }
        }
    }
    private void MoveItem(int w, int h, int ex, int ey)
    {
        curItemRect.position = GetItemPos(posType, ex, ey, w, h);

        int uid = curItem.itemData.Uid;
        for (int y = ey; y < ey + h; y++)
        {
            for (int x = ex; x < ex + w; x++)
                pGrids[y][x].slotId = uid;
        }
        curItem.iType = posType;
        curItem.x = ex; curItem.y = ey; curItem.eq = "";
        curItem.SetBgAlpha(1f);
        EndMovingItem();
    }
    private void ChangeItem(int w, int h, int ex, int ey)
    {
        List<List<InvenGrid>> grids = posType == 0 ? pGrids : rwdGrids;
        int uId = 0;
        for (int y = ey; y < ey + h; y++)
        {
            for (int x = ex; x < ex + w; x++)
            {
                if (grids[y][x].slotId != -1)
                {
                    uId = grids[y][x].slotId;
                    break;
                }
            }
        }
        ItemObj tg = GetItemObj(uId);
        if (tg != null)
        {
            int tx = tg.x, ty = tg.y;
            int tw = tg.itemData.W, th = tg.itemData.H;
            for (int y = ty; y < ty + th; y++)
            {
                for (int x = tx; x < tx + tw; x++)
                    grids[y][x].slotId = -1;
            }
            tg.x = -1; tg.y = -1;
            MoveItem(w, h, ex, ey);
            SelectItemObj(uId, true);
        }
    }

    private void EquipItem(string eq, ItemObj itemObj)
    {
        #region eqState 설명
        // 0 : 비어있으며 해당 칸에만 적용되는 한손 무기 및 일반 장비
        // 1 : 비어있지 않으며 해당 장비칸에만 적용되는 한손 무기 및 일반 장비
        // 2 : 손1,손2만 해당 => 착용된 양손 무기 빼고 한손 무기 착용
        // 3 : 손1,손2만 해당 => 둘 다 비어있으며 해당 무기가 양손일 경우
        // 4 : 착용된 한손,양손 무기 빼고 양손 무기 착용
        // 5 : 양손에 한 손 무기가 착용되어 예외처리
        // 양손 착용시 무조건 손1로 무기는 이동된다.
        // 손1, 손2 착용된 장비에 대한 체크
        #endregion
        int eqState;
        Dictionary<string, ItemData> myEqSlots = PlayerManager.I.pData.EqSlot;
        switch (eq)
        {
            case "Hand1":
            case "Hand2":
                bool one = itemObj.itemData.Both == 0; //해당 무기가 한손무기인지 양손무기인지 체크
                if (myEqSlots["Hand1"] != null && myEqSlots["Hand1"].Both == 1)
                {
                    //양손 무기 착용중 (양손무기는 무조건 손1에만 적용)
                    eqState = one ? 2 : 4;
                    break;
                }
                ItemData curSlot = PlayerManager.I.pData.EqSlot[eq]; //장착하려는 슬롯의 정보
                ItemData revSlot;
                if (curSlot == null)
                {
                    if (one)
                        eqState = 0;
                    else
                    {
                        revSlot = myEqSlots[eq == "Hand1" ? "Hand2" : "Hand1"];//반대쪽 슬롯의 정보
                        eqState = revSlot == null ? 3 : 4;
                    }
                }
                else
                {
                    if (one)
                        eqState = 1;
                    else
                    {
                        revSlot = myEqSlots[eq == "Hand1" ? "Hand2" : "Hand1"];//반대쪽 슬롯의 정보
                        eqState = revSlot == null ? 4 : 5;
                    }
                }
                break;
            default:
                eqState = myEqSlots[eq] == null ? 0 : 1;
                break;
        }
        int sx = itemObj.x, sy = itemObj.y, w = itemObj.itemData.W, h = itemObj.itemData.H;
        if (sx > -1 && sy > -1)
        {
            for (int y = sy; y < sy + h; y++)
            {
                for (int x = sx; x < sx + w; x++)
                    pGrids[y][x].slotId = -1;
            }
        }
        itemObj.SetBgAlpha(0f);
        switch (eqState)
        {
            case 0:
                eqSlots[eq].StateMain(false);
                PlaceEqItem(eq, itemObj);
                break;
            case 1:
                int uid1 = myEqSlots[eq].Uid;
                PlaceEqItem(eq, itemObj);
                SelectItemObj(uid1, true);
                break;
            case 5: return;
            default:
                string oEq = eq == "Hand1" ? "Hand2" : "Hand1"; //otherEq
                switch (eqState)
                {
                    case 2: //착용된 양손 무기 빼고 한손 무기 착용
                        int uid2 = myEqSlots["Hand1"].Uid;
                        PlayerManager.I.TakeoffEq("Hand1");
                        eqSlots[oEq].StateMain(true);
                        PlaceEqItem(eq, itemObj);
                        SelectItemObj(uid2, true);
                        break;
                    case 3: //손1,손2 비어있으며 양손무기 착용
                        eqSlots[eq].StateMain(false);
                        eqSlots[oEq].StateMain(false);
                        PlaceEqItem(eq, itemObj);
                        break;
                    case 4: //착용된 한손,양손 무기 빼고 양손 무기 착용
                        int uid3 = myEqSlots["Hand1"] != null ? myEqSlots["Hand1"].Uid : myEqSlots["Hand2"].Uid;
                        if (myEqSlots["Hand1"] != null) PlayerManager.I.TakeoffEq("Hand1");
                        else if (myEqSlots["Hand2"] != null) PlayerManager.I.TakeoffEq("Hand2");

                        eqSlots[eq].StateMain(false); eqSlots[oEq].StateMain(false);

                        PlaceEqItem("Hand1", itemObj);
                        SelectItemObj(uid3, true);
                        break;
                }
                break;
        }
    }

    private void PlaceEqItem(string eq, ItemObj itemObj)
    {
        itemObj.transform.position = eqSlots[eq].transform.position;
        itemObj.x = -1; itemObj.y = -1; itemObj.eq = eq;
        PlayerManager.I.ApplyEqSlot(eq, itemObj.itemData);
        EndMovingItem();
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
            eqSlots[v].StatePossible(true);
    }

    private void Update()
    {
        // if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        // {
        //     Debug.Log("Control + Left Click");
        //     return;
        // }
        if (mGameObject["RwdPop"].activeSelf)
        {

        }
        if (moveOn)
        {
            curItem.transform.position = Input.mousePosition;
            if (posType >= 0)
            {
                CheckGrids(posType);
                if (Input.GetMouseButtonDown(0))
                {
                    switch (curState)
                    {
                        case 0:
                            MoveItem(curItem.itemData.W, curItem.itemData.H, curItemX, curItemY);
                            break;
                        case 1:
                            ChangeItem(curItem.itemData.W, curItem.itemData.H, curItemX, curItemY);
                            break;
                    }
                }
            }
        }
    }
    private void InitItemGrid(int sx, int sy, int w, int h)
    {
        //아이템 클릭 하여 이동시 아이템이 있던 그리드에서 아이템의 그리드를 제외시킴
        List<List<InvenGrid>> grids = posType == 0 ? pGrids : rwdGrids;
        for (int y = sy; y < sy + h; y++)
        {
            for (int x = sx; x < sx + w; x++)
                grids[y][x].slotId = -1;
        }
    }
    private void EndMovingItem()
    {
        StateItemRaycast(true);
        ResetAllGrids();
        ResetAllEq();
        moveOn = false;
        curItem = null;
        curItemRect = null; itemBasePos = Vector3.zero;
        curItemX = -1; curItemY = -1;
        curEq = new string[] { };
    }
    private void StateItemRaycast(bool isActive)
    {
        foreach (var v in itemList)
            v.SetRaycastTarget(isActive);
    }
    private void ResetAllGrids()
    {
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (ivGridImg[y, x].color != gridColor)
                    ivGridImg[y, x].color = gridColor;
            }
        }
        if (mGameObject["RwdPop"].activeSelf)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (rwdGridImg[y, x].color != gridColor)
                        rwdGridImg[y, x].color = gridColor;
                }
            }
        }
    }
    private ItemObj GetItemObj(int id)
    {
        foreach (var v in itemList)
        {
            if (v.uid == id) return v;
        }
        return null;
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
    private void ResetAllEq()
    {
        foreach (var v in eqSlots)
            v.Value.StatePossible(false);
    }
    private Vector2Int GetRwdPos(int wid, int hei)
    {
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (rwdGrids[y][x].slotId == -1)
                {
                    int cnt = wid * hei;
                    if (x + wid > 9 || y + hei > 9) continue;
                    for (int h = y; h < y + hei; h++)
                    {
                        for (int w = x; w < x + wid; w++)
                        {
                            if (rwdGrids[h][w].slotId == -1)
                                cnt--;
                        }
                    }
                    if (cnt == 0)
                        return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
    private void DeleteRwdItems()
    {
        for (int i = itemList.Count - 1; i >= 0; i--)
        {
            if (itemList[i].iType == 1)
            {
                Destroy(itemList[i].gameObject);
                itemList.RemoveAt(i);
            }
        }
    }
    private void CloseRwdPop()
    {
        DeleteRwdItems();
        mGameObject["RwdPop"].SetActive(false);
        if (mGameObject["RwdCheck"].activeSelf)
            Close();
    }
    private void DeleteItem(int uid)
    {
        for (int i = itemList.Count - 1; i >= 0; i--)
        {
            if (itemList[i].uid == uid)
            {
                Destroy(itemList[i].gameObject);
                itemList.RemoveAt(i);
            }
        }
    }
    public override void Refresh() { }
}