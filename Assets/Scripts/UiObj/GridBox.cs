using UnityEngine;
using UnityEngine.EventSystems;
using GB;

public class GridBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int type;
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (type)
        {
            case 0:
                InvenPop.intoIvGrids = true;
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        switch (type)
        {
            case 0:
                InvenPop.intoIvGrids = false;
                if (InvenPop.moveOn)
                    Presenter.Send("InvenPop", "ResetAllGrids");
                break;
        }
    }
}
