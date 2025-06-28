using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GB;

public class ShopManager : AutoSingleton<ShopManager>
{
    [System.Serializable]
    public class ShopData
    {
        public int shopId;
        public string shopName;
        public int cityId;
        public ShopType shopType;
        public List<ShopItemData> items;
        
        public ShopData()
        {
            items = new List<ShopItemData>();
        }
    }

    [System.Serializable]
    public class ShopItemData
    {
        public int itemId;
        public int price;
        public string itemName;
        public int width;   // 아이템 너비 (W)
        public int height;  // 아이템 높이 (H)
        
        public ShopItemData(int id, int itemPrice)
        {
            itemId = id;
            price = itemPrice;
            width = 1;   // 기본값 1x1
            height = 1;
        }
    }

    public enum ShopType
    {
        Inn = 1,           // 여관 - 음식, 술, 음료 판매
        Smith = 2,         // 대장간 - 무기와 철제 갑옷 판매
        TailorShop = 3,    // 재단소 - 천 종류 옷, 장갑, 신발, 망토 판매
        Apothecary = 4,    // 약제상 - 약 판매
        Market = 5         // 시장 - 교역품 판매
    }

    [Header("Shop Data")]
    [SerializeField] private ShopTable shopTable;
    [SerializeField] private ShopItemTable shopItemTable;
    [SerializeField] private ItemTable itemTable;
    [SerializeField] private WpTable wpTable;
    [SerializeField] private EqTable eqTable;

    private Dictionary<int, ShopData> allShops = new Dictionary<int, ShopData>();
    private Dictionary<int, List<ShopData>> shopsByCity = new Dictionary<int, List<ShopData>>();


    void Start()
    {
        LoadShopData();
    }

    /// <summary>
    /// 상점 데이터를 로드하고 초기화합니다.
    /// </summary>
    public void LoadShopData()
    {
        if (shopTable == null || shopItemTable == null || itemTable == null || wpTable == null || eqTable == null)
        {
            Debug.LogError("ShopManager: 필요한 테이블 데이터가 없습니다!");
            return;
        }

        allShops.Clear();
        shopsByCity.Clear();

        // 상점 기본 정보 로딩
        for (int i = 0; i < shopTable.Count; i++)
        {
            var shopProb = shopTable[i];
            int shopId = int.Parse(shopProb.ID);
            int cityId = int.Parse(shopProb.CItyID);

            var shopData = new ShopData
            {
                shopId = shopId,
                shopName = shopProb.Name,
                cityId = cityId,
                shopType = (ShopType)shopProb.Type
            };

            allShops[shopId] = shopData;

            // 도시별 상점 분류
            if (!shopsByCity.ContainsKey(cityId))
                shopsByCity[cityId] = new List<ShopData>();

            shopsByCity[cityId].Add(shopData);
        }

        // 상점별 아이템 정보 로딩
        LoadShopItems();

        Debug.Log($"ShopManager: {allShops.Count}개의 상점 데이터를 로드했습니다.");
    }

    /// <summary>
    /// 상점별 아이템 정보를 로드합니다.
    /// </summary>
    private void LoadShopItems()
    {
        for (int i = 0; i < shopItemTable.Count; i++)
        {
            var itemProb = shopItemTable[i];
            int shopId = int.Parse(itemProb.ID);
            int itemId = int.Parse(itemProb.Name); // 실제로는 아이템 ID가 저장되어 있을 것으로 예상

            if (allShops.ContainsKey(shopId))
            {
                var shopData = allShops[shopId];
                var itemData = new ShopItemData(itemId, 0); // 기본 가격은 0으로 설정
                
                // 아이템 정보를 세 개의 테이블에서 찾기
                bool foundItem = false;
                
                // 1. ItemTable에서 검색 (물약, 재료, 귀중품 등)
                if (itemTable.ContainsKey(itemId.ToString()))
                {
                    var itemInfo = itemTable[itemId.ToString()];
                    itemData.price = itemInfo.Price;
                    itemData.itemName = itemInfo.Name;
                    itemData.width = itemInfo.W;
                    itemData.height = itemInfo.H;
                    foundItem = true;
                }
                // 2. WpTable에서 검색 (무기)
                else if (wpTable.ContainsKey(itemId.ToString()))
                {
                    var wpInfo = wpTable[itemId.ToString()];
                    itemData.price = wpInfo.Price;
                    itemData.itemName = wpInfo.Name;
                    itemData.width = wpInfo.W;
                    itemData.height = wpInfo.H;
                    foundItem = true;
                }
                // 3. EqTable에서 검색 (장비)
                else if (eqTable.ContainsKey(itemId.ToString()))
                {
                    var eqInfo = eqTable[itemId.ToString()];
                    itemData.price = eqInfo.Price;
                    itemData.itemName = eqInfo.Name;
                    itemData.width = eqInfo.W;
                    itemData.height = eqInfo.H;
                    foundItem = true;
                }

                if (foundItem)
                {
                    shopData.items.Add(itemData);
                }
                else
                {
                    Debug.LogWarning($"ShopManager: 아이템 ID {itemId}를 어떤 테이블에서도 찾을 수 없습니다.");
                }
            }
        }
    }

