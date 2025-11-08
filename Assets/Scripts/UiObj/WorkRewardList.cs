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
    public int val, id;
    private string imgKey, txtKey;
    void Start()
    {
        iconImg.sprite = ResManager.GetSprite(imgKey);
        UpdateText();
    }
    public void SetWorkReward(string img, string txt, int v, int i)
    {
        imgKey = img;
        txtKey = LocalizationManager.GetValue(txt);
        val = v;
        id = i;
    }
    public void UpdateVal(int v)
    {
        val = v;
        UpdateText();
    }
    void UpdateText()
    {
        if (id < 10000)
            Txt.text = string.Format(txtKey, GsManager.I.SkDataList[id].Name, val);
        else
            Txt.text = string.Format(txtKey, val);
    }
}
