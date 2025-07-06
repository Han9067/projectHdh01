using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 추가
using GB;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemInfo itemInfo;
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnItemClicked);
    }
    
    public void SetItemInfo(ItemInfo info)
    {
        itemInfo = info;
        //Debug.Log(itemInfo.itemId);
    }

    void OnItemClicked()
    {
        UIManager.ShowPopup("ItemSelPop");
        // Vector3 objPos = transform.position;
        // string str = $"{itemInfo.Price}-{objPos.x}-{objPos.y}";
        // GB.Presenter.Send("ItemSelPop", "Buy", str);
        Vector3 mousePos = Input.mousePosition;
        string str = $"{itemInfo.Price}-{mousePos.x}-{mousePos.y}";
        GB.Presenter.Send("ItemSelPop", "Buy", str);
    }

    // 마우스가 버튼 위에 올라갔을 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("마우스가 버튼 위에 올라갔습니다!");
    }

    // 마우스가 버튼에서 벗어났을 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("마우스가 버튼에서 벗어났습니다!");
    }
}