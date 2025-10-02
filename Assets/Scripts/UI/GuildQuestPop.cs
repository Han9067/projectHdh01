using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.UI;

public class GuildQuestPop : UIScreen
{
    [SerializeField] private Transform parent;
    public Slider mSlider;
    private List<GameObject> questBtn = new List<GameObject>(); //퀘스트 버튼 오브젝트
    private Dictionary<int, QuestInstData> qList = new Dictionary<int, QuestInstData>(); //생성된 퀘스트 데이터
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("GuildQuestPop", this);
    }
    private void OnDisable()
    {
        Presenter.UnBind("GuildQuestPop", this);
        foreach (GameObject btn in questBtn)
        {
            if (btn != null)
                Destroy(btn);
        }
        questBtn.Clear();
        qList.Clear();
    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }

    public void OnButtonClick(string key)
    {
        switch (key)
        {
            case "OnQuestClose":
                Close();
                break;
            case "OnQuestAccept":
                Debug.Log("QuestAccept");
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SettingQuestPop":
                //MyRankBadge
                mTexts["GradeGgVal"].text = PlayerManager.I.pData.GradeExp.ToString() + " / " + PlayerManager.I.pData.GradeNext.ToString();
                float gg = PlayerManager.I.pData.GradeExp / PlayerManager.I.pData.GradeNext * 100;
                if (gg > 100) gg = 100;
                mSlider.value = gg;
                CreateQuestBtn(data.Get<int>());
                break;
            case "ClickQuestListBtn":
                int qid = data.Get<int>();
                Debug.Log(qid);
                break;
        }
    }

    public override void Refresh()
    {
    }

    void CreateQuestBtn(int cityId)
    {
        var qData = QuestManager.I.CityQuest[cityId];
        for (int i = 1; i <= qData.Count; i++)
        {
            GameObject obj = Instantiate(ResManager.GetGameObject("QuestBtn"), parent);
            obj.name = "QuestBtn_" + qData[i].Qid;
            obj.GetComponent<QuestListBtn>().SetQuestListBtn(qData[i].Qid, qData[i].Star, qData[i].Name);
            questBtn.Add(obj);
            qList[qData[i].Qid] = qData[i];
        }
    }

}