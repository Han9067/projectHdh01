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
        if (type == 0)
            spr.color = new Color(1f, 1f, 1f, 0.5f);
    }
    public void SetColor(int type)
    {
        switch (type)
        {
            case 1:
                spr.color = Color.blue;
                break;
            case 2:
                spr.color = Color.red;
                break;
            case 3:
                spr.color = Color.green;
                break;
        }
    }
    public void SetPos(float x, float y, int x2, int y2)
    {
        transform.position = new Vector3(x, y, 0);
        xx = x2;
        yy = y2;
    }
    public void SetSpr(string res)
    {
        spr.sprite = ResManager.GetSprite(res);
    }
    public bool IsSkAvailable()
    {
        return spr.color == Color.green;
    }
}
