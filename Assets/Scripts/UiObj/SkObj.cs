using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;

public class SkObj : MonoBehaviour
{
    [SerializeField] private Image mainImg;
    [SerializeField] private Image iconImg;
    [SerializeField] private SkData data;
    void Start()
    {
        // mainImg.sprite = ResManager.GetSprite(skData.MainImg);
        // iconImg.sprite = ResManager.GetSprite(skData.IconImg);
        iconImg.sprite = ResManager.GetSprite("skIcon_" + data.SkId);
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }
    public void SetSk(SkData skData)
    {
        data = skData;
    }
    void OnButtonClick()
    {
        // Presenter.Send("SkillPop", "SelectSk", data);
    }
}
