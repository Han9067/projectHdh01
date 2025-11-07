using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GB;
using Unity.IO.LowLevel.Unsafe;

public class WorkRewardList : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI Txt;
    private int val, skId;
    private string imgKey, txtKey;
    void Start()
    {
        iconImg.sprite = ResManager.GetSprite(imgKey);
        if (skId == 0)
        {
            Txt.text = string.Format(LocalizationManager.GetValue(txtKey), val);
        }
        else
        {
            Txt.text = string.Format(LocalizationManager.GetValue(txtKey), GsManager.I.GetSkillName(skId), val);
        }
    }
    public void SetWorkReward(string img, string txt, int v, int sk)
    {
        imgKey = img;
        txtKey = txt;
        val = v;
        skId = sk;
    }
}
