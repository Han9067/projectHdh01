using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;

public class SelSkObj : MonoBehaviour
{
    [SerializeField] private Image icon, bg;
    public void SetSelSkObj(SkData skData)
    {
        icon.sprite = ResManager.GetSprite("skIcon_" + skData.SkId);
        switch (skData.UseType)
        {
            case 0:
                bg.color = Color.gray;
                break;
            case 1:
                bg.color = Color.red;
                break;
            case 2:
                bg.color = Color.blue;
                break;
            case 3:
                bg.color = Color.green;
                break;
            default:
                bg.color = Color.white;
                break;
        }
    }
}
