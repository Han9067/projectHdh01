using UnityEngine;
using UnityEngine.EventSystems;
using GB;

public class GridBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int type;
    public void OnPointerEnter(PointerEventData eventData)
    {
        InvenPop.posType = type;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InvenPop.moveOn)
        {
            InvenPop.posType = -1;
            Presenter.Send("InvenPop", "ResetAllGrids");
        }
    }
}
