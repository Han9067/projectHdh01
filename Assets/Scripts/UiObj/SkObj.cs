using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;

public class SkObj : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image mainImg;
    [SerializeField] private Image iconImg;
    [SerializeField] private SkData data;
    void Start()
    {
        iconImg.sprite = ResManager.GetSprite("skIcon_" + data.SkId);
        switch (data.UseType)
        {
            case 0:
                mainImg.color = Color.gray;
                break;
            case 1:
                mainImg.color = Color.red;
                break;
            case 2:
                mainImg.color = Color.blue;
                break;
            case 3:
                mainImg.color = Color.green;
                break;
            default:
                mainImg.color = Color.white;
                break;
        }
    }
    public void SetSk(SkData skData)
    {
        data = skData;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
            Presenter.Send("SkillPop", "SelectSk", data);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (data.SkType == 0) return;
        Presenter.Send("SkillPop", "DragSk", data);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (data.SkType == 0) return;
        Presenter.Send("SkillPop", "MoveSelSkObj");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (data.SkType == 0) return;
        Presenter.Send("SkillPop", "EndDragSk", data.SkId);
    }
}
