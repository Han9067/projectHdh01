using GB;
using UnityEngine;
using UnityEngine.EventSystems;
public class PropObj : MonoBehaviour
{
    private string nameStr;
    private int sort;
    public float curAlpha = 1f;
    public bool onMouse = false, onObj = false;
    public Bounds bounds;
    [SerializeField] private SpriteRenderer spr;

    void Start()
    {
        spr.sprite = ResManager.GetSprite(nameStr);
        spr.sortingOrder = sort;
        bounds = spr.bounds;
    }
    public void SetProp(string str, int st)
    {
        nameStr = str;
        sort = st;
    }
    public void SetAlpha(float alpha)
    {
        spr.color = new Color(1, 1, 1, alpha);
        curAlpha = alpha;
    }
}
