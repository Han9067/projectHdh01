using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;

public class SkObj : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image mainImg;
    [SerializeField] private Image iconImg;
    [SerializeField] private SkData data;
    void Start()
    {
        iconImg.sprite = ResManager.GetSprite("skIcon_" + data.SkId);
    }
    public void SetSk(SkData skData)
    {
        data = skData;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Presenter.Send("SkillPop", "SelectSk", data);
        }
    }
}
