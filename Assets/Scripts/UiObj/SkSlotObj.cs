using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;

public class SkSlotObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public int skId, skType, useType, line, idx, slotType;
    // public bool isBlock = false;
    [SerializeField] private Image icon, high;
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
        }
        else
            icon.gameObject.SetActive(false);

        if (high.gameObject.activeSelf) high.gameObject.SetActive(false);
    }
    public void SetSkIdx(int line, int idx)
    {
        this.line = line;
        this.idx = idx;
    }
    // public void StateBlock(bool on)
    // {
    //     isBlock = on;
    //     // float val = on ? (80f / 255f) : 1f;
    //     // high.color = new Color(val, val, val, 0.5f);
    // }
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (slotType)
        {
            case 0:
                // if (isBlock) return;
                // if (high.color.a != 0.5f)
                //     high.color = new Color(1, 1, 1, 0.5f);
                high.gameObject.SetActive(true);
                break;
            case 1:
                SkillPop.slotLine = line;
                SkillPop.slotIdx = idx;
                high.gameObject.SetActive(true);
                break;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        switch (slotType)
        {
            case 0:
                // if (isBlock) return;
                high.gameObject.SetActive(false);
                break;
            case 1:
                SkillPop.slotLine = -1;
                SkillPop.slotIdx = -1;
                high.gameObject.SetActive(false);
                break;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (skId == 0) return;
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
        {
            if (!BattleCore.I.GetIsActPlayer())
            {
                GsManager.I.ShowTstMsg("Tst_NotCurUsingSk");
                return;
            }
            // if (isBlock)
            // {
            //     GsManager.I.ShowTstMsg("Tst_NotSk");
            //     return;
            // }
            BattleSkManager.I.ClickSk(skId, idx);
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
