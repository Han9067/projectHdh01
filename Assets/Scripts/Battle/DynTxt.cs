using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class DynTxt : MonoBehaviour
{
    public TextMeshProUGUI dynTxt;
    private float time;
    public void ShowDynTxt(string txt, Vector3 pos, float t)
    {
        dynTxt.text = txt;
        gameObject.SetActive(true);
        transform.position = pos;
        time = t;
        OnTween();
    }
    private void OnTween()
    {
        DOTween.Sequence().SetAutoKill(true).Append(transform.DOMoveY(transform.position.y + 0.6f, time).SetEase(Ease.OutQuad))
            .Join(dynTxt.DOFade(0f, 1f))
            .Join(transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
            .Append(transform.DOScale(1f, 0.2f))
            .OnComplete(() =>
            {
                dynTxt.color = new Color(1f, 1f, 1f, 1f);
                gameObject.SetActive(false);
            });
    }
}
