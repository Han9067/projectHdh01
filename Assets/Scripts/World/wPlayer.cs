using System.Collections;
using System.Collections.Generic;
using GB;
using UnityEngine;

public class wPlayer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer frmBack, frmFront;

    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain;

    private PtType[] sOrder; // 레이어 순서

    void Awake()
    {
        AppearanceManager.I.InitParts(ptSpr, ptMain);
    }
    void Start()
    {
        if (frmBack.sprite == null)
            frmBack.sprite = ResManager.GetSprite("frm_back");
        if (frmFront.sprite == null)
            frmFront.sprite = ResManager.GetSprite("frm_front");

        AppearanceManager.I.SetObjAppearance(0, ptSpr);

        CacheLayerOrder();
        SetObjLayer(ObjLayerManager.I.GetObjLayer(transform.position.y));
    }
    void CacheLayerOrder()
    {
        int childCount = ptMain.transform.childCount;
        sOrder = new PtType[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = ptMain.transform.GetChild(i);
            if (System.Enum.TryParse<PtType>(child.name, out PtType ptType))
                sOrder[i] = ptType;
        }
    }
    public void SetObjLayer(int y)
    {
        frmBack.sortingOrder = y++;
        frmFront.sortingOrder = y++;

        for (int i = 0; i < sOrder.Length; i++)
            ptSpr[sOrder[i]].sortingOrder = y + i;
    }
}
