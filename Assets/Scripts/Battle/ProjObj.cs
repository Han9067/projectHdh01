using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class ProjObj : MonoBehaviour
{
    private SpriteRenderer spr;
    public void SetProj(string name)
    {
        if (spr == null)
            spr = GetComponent<SpriteRenderer>();
        spr.sprite = ResManager.GetSprite(name);
    }
    public float GetHWid()
    {
        return spr.bounds.extents.x / 2f;
    }
}
