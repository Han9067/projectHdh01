using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class GridObj : MonoBehaviour
{
    public Sprite whiteImg;
    public Sprite greenImg;
    public Sprite yellowImg;
    public Sprite redImg;
    private Image imgComp;
    private void Start()
    {
        imgComp = GetComponent<Image>();
        imgComp.sprite = whiteImg;
    }
    public void ChangeToWhite() => imgComp.sprite = whiteImg;
    public void ChangeToGreen() => imgComp.sprite = greenImg;
    public void ChangeToYellow() => imgComp.sprite = yellowImg;
    public void ChangeToRed() => imgComp.sprite = redImg;
    public void SetSprite(Sprite spr) => imgComp.sprite = spr;
}
