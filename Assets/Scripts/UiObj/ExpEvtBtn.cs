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
        switch (sKey)
        {
            case "StartBattle":
                txt.text = "전투를 시작한다";
                break;
            case "TakeRest":
                txt.text = "휴식을 취한다";
                break;
            case "OpenBox":
                txt.text = "상자를 연다";
                break;
        }
    }
    public void OnClick()
    {
        Presenter.Send("WorldMainUI", sKey);
    }
}