    /// <summary>
    /// 특정 마을의 모든 상점을 반환합니다.
    /// </summary>
    public List<ShopData> GetShopsInCity(int cityId)
    {
        if (shopsByCity.ContainsKey(cityId))
            return shopsByCity[cityId];
        
        return new List<ShopData>();
    }

    /// <summary>
    /// 상점 ID로 상점 정보를 반환합니다.
    /// </summary>
    public ShopData GetShopById(int shopId)
    {
        if (allShops.ContainsKey(shopId))
            return allShops[shopId];
        
        return null;
    }

    /// <summary>
    /// 특정 상점의 판매 아이템 목록을 반환합니다.
    /// </summary>
    public List<ShopItemData> GetShopItems(int shopId)
    {
        var shop = GetShopById(shopId);
        if (shop != null)
            return shop.items.ToList();
        
        return new List<ShopItemData>();
    }

    /// <summary>
    /// 아이템을 구매합니다.
    /// </summary>
    public bool PurchaseItem(int shopId, int itemId, int quantity = 1)
    {
        var shop = GetShopById(shopId);
        if (shop == null)
        {
            Debug.LogWarning($"ShopManager: 상점 ID {shopId}를 찾을 수 없습니다.");
            return false;
        }

        var shopItem = shop.items.FirstOrDefault(item => item.itemId == itemId);
        if (shopItem == null)
        {
            Debug.LogWarning($"ShopManager: 상점 {shopId}에서 아이템 ID {itemId}를 찾을 수 없습니다.");
            return false;
        }

        // 플레이어 골드 확인
        if (!CheckPlayerGold(shopItem.price * quantity))
        {
            Debug.LogWarning($"ShopManager: 골드가 부족합니다. (필요: {shopItem.price * quantity})");
            return false;
        }

        // 구매 처리
        if (ProcessPurchase(shopItem, quantity))
        {
            Debug.Log($"ShopManager: 아이템 {itemId}를 {quantity}개 구매했습니다.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 플레이어의 골드를 확인합니다.
    /// </summary>
    private bool CheckPlayerGold(int requiredGold)
    {
        // TODO: 플레이어 매니저와 연동하여 실제 골드 확인
        return true;
    }

    /// <summary>
    /// 구매 처리를 합니다.
    /// </summary>
    private bool ProcessPurchase(ShopItemData item, int quantity)
    {
        // TODO: 플레이어 인벤토리 매니저와 연동하여 아이템 추가
        // TODO: 플레이어 골드 차감
        return true;
    }

    /// <summary>
    /// 특정 상점 타입의 상점들을 반환합니다.
    /// </summary>
    public List<ShopData> GetShopsByType(ShopType shopType)
    {
        return allShops.Values.Where(shop => shop.shopType == shopType).ToList();
    }

    /// <summary>
    /// 상점의 아이템 가격을 조정합니다.
    /// </summary>
    public void SetItemPrice(int shopId, int itemId, int newPrice)
    {
        var shop = GetShopById(shopId);
        if (shop == null) return;

        var item = shop.items.FirstOrDefault(i => i.itemId == itemId);
        if (item != null)
        {
            item.price = newPrice;
            Debug.Log($"ShopManager: 상점 {shopId}의 아이템 {itemId} 가격을 {newPrice}로 변경했습니다.");
        }
    }

    /// <summary>
    /// 디버그용: 모든 상점 정보를 출력합니다.
    /// </summary>
    [ContextMenu("Print All Shop Info")]
    public void PrintAllShopInfo()
    {
        Debug.Log("=== 모든 상점 정보 ===");
        foreach (var shop in allShops.Values)
        {
            Debug.Log($"상점 ID: {shop.shopId}, 이름: {shop.shopName}, 도시: {shop.cityId}, 타입: {shop.shopType}");
            Debug.Log($"아이템 수: {shop.items.Count}");
            foreach (var item in shop.items)
            {
                Debug.Log($"  - 아이템 ID: {item.itemId}, 가격: {item.price}, 이름: {item.itemName}, 크기: {item.width}x{item.height}");
            }
        }
    }
} 