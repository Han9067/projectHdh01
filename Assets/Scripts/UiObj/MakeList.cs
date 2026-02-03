using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GB;
using UnityEngine.EventSystems;
public class MakeList : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI nameTxt;
    public MakeData data;
    void Start()
    {
        nameTxt.text = LocalizationManager.GetValue(ItemManager.I.ItemDataList[data.ItemId].Name);
    }
    public void SetMakeObj(MakeData makeData)
    {
        data = makeData;
    }
    public void StateMakeObj(bool on)
    {
        nameTxt.color = on ? Color.yellow : Color.white;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InvenPop.curMakeId = data.MakeID;
            Presenter.Send("InvenPop", "ClickMakeList", data.Recipe);
        }
    }
}
