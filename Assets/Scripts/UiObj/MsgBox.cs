using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MsgBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] msgTexts; //0~7까지 존재.
    private int idx = 0;
    void Start()
    {
        foreach (var v in msgTexts)
            v.gameObject.SetActive(false);
        idx = 0;
    }
    public void ShowMsg(string msg)
    {
        //y좌표가 -25이면 최상단에 있기에 UI오브젝트를 최하단으로 내려준다.
        //exampleObject.transform.SetAsLastSibling();
        if (IsAllActive() && msgTexts[idx].transform.position.y == -25f)
            msgTexts[idx].transform.SetAsLastSibling();
        msgTexts[idx].text = msg;
        msgTexts[idx].gameObject.SetActive(true);
        idx++;
        if (idx >= msgTexts.Length) idx = 0;
    }
    private bool IsAllActive()
    {
        foreach (var v in msgTexts)
            if (!v.gameObject.activeSelf)
                return false;
        return true;
    }
}
