using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;

public class ChatMentBtn : MonoBehaviour
{
    [SerializeField] private Text mTxtName;
    [SerializeField] private Button btn;
    string sKey, tName;
    public void OnButtonClick()
    {
        Presenter.Send("ChatPop", "ChatMentBtn", sKey);
    }
    public void SetChatMentBtn(string key, string name)
    {
        sKey = key;
        tName = name;
    }
    private void Start()
    {
        btn.onClick.AddListener(OnButtonClick);

        mTxtName.text = tName;
    }
}
