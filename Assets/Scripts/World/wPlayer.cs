using System.Collections;
using System.Collections.Generic;
using GB;
using UnityEngine;

public class wPlayer : MonoBehaviour
{
    [SerializeField] private GameObject frmBack, frmFront;

    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain;
    void Awake()
    {
        HumanAppearance.I.InitParts(ptSpr, ptMain);
    }
    void Start()
    {
        if (frmBack.GetComponent<SpriteRenderer>().sprite == null)
            frmBack.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_back");
        if (frmFront.GetComponent<SpriteRenderer>().sprite == null)
            frmFront.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_front");

        HumanAppearance.I.SetObjAppearance(0, ptSpr);
    }
}
