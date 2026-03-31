using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GB;
public class MakeList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private Button button;
    public MakeData data;
    void Start()
    {
        nameTxt.text = LocalizationManager.GetValue(ItemManager.I.ItemDataList[data.ItemId].Name);
        button.onClick.AddListener(OnButtonClick);
    }
    public void SetMakeObj(MakeData makeData)
    {
        data = makeData;
    }
    public void StateMakeObj(bool on)
    {
        nameTxt.color = on ? Color.yellow : Color.white;
    }
    public void OnButtonClick()
    {
        InvenPop.curMakeId = data.MakeID;
        Presenter.Send("InvenPop", "ClickMakeList", data.Recipe);
    }
}
