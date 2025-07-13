using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class InvenItemObj : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData itemData;
    private Vector3 oPos; //original position
    private Canvas cvs; //canvas
    private CanvasGroup cvsGrp; //canvas group
    
    public void SetItemData(ItemData data)
    {
        itemData = data;
        GetComponent<Image>().sprite = Resources.Load<Sprite>(itemData.Path);
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        oPos = transform.position;
        cvsGrp.alpha = 0.8f;
        cvsGrp.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Canvas의 렌더 모드에 따라 위치 계산
        if (cvs.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            transform.position = eventData.position;
        }
        else
        {
            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                transform as RectTransform, eventData.position, eventData.pressEventCamera, out globalMousePos))
            {
                transform.position = globalMousePos;
            }
        }
        CheckOverlapWithGrid();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // UnityEngine.Debug.Log("OnEndDrag");
        
        // 원래 상태로 복원
        cvsGrp.alpha = 1f;
        cvsGrp.blocksRaycasts = true;
        transform.position = oPos;
    }

    private void CheckOverlapWithGrid()
    {
        InvenPop invenPop = FindObjectOfType<InvenPop>();
        RectTransform dragRect = transform as RectTransform;
        resetAllGrids(invenPop);
        UnityEngine.Debug.Log(dragRect.position.x + " " + dragRect.position.y + " " + dragRect.sizeDelta.x + " " + dragRect.sizeDelta.y);
        // UnityEngine.Debug.Log(dragRect.sizeDelta.x + " " + dragRect.sizeDelta.y);
        // float wid = 
    }
    
    // 모든 그리드를 흰색으로 초기화하는 함수
    private void resetAllGrids(InvenPop invenPop)
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                if (invenPop.gridObj[x, y] != null)
                {
                    GridObj gridObjScript = invenPop.gridObj[x, y].GetComponent<GridObj>();
                    if (gridObjScript != null)
                    {
                        gridObjScript.ChangeToWhite();
                    }
                }
            }
        }
    }
}