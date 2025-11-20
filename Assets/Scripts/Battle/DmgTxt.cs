using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DmgTxt : MonoBehaviour
{
    public TextMeshPro dmgTxt;
    private int dmg;
    void Start()
    {
        dmgTxt.text = dmg.ToString();
        OnTween();
    }
    public void SetDmgTxt(int d, Vector3 pos)
    {
        dmg = d;
        transform.position = pos;
    }
    public void ShowDmgTxt(int d, Vector3 pos)
    {
        dmg = d;
        dmgTxt.text = dmg.ToString();
        gameObject.SetActive(true);
        transform.position = pos;
        OnTween();
    }
    private void OnTween()
    {
        DOTween.Sequence().SetAutoKill(true).Append(transform.DOMoveY(transform.position.y + 0.6f, 0.5f).SetEase(Ease.OutQuad))
            .Join(dmgTxt.DOFade(0f, 1f))
            .Join(transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
            .Append(transform.DOScale(1f, 0.2f))
            .OnComplete(() =>
            {
                dmgTxt.color = new Color(1f, 1f, 1f, 1f);
                gameObject.SetActive(false);
            });
    }
}
