using UnityEngine;
using UnityEngine.UI;
using GB;
using TMPro;

public class ExpEvtBtn : MonoBehaviour
{
    private string sKey;
    [SerializeField] private Button btn;
    [SerializeField] private TextMeshProUGUI txt;
    private void Awake()
    {
        btn.onClick.AddListener(OnClick);
    }
    public void SetEvtBtn(string key)
    {
        sKey = key;
        txt.text = LocalizationManager.GetValue(sKey);
    }
    public void OnClick()
    {
        Presenter.Send("WorldMainUI", sKey);
    }
}
