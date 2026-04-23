using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class RngGrid : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spr;
    public int xx, yy;
    public void SetColor(Color color)
    {
        spr.color = color;
    }
    public void SetData(int type = 0)
    {
        spr.sortingOrder = type;
        switch (type)
        {
            case 0:
                spr.color = new Color(1f, 1f, 1f, 0.5f);
                break;
            case 1:
                spr.color = new Color(1f, 0f, 0f, 1.0f);
                break;//오브젝트 범위 그리드
            case 2:
                spr.color = new Color(0f, 0f, 1f, 1.0f);
                break;//스킬 범위 그리드
        }
    }
    public void SetPos(float x, float y, int x2, int y2, string res, int order)
    {
        transform.position = new Vector3(x, y, 0);
        xx = x2;
        yy = y2;
        spr.sprite = ResManager.GetSprite(res);
    }
    public bool IsSkAvailable()
    {
        return spr.color == Color.green;
    }
}
