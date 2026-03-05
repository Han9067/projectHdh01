using GB;
using UnityEngine;
using UnityEngine.EventSystems;
public class Prop2Obj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string nameStr;
    private int sort;
    [SerializeField] private SpriteRenderer spr;

    void Start()
    {
        spr.sprite = ResManager.GetSprite(nameStr);
        spr.sortingOrder = sort;
    }
    public void SetProp2(string str, int st)
    {
        nameStr = str;
        sort = st;
    }
    public void SetAlpha(float alpha)
    {
        spr.color = new Color(1, 1, 1, alpha);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
        SetAlpha(0.5f);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        SetAlpha(1f);
    }
}
