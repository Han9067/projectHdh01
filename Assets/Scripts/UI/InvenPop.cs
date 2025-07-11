using GB;
using UnityEngine;
using UnityEngine.UI;

public class InvenPop : UIScreen
{
    [SerializeField] private Transform slotParent; // Slot 게임오브젝트

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
        if (PlayerManager.I?.pData?.Inven == null)
        {
            Debug.LogWarning("PlayerManager 또는 인벤토리 데이터가 준비되지 않았습니다.");
            return;
        }
        for(int i = 0; i < PlayerManager.I.pData.Inven.Count; i++)
        {
            
        }
    }

}