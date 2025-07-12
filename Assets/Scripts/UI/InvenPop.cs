using GB;
using UnityEngine;
using UnityEngine.UI;

public class InvenPop : UIScreen
{
    [SerializeField] private Transform slotParent; // Slot 게임오브젝트
[SerializeField] private Transform eqParent; // 장비 게임오브젝트
    private Image[,] gridImages; // 그리드 이미지 배열

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
        gridImages = new Image[10, 10];
        // grid_0_0부터 grid_9_9까지 찾아서 배열에 할당
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                string gridName = $"grid_{x}_{y}";
                Transform grid = slotParent.Find(gridName);
                Image img = grid.GetComponent<Image>(); // 그리드 이미지 가져오기
                gridImages[x, y] = img; // 그리드 이미지 배열에 할당
            }
        }
    }
    private void LoadPlayerInven()
    {
        UnityEngine.Debug.Log(PlayerManager.I.pData.Inven.Count);
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
                    }else{
                        UnityEngine.Debug.Log("아이템을 배치할 수 없습니다.");
                    }
                }
            }
        }
    }
    private bool CanPlaceItem(int startX, int startY, int width, int height)
    {
        // 그리드 범위 체크
        if (startX + width > 10 || startY + height > 10)
            return false;
        
        // 해당 영역이 비어있는지 검사
        for (int y = startY; y < startY + height; y++)
        {
            for (int x = startX; x < startX + width; x++)
            {
                if (gridImages[x, y].sprite != null)
                    return false; // 이미 아이템이 있는 경우
            }
        }
        return true;
    }
    private void PlaceItem(int startX, int startY, int width, int height, ItemData item)
    {
        // 아이템 스프라이트 로드
        Sprite itemSprite = Resources.Load<Sprite>(item.Path);

        // 아이템 게임오브젝트 생성
        GameObject itemObj = new GameObject($"Item_{item.Name}");
        itemObj.transform.SetParent(eqParent); // 장비 부모 아래에 배치
        
        // RectTransform 설정
        RectTransform rectTransform = itemObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);

        // 그리드 크기에 맞춰 위치와 크기 설정
        rectTransform.anchoredPosition = new Vector2(startX * 64, -startY * 64);
        rectTransform.sizeDelta = new Vector2(width * 64, height * 64);
        rectTransform.localScale = new Vector3(1, 1, 1);
        
        // Image 컴포넌트 추가
        Image itemImage = itemObj.AddComponent<Image>();
        itemImage.sprite = itemSprite;
        itemImage.color = Color.white;
    }
}