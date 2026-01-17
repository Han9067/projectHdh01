using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ItemObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int iType; //item type -> 팝업에 따라 해당 타입이 바뀜. 0: 유저 인벤, 1: 상점 인벤
    public int x, y, uid;
    public string eq = "";
    public ItemData itemData;
    [SerializeField] private Image bg, main;
    private float bgAlpha = 1f;
    private Color bgColor;
    // Start is called before the first frame update
    void Start()
    {
        main.sprite = ResManager.GetSprite(itemData.Res);
        bgColor = ColorData.GetItemGradeColor(itemData.Grade);
        bg.color = new Color(bgColor.r, bgColor.g, bgColor.b, bgAlpha);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                switch (iType)
                {
                    case 0:
                    case 1:
                        if (InvenPop.isInstantMove)
                        {
                            InvenPop.isInstantMove = false;
                            Presenter.Send("InvenPop", "MoveItemToOppositeInven", new List<int> { uid, iType, itemData.W, itemData.H, x, y });
                            return;
                        }
                        if (ItemInfoPop.isActive)
                            UIManager.ClosePopup("ItemInfoPop");
                        Presenter.Send("InvenPop", "ClickItem", itemData.Uid);
                        break;
                    case 10:
                        UIManager.ShowPopup("SelectPop");
                        Presenter.Send("SelectPop", "SetList", 0);
                        Presenter.Send("SelectPop", "SetItemData", itemData);
                        break;
                }
                break;
            case PointerEventData.InputButton.Right:
                UIManager.ShowPopup("SelectPop");
                Presenter.Send("SelectPop", "SetItemData", itemData);
                if (itemData.ItemId > 60000)
                {
                    if (itemData.ItemId < 64001)
                        Presenter.Send("SelectPop", "SetList", 4); //소모형 아이템
                    else
                        Presenter.Send("SelectPop", "SetList", 5);
                }
                else
                    Presenter.Send("SelectPop", "SetList", 3);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InvenPop.moveOn) return;
        #region 아이템 체크
        if (bgAlpha > 0f)
            bg.color = new Color(125f / 255f, 1f, 210 / 255f, 1f);
        // SetBgAlpha(0.5f);
        #endregion
        #region 아이템 정보 팝업업
        UIManager.ShowPopup("ItemInfoPop");
        Presenter.Send("ItemInfoPop", "OnItemType", iType);
        Presenter.Send("ItemInfoPop", "OnItemInfo", itemData);

        RectTransform rect = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 캔버스 크기
        Vector2 canvasSize = canvasRect.rect.size;
        int dir = rect.position.x < canvasSize.x / 2f ? 1 : -1;
        Vector3 pos = new Vector3(rect.position.x + (dir * (rect.sizeDelta.x / 2 + 202)), rect.position.y, 0);
        Presenter.Send("ItemInfoPop", "OnItemPos", pos);
        #endregion
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        bg.color = new Color(bgColor.r, bgColor.g, bgColor.b, bgAlpha);
        // SetBgAlpha(1f);
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
    public string GetItemRes()
    {
        return itemData.Res;
    }
}
