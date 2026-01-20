using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;

public class SkSlot : MonoBehaviour, IPointerEnterHandler
{
    public int skId, skType, useType, line, idx, slotType;
    [SerializeField] private Image slot, icon, sel;
    private void Start()
    {
        SetSkSlot(skId, skType, useType);
    }
    public void SetSkSlot(int skId, int skType, int useType)
    {
        this.skId = skId;
        this.skType = skType;
        this.useType = useType;
        if (skId > 0)
        {
            icon.gameObject.SetActive(true);
            icon.sprite = ResManager.GetSprite("skIcon_" + skId);
            switch (useType)
            {
                case 0:
                    slot.color = Color.gray;
                    break;
                case 1:
                    slot.color = Color.red;
                    break;
                case 2:
                    slot.color = Color.blue;
                    break;
                case 3:
                    slot.color = Color.green;
                    break;
                default:
                    slot.color = Color.white;
                    break;
            }
        }
        else
        {
            icon.gameObject.SetActive(false);
            slot.color = Color.white;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (slotType)
        {
            case 0:
                break;
            case 1:
                if (eventData.pointerDrag != null)
                    sel.gameObject.SetActive(true);
                break;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        switch (slotType)
        {
            case 0:
                break;
            case 1:
                if (eventData.pointerDrag != null)
                    sel.gameObject.SetActive(false);
                break;
        }
    }
}
