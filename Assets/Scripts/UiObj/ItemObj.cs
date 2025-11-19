using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;
public class ItemObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int iType; //item type -> 팝업에 따라 해당 타입이 바뀜. 0: 유저 인벤, 1: 상점 인벤
    public int x, y, uid;
    public string eq = "";
    public ItemData itemData;
    [SerializeField] private Button button;
    [SerializeField] private Image bg, main;
    private float bgAlpha = 1f;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        main.sprite = ResManager.GetSprite(itemData.Res);
        bg.color = ColorData.GetItemGradeColor(itemData.Grade);
        if (bg.color.a != bgAlpha)
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, bgAlpha);
    }

    public void OnButtonClick()
    {
        switch (iType)
        {
            case 0:
            case 1:
                Presenter.Send("InvenPop", "ClickItem", itemData.Uid);
                if (ItemInfoPop.isActive)
                    UIManager.ClosePopup("ItemInfoPop");
                break;
            case 10:
                UIManager.ShowPopup("SelectPop");
                Presenter.Send("SelectPop", "SetList", 0);
                Presenter.Send("SelectPop", "SetItemData", itemData);
                break;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InvenPop.moveOn) return;
        UIManager.ShowPopup("ItemInfoPop");
        Presenter.Send("ItemInfoPop", "OnItemInfo", itemData);

        RectTransform rect = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 캔버스 크기
        Vector2 canvasSize = canvasRect.rect.size;
        int dir = rect.position.x < canvasSize.x / 2f ? 1 : -1;
        Vector3 pos = new Vector3(rect.position.x + (dir * (rect.sizeDelta.x / 2 + 202)), rect.position.y, 0);
        Presenter.Send("ItemInfoPop", "OnItemPos", pos);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.ClosePopup("ItemInfoPop");
        // 마우스가 버튼 밖으로 나갔을 때 한 번 실행
    }
    public void SetItemData(ItemData data, int type)
    {
        itemData = data;
        iType = type;
    }
    public void SetItemData(ItemData data, int xx, int yy, int type)
    {
        itemData = data;
        x = xx; y = yy; uid = data.Uid;
        iType = type;
    }
    public void SetRaycastTarget(bool isActive)
    {
        bg.raycastTarget = isActive;
    }
    public void SetBgAlpha(float alpha)
    {
        bgAlpha = alpha;
        bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, bgAlpha);
    }
}
