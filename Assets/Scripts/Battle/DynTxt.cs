using UnityEngine;
using TMPro;
using DG.Tweening;

public class DynTxt : MonoBehaviour
{
    public TextMeshProUGUI txt;
    public GameObject tObj;
    [SerializeField] private float duration = 4f;
    private Vector3 offset;

    public void ShowDynTxt(string text, GameObject obj, int id)
    {
        DOTween.Kill(this);
        transform.DOKill();
        txt.DOKill();
        offset = GetOffset(id);
        tObj = obj;
        txt.text = text;
        gameObject.SetActive(true);

        if (tObj != null)
            transform.position = tObj.transform.position + offset;

        OnTween();
    }

    private void OnTween()
    {
        if (tObj == null)
        {
            gameObject.SetActive(false);
            return;
        }

        DOTween.To(() => 0f, _ => { }, 1f, duration)
            .SetEase(Ease.Linear)
            .SetTarget(this)
            .OnUpdate(() =>
            {
                if (tObj == null || !tObj.activeInHierarchy)
                {
                    DOTween.Kill(this);
                    gameObject.SetActive(false);
                    return;
                }
                transform.position = tObj.transform.position + offset;
            })
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
    private Vector3 GetOffset(int objId)
    {
        if (objId == 1000)
            return new Vector3(0f, 1.4f, 0f);
        else
            return new Vector3(0f, 1.41f, 0f); //추후 세분화
    }
}
