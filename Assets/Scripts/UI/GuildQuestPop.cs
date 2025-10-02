using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.UI;

public class GuildQuestPop : UIScreen
{
    [SerializeField] private Transform parent;
    public Slider mSlider;
    private List<GameObject> questBtn = new List<GameObject>();
    private Dictionary<int, QuestInstData> qList = new Dictionary<int, QuestInstData>();
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("GuildQuestPop", this);
        questBtn.Clear();
    }
    private void OnDisable()
    {
        Presenter.UnBind("GuildQuestPop", this);

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
                Debug.Log("QuestAccept 클릭되었습니다!");
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
        }
    }

    public override void Refresh()
    {
    }

    void CreateQuestBtn(int cityId)
    {
        qList.Clear();
        questBtn.Clear();
        //CityQuest[cityID][i]
        var qData = QuestManager.I.CityQuest[cityId];

        // for (int i = 0; i < qData.Count; i++)
        // {
        //     // GameObject btn = Instantiate(mGameObject["QuestBtn"], parent);
        //     // btn.GetComponent<QuestListBtn>().SetQuestListBtn(qData[i].Qid, qData[i].Star, qData[i].Name);
        //     // questBtn.Add(btn);
        //     // qList[qData[i].Qid] = qData[i];
        //     Debug.Log(qData[i].Qid + " " + qData[i].Star + " " + qData[i].Name);
        // }
    }

}