using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;

public class SelSkObj : MonoBehaviour
{
    [SerializeField] private Image icon;
    public void SetSelSkObj(SkData skData)
    {
        icon.sprite = ResManager.GetSprite("skIcon_" + skData.SkId);
    }
}
