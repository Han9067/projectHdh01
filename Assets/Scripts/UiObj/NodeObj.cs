using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;

public class NodeObj : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int x, y, nType, eType; //node type, event type
    public Image icon, highlight;
    public bool isClear = false;
    [SerializeField] private RectTransform rt;
    public void SetNode(int xx, int yy, int nt, int et, bool clear)
    {
        x = xx;
        y = yy;
        isClear = clear;
        nType = nt;
        eType = et;
        rt.anchoredPosition = new Vector2(-400 + x * 160, 240 - y * 160);
        // icon.sprite = ResManager.GetSprite("node_" + type);
    }
    public void SetClear()
    {
        isClear = true;
        // icon.sprite = ResManager.GetSprite("node_clear");
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
        //     Presenter.Send("NodePop", "ClickNode", x, y);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight.gameObject.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        highlight.gameObject.SetActive(false);
    }
}
