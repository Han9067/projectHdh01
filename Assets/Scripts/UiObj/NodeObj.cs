using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;

public class NodeObj : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2Int pos;
    public int nType, eType; //node type, event type
    public Image icon, highlight;
    public bool isClear = false;
    public bool isMoveable = false;
    [SerializeField] private RectTransform rt;
    public void SetNode(int xx, int yy, int nt, int et, bool clear)
    {
        pos = new Vector2Int(xx, yy);
        isClear = clear;
        nType = nt;
        eType = et;
        rt.anchoredPosition = new Vector2(-400 + pos.x * 160, 240 - pos.y * 160);
        // icon.sprite = ResManager.GetSprite("node_" + type);
    }
    public void StateMoveable(bool on)
    {
        isMoveable = on;
    }
    public void SetClear()
    {
        isClear = true;
        // icon.sprite = ResManager.GetSprite("node_clear");
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging
        && isMoveable && !WorldMainUI.isMoveNode)
            Presenter.Send("WorldMainUI", "ClickNode", pos);
    }
    public void StateHighlight(bool on)
    {
        highlight.gameObject.SetActive(on);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isMoveable)
        {
            if (highlight.gameObject.activeSelf)
                StateHighlight(false);
            return;
        }
        StateHighlight(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isMoveable) return;
        StateHighlight(false);
    }
}
