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
        Debug.Log("OnBeginDrag");
        Presenter.Send("SkillPop", "DragSk");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        Presenter.Send("SkillPop", "EndDragSk");
    }
}
