using UnityEngine;
using UnityEngine.UI;
using GB;

public class NodeObj : MonoBehaviour
{
    public int x, y, type;
    public Image icon;
    public bool isClear = false;
    [SerializeField] private RectTransform rt;
    public void SetNode(int xx, int yy, int t)
    {
        x = xx;
        y = yy;
        type = t;
        rt.anchoredPosition = new Vector2(-400 + x * 160, 240 - y * 160);
        // icon.sprite = ResManager.GetSprite("node_" + type);
    }
    public void SetClear()
    {
        isClear = true;
        // icon.sprite = ResManager.GetSprite("node_clear");
    }
}
