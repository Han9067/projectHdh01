using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GB;

public class InvenItemObj : MonoBehaviour, IPointerClickHandler
{
    public ItemData itemData;
    private Vector3 oPos; //original position
    private Canvas cvs; //canvas
    private CanvasGroup cvsGrp; //canvas group
    // private int curType = 0;
    public int x, y;
    public void SetItemData(ItemData data, int xx, int yy)
    {
        itemData = data;
        GetComponent<Image>().sprite = Resources.Load<Sprite>(itemData.Path);
        x = xx; y = yy;
    }

    void OnEnable()
    {
        oPos = transform.position;
        cvs = GetComponentInParent<Canvas>();
        cvsGrp = GetComponent<CanvasGroup>();
        
        // CanvasGroup이 없으면 자동으로 추가
        if (cvsGrp == null)
            cvsGrp = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        string str = $"{x}_{y}_{itemData.ItemId}_{itemData.Type}";
        Presenter.Send("InvenPop","ClickObj", str);
    }
}