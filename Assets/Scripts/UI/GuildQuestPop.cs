using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.UI;

public class GuildQuestPop : UIScreen
{
    [SerializeField] private Transform parent;
    public Slider mSlider;
    private List<GameObject> questBtn = new List<GameObject>(); //퀘스트 버튼 오브젝트
    private List<QuestInstData> qList = new List<QuestInstData>(); //생성된 퀘스트 데이터

    private int curId = 0, cityId = 1, qIdx = 0;
    private bool isAccept = false;
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
        InitQuestList();
        isAccept = false;
        cityId = 1;
    }
    void InitQuestList()
    {
        foreach (GameObject btn in questBtn)
        {
            if (btn != null)
                Destroy(btn);
        }
        questBtn.Clear();
        qList.Clear();
        qIdx = 0;
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
            case "OnQstClose":
                Close();
                break;
            case "OnQstAccept":
                if (isAccept) return;
                PlayerManager.I.pData.QuestList.Add(qList[curId]);
                int n = PlayerManager.I.pData.QuestList.Count - 1;
                PlayerManager.I.pData.QuestList[n].IsAccept = true;
                PlayerManager.I.pData.QuestList[n].sDay = GsManager.I.tDay;
                PlayerManager.I.pData.QuestList[n].eDay = GsManager.I.tDay + qList[curId].Days;
                QuestManager.I.CityQuest[cityId].Remove(qList[curId].Qid);
                InitQuestList();
                CreateMyQuest();
                CreateCityQuest();
                questBtn[n].GetComponent<QuestListBtn>().OnButtonClick();
                mTexts["MyQuestVal"].text = (n + 1).ToString() + " / " + PlayerManager.I.pData.QuestMax.ToString();
                break;
            case "OnUpgrade":
                Debug.Log("Upgrade");
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SettingQuestPop":
                //MyRankBadge
                qIdx = 0;
                mTexts["GradeGgVal"].text = PlayerManager.I.pData.GradeExp.ToString() + " / " + PlayerManager.I.pData.GradeNext.ToString();
                float gg = PlayerManager.I.pData.GradeExp / PlayerManager.I.pData.GradeNext * 100;
                if (gg > 100) gg = 100;
                mSlider.value = gg;
                bool not = true;
                if (PlayerManager.I.pData.QuestList.Count > 0)
                {
                    CreateMyQuest();
                    not = false;
                }
                mTexts["MyQuestVal"].text = PlayerManager.I.pData.QuestList.Count.ToString() + " / " + PlayerManager.I.pData.QuestMax.ToString();
                cityId = data.Get<int>();
                if (QuestManager.I.CityQuest[cityId].Count > 0)
                {
                    CreateCityQuest();
                    not = false;
                    questBtn[0].GetComponent<QuestListBtn>().OnButtonClick();
                }
                if (not)
                    NotQuestList(); //퀘스트가 없을떄 표시
                break;
            case "ClickQuestListBtn":
                curId = data.Get<int>();
                UpdateStars(qList[curId].Star);
                mTexts["DescVal"].text = qList[curId].Desc;
                mTexts["ExpVal"].text = qList[curId].Exp.ToString();
                mTexts["CrownVal"].text = qList[curId].Crown.ToString();
                mTexts["GdExpVal"].text = qList[curId].GradeExp.ToString();
                mTexts["DaysVal"].text = qList[curId].Days.ToString();
                for (int i = 0; i < questBtn.Count; i++)
                    questBtn[i].GetComponent<Image>().color = qList[i].IsAccept ? Color.gray : Color.white;
                questBtn[curId].GetComponent<Image>().color = Color.yellow;
                isAccept = qList[curId].IsAccept;
                mButtons["OnQstAccept"].gameObject.SetActive(!isAccept);
                mGameObject["tObjProgress"].SetActive(isAccept);
                break;
        }
    }

    public override void Refresh()
    {
    }
    void CreateMyQuest()
    {
        var qData = PlayerManager.I.pData.QuestList;
        for (int i = 0; i < qData.Count; i++)
        {
            GameObject obj = Instantiate(ResManager.GetGameObject("GuildQstBtn"), parent);
            obj.name = "MyQuest_" + qIdx;
            obj.GetComponent<QuestListBtn>().SetQuestListBtn(qIdx, qData[i].Star, qData[i].QType, LocalizationManager.GetValue(qData[i].Name), "GuildQuestPop");
            questBtn.Add(obj);
            qList.Add(qData[i]);
            qIdx++;
        }
    }
    void CreateCityQuest()
    {
        var qData = QuestManager.I.CityQuest[cityId];
        foreach (var v in qData)
        {
            GameObject obj = Instantiate(ResManager.GetGameObject("GuildQstBtn"), parent);
            obj.name = "CityQuest_" + qIdx;
            obj.GetComponent<QuestListBtn>().SetQuestListBtn(qIdx, v.Value.Star, v.Value.QType, LocalizationManager.GetValue(v.Value.Name), "GuildQuestPop");
            questBtn.Add(obj);
            qList.Add(v.Value);
            qIdx++;
        }
    }
    void UpdateStars(int star)
    {
        for (int i = 1; i <= 10; i++)
            mGameObject[$"gqPopStar{i}"].SetActive(i <= star);
    }
    void NotQuestList()
    {

    }
}