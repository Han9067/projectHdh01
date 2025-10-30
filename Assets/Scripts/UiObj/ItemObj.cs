using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;
public class ItemObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int ivType; //inven type -> 팝업에 따라 해당 타입이 바뀜. 0: 유저 인벤, 1: 상점 인벤
    public int x, y, uid;
    public string eq = "";
    public ItemData itemData;
    [SerializeField] private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        GetComponent<Image>().sprite = ResManager.GetSprite(itemData.Res);
    }

    public void OnButtonClick()
    {
        string str;
        switch (ivType)
        {
            case 0:
                str = $"{itemData.Uid}_{itemData.ItemId}_{itemData.Type}";
                Presenter.Send("InvenPop", "ClickObj", str);
                break;
            case 1:
                UIManager.ShowPopup("SelectPop");
                Presenter.Send("SelectPop", "SetList", 0);
                break;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 버튼 위에 올라왔을 때 한 번 실행
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 버튼 밖으로 나갔을 때 한 번 실행
    }
    public void SetItemData(ItemData data, int type)
    {
        itemData = data;
        ivType = type;
    }
    public void SetItemData(ItemData data, int xx, int yy, int type)
    {
        itemData = data;
        x = xx; y = yy; uid = data.Uid;
        ivType = type;
    }
}
