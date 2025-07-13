using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.UI;

public class InvenPop : UIScreen
{
    [SerializeField] private Transform slotParent; // Slot 게임오브젝트
    [SerializeField] private Transform eqParent; // 장비 게임오브젝트
    public GameObject[,] gridObj;
    private List<List<InvenGrid>> pGrids;
    public GameObject itemPrefab;
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
    public override void ViewQuick(string key, IOData data){}
    public override void Refresh(){}
    private void InitGrid()
    {
        // 10x10 배열 초기화
        gridObj = new GameObject[10, 10];
        // grid_0_0부터 grid_9_9까지 찾아서 배열에 할당
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                string gridName = $"grid_{x}_{y}";
                Transform grid = slotParent.Find(gridName);
                gridObj[x, y] = grid.gameObject; // 그리드 이미지 배열에 할당
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
        invenItemObj.SetItemData(item);
        // RectTransform 설정
        RectTransform rectTransform = itemObj.GetComponent<RectTransform>();
        float cx = gridObj[sx, sy].GetComponent<RectTransform>().anchoredPosition.x + ((wid - 1) * 32);
        float cy = gridObj[sx, sy].GetComponent<RectTransform>().anchoredPosition.y - ((hei - 1) * 32);

        // 그리드 크기에 맞춰 위치와 크기 설정
        rectTransform.anchoredPosition = new Vector2(cx, cy);
        rectTransform.sizeDelta = new Vector2(wid * 64, hei * 64);
        rectTransform.localScale = new Vector3(1, 1, 1);
        // 그리드 영역 체크
        for(int y = sy; y < sy + hei; y++)
        {
            for(int x = sx; x < sx + wid; x++)
            {
                pGrids[y][x].slotId = item.itemId;
            }
        }
    }
}