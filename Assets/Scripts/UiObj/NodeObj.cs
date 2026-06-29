using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;
using DG.Tweening;
public class NodeObj : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2Int pos;
    public int nType, eType; //node type, event type
    public Image icon1, icon2, highlight;
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
        icon1.gameObject.SetActive(true);
        icon2.gameObject.SetActive(false);
        if (clear)
            icon1.sprite = ResManager.GetSprite("exp_clear");
        else
        {

            switch (nType)
            {
                default:
                    icon1.gameObject.SetActive(false);
                    break;
                case 1:
                    icon1.sprite = ResManager.GetSprite("exp_boss");
                    break;
                case 2:
                    icon1.sprite = ResManager.GetSprite("exp_ran");
                    switch (eType)
                    {
                        case 1:
                            icon2.sprite = ResManager.GetSprite("exp_bt");
                            break;
                        case 2:
                            icon2.sprite = ResManager.GetSprite("exp_rest");
                            break;
                    }
                    break;
                case 3:
                    icon1.sprite = ResManager.GetSprite("exp_box");
                    break;
            }
        }
    }
    public void ShowCurEvtIcon()
    {
        float duration = 0.3f;
        icon1.DOKill();
        icon2.DOKill();
        icon1.gameObject.SetActive(true);
        SetAlpha(icon1, 1f);

        if (eType == 0 || eType == 99)
        {
            icon2.gameObject.SetActive(false);
            icon1.DOFade(0f, duration).SetUpdate(true)
                .OnComplete(() => icon1.gameObject.SetActive(false));
            return;
        }

        icon2.gameObject.SetActive(true);
        SetAlpha(icon2, 0f);
        DOTween.Sequence()
            .Append(icon1.DOFade(0f, duration))
            .AppendCallback(() =>
            {
                icon1.gameObject.SetActive(false);
                SetAlpha(icon2, 0f);
            })
            .Append(icon2.DOFade(1f, duration))
            .SetUpdate(true);
    }
    void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
    public void StateMoveable(bool on)
    {
        isMoveable = on;
    }
    public void SetClear()
    {
        Debug.Log("SetClear");
        isClear = true;
        icon1.gameObject.SetActive(true);
        icon2.gameObject.SetActive(false);
        icon1.sprite = ResManager.GetSprite("exp_clear");
        SetAlpha(icon1, 1f);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging
        && isMoveable && WorldMainUI.isActNode)
        {
            Presenter.Send("WorldMainUI", "ClickNode", pos);
        }
    }
    public void StateHighlight(bool on)
    {
        highlight.gameObject.SetActive(on);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isMoveable || !WorldMainUI.isActNode)
        {
            if (highlight.gameObject.activeSelf)
                StateHighlight(false);
            return;
        }
        StateHighlight(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isMoveable || !WorldMainUI.isActNode) return;
        StateHighlight(false);
    }
}
