using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class bPlayer : MonoBehaviour
{
    Dictionary<PtType, SpriteRenderer> ptSpr = new Dictionary<PtType, SpriteRenderer>();
    public GameObject ptMain;
    void Awake()
    {
        InitParts();
    }
    void Start()
    {
        setAppearance();
    }
    void InitParts()
    {
        foreach (PtType PtType in System.Enum.GetValues(typeof(PtType)))
        {
            string partName = PtType.ToString();
            ptSpr[PtType] = ptMain.transform.Find(partName).GetComponent<SpriteRenderer>();
        }
    }
    void setAppearance()
    {
        var pData = PlayerManager.I.pData;
        Color skinColor = CharColor.GetSkinColor(pData.Skin);
        Color hairColor = CharColor.GetHairColor(pData.HairColor);
        PtType[] skinParts = new PtType[] { PtType.Face, PtType.Ear, PtType.BaseBody, PtType.BaseHand1A, PtType.BaseHand1A2, PtType.BaseHand1B, PtType.BaseHand2, PtType.BaseBoth };
        foreach (PtType skinPart in skinParts)
            ptSpr[skinPart].color = skinColor;
        PtType[] hairParts = new PtType[] { PtType.FrontHair1, PtType.FrontHair2, PtType.BackHair };
        foreach (PtType hairPart in hairParts)
            ptSpr[hairPart].color = hairColor;
        switch (pData.Hair)
        {
            case 1:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(false);
                ptSpr[PtType.BackHair].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair1].sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                ptSpr[PtType.BackHair].sprite = ResManager.GetSprite("Hair_2_" + pData.Hair);
                ptSpr[PtType.FrontHair1].color = hairColor;
                ptSpr[PtType.BackHair].color = hairColor;
                break;
            case 2:
            case 3:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(false);
                ptSpr[PtType.BackHair].gameObject.SetActive(false);
                ptSpr[PtType.FrontHair1].sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                ptSpr[PtType.FrontHair1].color = hairColor;
                break;
            case 100:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(false);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(true);
                ptSpr[PtType.BackHair].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                ptSpr[PtType.FrontHair2].color = hairColor;
                ptSpr[PtType.BackHair].sprite = ResManager.GetSprite("Hair_2_" + pData.Hair);
                ptSpr[PtType.BackHair].color = hairColor;
                break;
        }
    }
}
