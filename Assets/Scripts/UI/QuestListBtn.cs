using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;
public class QuestListBtn : MonoBehaviour
{
    public int qId, star;
    public List<GameObject> starList = new List<GameObject>();
    public Text qName;
    public void OnButtonClick()
    {
        Debug.Log("QuestListBtn 클릭되었습니다!");
        Presenter.Send("GuildQuestPop", "ClickQuestListBtn", qId);
    }
    public void SetQuestListBtn(int qId, int star, string name)
    {
        this.qId = qId;
        this.star = star;
        qName.text = name;
        for (int i = 0; i < 10; i++)
            starList[i].SetActive(i < star);
    }
}
