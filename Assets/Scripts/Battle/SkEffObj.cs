using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class SkEffObj : MonoBehaviour
{
    public SPRAnimation anim;
    [SerializeField] private SpriteRenderer spr;
    void Start()
    {
        spr = anim.gameObject.GetComponent<SpriteRenderer>();
    }
    public float GetHWid()
    {
        return spr.bounds.extents.x / 2f;
    }
}
