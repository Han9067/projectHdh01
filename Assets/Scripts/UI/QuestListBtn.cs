using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;
public class QuestListBtn : MonoBehaviour
{
    public int uId, star;
    private string qName;
    public List<GameObject> starList = new List<GameObject>();
    [SerializeField] private Text mTxtName;
    [SerializeField] private Button btn;
    public void OnButtonClick()
    {
        // Debug.Log("QuestListBtn 클릭되었습니다!");
        Presenter.Send("GuildQuestPop", "ClickQuestListBtn", uId);
    }
    public void SetQuestListBtn(int u, int st, string name)
    {
        uId = u;
        star = st;
        qName = name;
    }
    private void Start()
    {
        btn.onClick.AddListener(OnButtonClick);

        mTxtName.text = qName;
        for (int i = 0; i < 10; i++)
            starList[i].SetActive(i < star);
    }
    private void OnDestroy()
    {
        btn.onClick.RemoveListener(OnButtonClick);
    }
}
