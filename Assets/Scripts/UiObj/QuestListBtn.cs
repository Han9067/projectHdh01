using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class QuestListBtn : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int uId, star, type;
    private string qName, popName;
    [SerializeField] private Image mImg;
    [SerializeField] private TextMeshProUGUI mTxtName;
    public GameObject sel;
    // [SerializeField] private Button btn;
    public void OnButtonClick()
    {
        Presenter.Send(popName, "ClickQuestListBtn", uId);
    }
    public void SetQuestListBtn(int u, int st, int tp, string name, string pName)
    {
        uId = u;
        star = st;
        type = tp;
        qName = name;
        popName = pName;
    }
    private void Start()
    {
        mTxtName.text = qName;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
            OnButtonClick();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        mImg.color = new Color(1, 1, 1, 0.5f);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        mImg.color = Color.clear;
    }
    public void StateTxtColor(int idx)
    {
        switch (idx)
        {
            case 1:
                mTxtName.color = Color.yellow;
                break;
            case 2:
                mTxtName.color = Color.green;
                break;
            case 3:
                mTxtName.color = Color.gray;
                break;
            default:
                mTxtName.color = Color.white;
                break;
        }
    }
}
