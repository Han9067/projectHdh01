using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
public class QuestListBtn : MonoBehaviour
{
    public int qId, grade;
    public void OnButtonClick()
    {
        Debug.Log("QuestListBtn 클릭되었습니다!");
        // Presenter.Send("QuestListBtn", "ClickQuestListBtn", qId);
    }
}
