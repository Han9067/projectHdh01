using GB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
public class InvenPop : UIScreen
{
    private Image[,] ivGridImg;
    private RectTransform[,] ivGridRect;
    [SerializeField] private List<ItemObj> itemList; //item object 인벤토리 아이템 리스트
    private List<List<InvenGrid>> pGrids; //플레이어 인벤토리 그리드
    private Transform sPrt1, iPrt1, eqPrt;
    // private RectTransform sResct1, eqRect1;
    public static bool isActive = false, moveOn = false, intoIvGrids = false;
    public static string[] curEq;
    private int curIdx = -1, curState = 0, curItemX = -1, curItemY = -1;
    private RectTransform curItemRect;
    private ItemObj curItem;
    private Dictionary<string, EqSlot> eqSlots = new Dictionary<string, EqSlot>();
    // private float[] popEdge = new float[4];
    // private float[] slotEdge = new float[4];
    private Vector2 ivPos, itemBasePos;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        sPrt1 = mGameObject["Slot1"].transform;
        iPrt1 = mGameObject["Item1"].transform;
        eqPrt = mGameObject["Eq"].transform;
        // sResct1 = mGameObject["Slot1"].GetComponent<RectTransform>();
        // eqRect1 = mGameObject["Eq1"].GetComponent<RectTransform>();
        pGrids = PlayerManager.I.grids;
        ivPos = new Vector2(sPrt1.position.x - 288, sPrt1.position.y + 288);
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
                Close();
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
                var obj = sPrt1.Find(gName);
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
    private void LoadInven()
    {
        itemList = new List<ItemObj>();
        for (int i = 0; i < PlayerManager.I.pData.Inven.Count; i++)
        {
            ItemData item = PlayerManager.I.pData.Inven[i];
            PlaceInvenItem(item.X, item.Y, item.W, item.H, item, item.X == -1 && item.Y == -1);
        }
        curItem = null;
        curIdx = -1;
    }
    private void SelectItemObj(int uid, bool isEq = false)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].uid == uid)
            {
                curIdx = i;
                itemList[i].gameObject.transform.SetAsLastSibling();
                curItem = itemList[i];
                curItemRect = curItem.transform as RectTransform;
                itemBasePos = curItem.transform.position;
            }
        }
        StateItemRaycast(false);
        moveOn = true;
        CheckCurEq(curItem.itemData.ItemId, curItem.itemData.Type);
        if (curItem.x > -1 && curItem.y > -1)
            CheckGrids();
        else
        {
            if (isEq) return;
            //장착 중이던 아이템일 경우 장착 해제시켜야함
            string eq = GetEquipBody(curItem.itemData.Uid);
            eqSlots[eq].StateMain(true);
            PlayerManager.I.TakeoffEq(eq);
        }
    }
    private void PlaceInvenItem(int sx, int sy, int wid, int hei, ItemData item, bool isEq)
    {
        GameObject obj = Instantiate(ResManager.GetGameObject("ItemObj"), iPrt1);
        obj.name = $"Item_{item.Uid}";
        ItemObj itemObj = obj.GetComponent<ItemObj>();
        itemObj.SetItemData(item, sx, sy, 0);
        itemList.Add(itemObj);

        RectTransform rect = obj.transform as RectTransform;
        rect.sizeDelta = new Vector2(wid * 64, hei * 64);
        rect.position = GetItemPos(sx, sy, wid, hei);

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
    private void ReturnItem()
    {
        curItem.transform.position = itemBasePos;
        EndMovingItem();
    }
    private Vector3 GetItemPos(int sx, int sy, int wid, int hei)
    {
        int w = (wid - 1) * 32, h = (hei - 1) * 32;
        float x = ivPos.x + (sx * 64) + w, y = ivPos.y - (sy * 64) - h;
        return new Vector3(x, y, 0);
    }

    private void CheckGrids()
    {
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
                RectTransform gRect = ivGridRect[y, x];
                float gMinX = gRect.position.x - 32, gMinY = gRect.position.y - 32, gMaxX = gRect.position.x + 32, gMaxY = gRect.position.y + 32;
                isOverlapping = !(maxX < gMinX || minX > gMaxX || maxY < gMinY || minY > gMaxY);
                if (isOverlapping)
                {
                    // Debug.Log(x + " " + y);
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
        void ChangeGridColor(int x, int y)
        {
            switch (curState)
            {
                case 0: ivGridImg[y, x].color = new Color(129 / 255f, 183 / 255f, 127 / 255f, 1); break; // 초록색
                case 1: ivGridImg[y, x].color = new Color(255 / 255f, 250 / 255f, 101 / 255f, 1); break; // 노란색
                case 2: ivGridImg[y, x].color = new Color(255 / 255f, 105 / 255f, 101 / 255f, 1); break; // 빨간색
            }
        }
        // 겹치는 그리드 색상 변경
        for (int i = 0; i < maxCnt; i++)
            ChangeGridColor(gridX[i], gridY[i]);
    }
    private void MoveItem(int sx, int sy, int w, int h, int ex, int ey)
    {
        if (curItem.x > -1 && curItem.y > -1)
        {
            for (int y = sy; y < sy + h; y++)
            {
                for (int x = sx; x < sx + w; x++)
                    pGrids[y][x].slotId = -1;
            }
        }
        curItemRect.position = GetItemPos(ex, ey, w, h);

        int uid = curItem.itemData.Uid;
        for (int y = ey; y < ey + h; y++)
        {
            for (int x = ex; x < ex + w; x++)
                pGrids[y][x].slotId = uid;
        }
        curItem.x = ex; curItem.y = ey; curItem.eq = "";
        EndMovingItem();
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
        ItemObj tg = GetItemObj(uId);
        if (tg != null)
        {
            int tx = tg.x, ty = tg.y;
            int tw = tg.itemData.W, th = tg.itemData.H;
            for (int y = ty; y < ty + th; y++)
            {
                for (int x = tx; x < tx + tw; x++)
                    pGrids[y][x].slotId = -1;
            }
            tg.x = -1; tg.y = -1;
            MoveItem(sx, sy, w, h, ex, ey);
            SelectItemObj(uId);
        }
    }

    private void EquipItem(string eq, ItemObj itemObj)
    {
        #region eqState 설명
        // 0 : 비어있으며 해당 칸에만 적용되는 한손 무기 및 일반 장비
        // 1 : 비어있지 않으며 해당 장비칸에만 적용되는 한손 무기 및 일반 장비
        // 2 : 손1,손2만 해당 => 둘 다 비어있으며 해당 무기가 양손일 경우
        // 3 : 손1,손2만 해당 => 둘 중 한 곳이 한손무기이고 해당 무기가 양손일 경우(장착된 한손 무기를 제거하고 양손무기 장착)
        // 4 : 손1,손2만 해당 => 장착된 무기가 양손무기이고 해당 무기가 한손일 경우(장착된 양손 무기를 제거하고 해당 칸에 한손무기 장착)
        // 5 : 손1,손2만 해당 => 장착된 무기가 양손무기이고 해당 무기가 양손일 경우(장착된 양손 무기를 제거하고 해당 무기를 장착)
        // 6 : 손1,손2만 해당 => 장착된 무기가 한손무기 두 개이며 해당 무기가 양손일 경우(해당 무기를 장착하지 못하게 예외처리) 
        // 손1, 손2 착용된 장비에 대한 체크
        #endregion
        int eqState;
        switch (eq)
        {
            case "Hand1":
            case "Hand2":
                //해당 무기가 한손무기인지 양손무기인지 체크
                bool one = itemObj.itemData.Both == 0;
                Debug.Log(itemObj.itemData.ItemId + " " + itemObj.itemData.Both);
                ItemData curSlot = PlayerManager.I.pData.EqSlot[eq]; //장착하려는 슬롯의 정보
                ItemData reSlot = PlayerManager.I.pData.EqSlot[eq == "Hand1" ? "Hand2" : "Hand1"];//반대쪽 슬롯의 정보
                if (curSlot == null)//해당 칸이 비어있을때
                    eqState = one ? 0 : (reSlot == null ? 2 : 3); //착용 예정 무기가 한손이면 0번, 양손이면 반대 손을 체크하여 비어있으면 2번, 비어있지 않으면 3번
                else
                {
                    //해당 칸이 비어 있지 않을때
                    if (one)//예정 무기가 한손일때
                        eqState = curSlot.Both == 0 ? 1 : 4; //착용 예정 무기가 한손이면 1번, 양손이면 4번
                    else//예정 무기가 양손일때
                        eqState = reSlot == null ? 3 : (curSlot.Both == 1 ? 5 : 6); //착용 예정 무기가 양손이면 3번, 양손이면 5번, 양손이 아니면 둘 다 한손 무기이기에 착용 불가능으로 6번
                }
                break;
            default:
                eqState = PlayerManager.I.pData.EqSlot[eq] == null ? 0 : 1;
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
        switch (eqState)
        {
            case 0:
                eqSlots[eq].StateMain(false);
                PlaceEqItem(eq, itemObj);
                break;
            case 1:
                int backUid = PlayerManager.I.pData.EqSlot[eq].Uid;
                PlaceEqItem(eq, itemObj);
                SelectItemObj(backUid, true);
                break;
            case 6:
                return;
            default:
                string otherEq = eq == "Hand1" ? "Hand2" : "Hand1";
                switch (eqState)
                {
                    case 2: //손1,손2 비어있으며 양손무기 착용
                        Debug.Log("2");
                        eqSlots[eq].StateMain(false);
                        eqSlots[otherEq].StateMain(false);
                        PlaceEqItem(eq, itemObj);
                        break;
                    case 3:
                        // if (eqMain[eq].activeSelf)
                        // {
                        //     eqMain[eq].SetActive(false);
                        //     SendMessageEqItem(eq);
                        // }
                        // else
                        // {
                        //     eqMain[otherEq].SetActive(false);
                        //     SendMessageEqItem(otherEq);
                        // }
                        //양손착용
                        break;
                    case 4:
                    case 5:
                        // if (PlayerManager.I.pData.EqSlot[eq] != null)
                        //     SendMessageEqItem(eq);
                        // else
                        //     SendMessageEqItem(otherEq);
                        // if (eqState == 4)
                        //     eqMain[otherEq].SetActive(true);
                        // //
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
        if (moveOn)
        {
            curItem.transform.position = Input.mousePosition;
            if (intoIvGrids)
            {
                CheckGrids();
                if (Input.GetMouseButtonDown(0))
                {
                    switch (curState)
                    {
                        case 0:
                            MoveItem(curItem.x, curItem.y, curItem.itemData.W, curItem.itemData.H, curItemX, curItemY);
                            break;
                        case 1:
                            ChangeItem(curItem.x, curItem.y, curItem.itemData.W, curItem.itemData.H, curItemX, curItemY);
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                }
            }
        }
    }
    private void EndMovingItem()
    {
        StateItemRaycast(true);
        ResetAllGrids();
        ResetAllEq();
        moveOn = false;
        curItem = null; curIdx = -1;
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
        Color whiteColor = Color.white;
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (ivGridImg[y, x].color != whiteColor)
                    ivGridImg[y, x].color = whiteColor;
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
    public override void Refresh() { }
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