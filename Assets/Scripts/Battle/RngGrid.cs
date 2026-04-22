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
    public void SetPos(float x, float y, int x2, int y2, string res)
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
