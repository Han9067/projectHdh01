using UnityEngine;
using UnityEngine.EventSystems;
using GB;

public class ShopIconObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string iKey = "";
    public void OnPointerEnter(PointerEventData eventData)
    {
        Presenter.Send("ShopInvenPop", "OpenIconInfo", iKey);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Presenter.Send("ShopInvenPop", "CloseIconInfo");
    }
}
