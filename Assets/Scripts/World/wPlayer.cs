using System.Collections;
using System.Collections.Generic;
using GB;
using UnityEngine;

public class wPlayer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer frmBack, frmFront;

    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain;

    void Awake()
    {
        GsManager.I.SetObjParts(ptSpr, ptMain, true);
    }
    void Start()
    {
        if (frmBack.sprite == null)
            frmBack.sprite = ResManager.GetSprite("frm_back");
        if (frmFront.sprite == null)
            frmFront.sprite = ResManager.GetSprite("frm_front");

        GsManager.I.SetObjAppearance(0, ptSpr, true);
        GsManager.I.SetObjAllEqParts(0, ptSpr);
    }
}
