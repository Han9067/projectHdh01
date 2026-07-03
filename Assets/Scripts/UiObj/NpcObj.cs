using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NpcObj : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public NpcData data;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private Image high;
    public void SetNpcData(NpcData npcData)
    {
        data = npcData;
        nameTxt.text = data.Name;
        high.gameObject.SetActive(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging) OnButtonClick();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        high.gameObject.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        high.gameObject.SetActive(false);
    }
    private void OnButtonClick()
    {
        Debug.Log("OnButtonClick");
    }
    // private Color GetColor(int g)
    // {
    //     switch (g)
    //     {
    //         default:
    //     }
    // }
}
