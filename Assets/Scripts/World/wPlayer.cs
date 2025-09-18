using System.Collections;
using System.Collections.Generic;
using GB;
using UnityEngine;

public class wPlayer : MonoBehaviour
{
    [SerializeField] private GameObject frmBack, frmFront;
    void Start()
    {
        if (frmBack.GetComponent<SpriteRenderer>().sprite == null)
            frmBack.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_back");
        if (frmFront.GetComponent<SpriteRenderer>().sprite == null)
            frmFront.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite("frm_front");
    }
}
