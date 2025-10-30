using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GB;

public class SelectList : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private string sKey = "";
    [SerializeField] private Text text;
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }
    public void OnButtonClick()
    {
        Presenter.Send("SelectPop", sKey);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 버튼 위에 올라왔을 때 한 번 실행
        text.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 버튼 밖으로 나갔을 때 한 번 실행
        text.color = Color.white;
    }

}
