using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;
using UnityEngine.EventSystems;

public class TalkAskBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Text mTxtName;
    [SerializeField] private Button btn;
    private int idx;
    string sKey, tMent;
    public void OnButtonClick()
    {
        Presenter.Send("TalkPop", "OnClick", sKey);
    }
    public void SetTalkAskBtn(string key, string ment, int i)
    {
        sKey = key;
        tMent = LocalizationManager.GetValue(ment);
        idx = i;
    }
    private void Start()
    {
        btn.onClick.AddListener(OnButtonClick);

        mTxtName.text = $"{idx}. {tMent}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 버튼 위에 올라왔을 때 한 번 실행
        mTxtName.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 버튼 밖으로 나갔을 때 한 번 실행
        mTxtName.color = Color.black;
    }
}
