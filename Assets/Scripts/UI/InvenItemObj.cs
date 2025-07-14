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
        string str = $"{x}_{y}";
        Presenter.Send("InvenPop","ClickObj", str);
    }

    // public void OnBeginDrag(PointerEventData eventData)
    // {
    //     // oPos = transform.position;
    //     // cvsGrp.alpha = 0.8f;
    //     // cvsGrp.blocksRaycasts = false;
    //     // transform.SetAsLastSibling();

    // }

    // public void OnDrag(PointerEventData eventData)
    // {
    //     // // Canvas의 렌더 모드에 따라 위치 계산
    //     // if (cvs.renderMode == RenderMode.ScreenSpaceOverlay)
    //     // {
    //     //     transform.position = eventData.position;
    //     // }
    //     // else
    //     // {
    //     //     Vector3 globalMousePos;
    //     //     if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
    //     //         transform as RectTransform, eventData.position, eventData.pressEventCamera, out globalMousePos))
    //     //     {
    //     //         transform.position = globalMousePos;
    //     //     }
    //     // }
    //     // CheckOverlapWithGrid();
    // }

    // public void OnEndDrag(PointerEventData eventData)
    // {
    //     // // 원래 상태로 복원
    //     // cvsGrp.alpha = 1f;
    //     // cvsGrp.blocksRaycasts = true;
    //     // transform.position = oPos;
    //     // if(curType != 0)
    //     //     Presenter.Send("InvenPop","ItemPosChange", curType);
    //     Presenter.Send("InvenPop","Test");
    // }

    // private void CheckOverlapWithGrid()
    // {
    //     InvenPop invenPop = FindObjectOfType<InvenPop>();
    //     resetAllGrids(invenPop);

    //     RectTransform itemRect = transform as RectTransform;
    //     float wid = itemRect.sizeDelta.x - 32, hei = itemRect.sizeDelta.y - 32;
    //     float minX = itemRect.position.x - wid/2, minY = itemRect.position.y - hei/2;
    //     float maxX = itemRect.position.x + wid/2, maxY = itemRect.position.y + hei/2;
    //     int myId = itemData.itemId;
    //     int maxCnt = itemData.W * itemData.H;
    //     int gx = 0, gy = 0;
    //     int[] gridX = new int[maxCnt], gridY = new int[maxCnt];
    //     List<int> gridItemId = new List<int>();
    //     for (int y = 0; y < 10; y++)
    //     {
    //         bool isOverlapping = false;
    //         for (int x = 0; x < 10; x++)
    //         {
    //             RectTransform gRect = invenPop.gridObj[y, x].transform as RectTransform;
    //             float gw = gRect.sizeDelta.x, gh = gRect.sizeDelta.y;
    //             float gMinX = gRect.position.x - gw/2, gMinY = gRect.position.y - gh/2;
    //             float gMaxX = gRect.position.x + gw/2, gMaxY = gRect.position.y + gh/2;

    //             isOverlapping = !(maxX < gMinX || minX > gMaxX ||
    //                                 maxY < gMinY || minY > gMaxY);
    //             if (isOverlapping)
    //             {
    //                 gx = x;gy = y;
    //                 break;
    //             }
    //         }
    //         if (isOverlapping) break;
    //     }
    //     int cnt = 0;
    //     for(int h = 0; h < itemData.H; h++)
    //     {
    //         for(int w = 0; w < itemData.W; w++)
    //         {
    //             gridX[cnt] = gx + w;
    //             gridY[cnt] = gy + h;
    //             cnt++;
                
    //             if(gridItemId.IndexOf(PlayerManager.I.grids[gy + h][gx + w].slotId) == -1 && 
    //             PlayerManager.I.grids[gy + h][gx + w].slotId != -1 &&
    //             PlayerManager.I.grids[gy + h][gx + w].slotId != myId)
    //             {
    //                 gridItemId.Add(PlayerManager.I.grids[gy + h][gx + w].slotId);
    //             }
    //         }
    //     }
    //     curType = gridItemId.Count == 0 ? 0 : (gridItemId.Count > 1 ? 2 : 1); //0은 초록, 1은 노랑, 2는 빨강
    //     for(int i = 0; i < maxCnt; i++)
    //     {
    //         switch(curType)
    //         {
    //             case 0:
    //                 invenPop.gridObj[gridY[i], gridX[i]].GetComponent<GridObj>().ChangeToGreen();
    //                 break;
    //             case 1:
    //                 invenPop.gridObj[gridY[i], gridX[i]].GetComponent<GridObj>().ChangeToYellow();
    //                 break;
    //             case 2:
    //                 invenPop.gridObj[gridY[i], gridX[i]].GetComponent<GridObj>().ChangeToRed();
    //                 break;
    //         }
    //     }
    // }
    
    // // 모든 그리드를 흰색으로 초기화하는 함수
    // private void resetAllGrids(InvenPop invenPop)
    // {
    //     for (int y = 0; y < 10; y++)
    //     {
    //         for (int x = 0; x < 10; x++)
    //         {
    //             if (invenPop.gridObj[y, x] != null)
    //             {
    //                 GridObj gridObjScript = invenPop.gridObj[y, x].GetComponent<GridObj>();
    //                 gridObjScript.ChangeToWhite();
    //             }
    //         }
    //     }
    // }
}