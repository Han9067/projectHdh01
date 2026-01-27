using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;

public class SkSlotObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
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

        if (sel.gameObject.activeSelf) sel.gameObject.SetActive(false);
    }
    public void SetSkIdx(int line, int idx)
    {
        this.line = line;
        this.idx = idx;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (slotType)
        {
            case 0:
                if (sel.color.a == 0)
                    sel.color = new Color(1, 1, 1, 40f / 255f);
                sel.gameObject.SetActive(true);
                break;
            case 1:
                SkillPop.slotLine = line;
                SkillPop.slotIdx = idx;
                sel.gameObject.SetActive(true);
                break;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        switch (slotType)
        {
            case 0:
                sel.gameObject.SetActive(false);
                break;
            case 1:
                SkillPop.slotLine = -1;
                SkillPop.slotIdx = -1;
                sel.gameObject.SetActive(false);
                break;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (skId == 0) return;
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
        {
            // Debug.Log(skId);
            BattleCore.I.ClickSk(skId);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotType == 0 || skId == 0) return;
        Debug.Log("BeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (slotType == 0 || skId == 0) return;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (slotType == 0 || skId == 0) return;
        Debug.Log("EndDrag");
    }
    public void AnimSel()
    {

    }
}
