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
    private int qstState; //현재 퀘스트 상태 0: 수락 가능, 1: 수락 완료, 2: 퀘스트 완료
    private bool isUpgrade = false; //업그레이드 가능 여부
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
        qstState = 0;
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
        isUpgrade = false;
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
                if (qstState == 1) return;
                PlayerManager.I.pData.QuestList.Add(qList[curId]);
                int n = PlayerManager.I.pData.QuestList.Count - 1;
                PlayerManager.I.pData.QuestList[n].State = 1;
                PlayerManager.I.pData.QuestList[n].sDay = GsManager.I.tDay;
                PlayerManager.I.pData.QuestList[n].eDay = GsManager.I.tDay + qList[curId].Days;
                QuestManager.I.CityQuest[cityId].Remove(qList[curId].Qid);
                UpdateQuestList(n);
                questBtn[n].GetComponent<QuestListBtn>().OnButtonClick();
                break;
            case "OnQstComplete":
                //보상내역 Exp, Crown, GradeExp
                Debug.Log("보상내역: " + qList[curId].Crown + " " + qList[curId].Exp + " " + qList[curId].GradeExp);
                PlayerManager.I.pData.Crown += qList[curId].Crown;
                PlayerManager.I.pData.Exp += qList[curId].Exp;
                PlayerManager.I.pData.GradeExp += qList[curId].GradeExp;
                //퀘스트 삭제 및 UI 갱신
                PlayerManager.I.pData.QuestList.Remove(qList[curId]);
                UpdateQuestList(curId);
                UpdateGradeGgSlider();
                int n2 = curId > qIdx ? qIdx - 1 : curId;
                questBtn[n2].GetComponent<QuestListBtn>().OnButtonClick();
                break;
            case "OnUpgrade":
                if (!isUpgrade) return;
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
                cityId = data.Get<int>();
                qIdx = 0;
                UpdateGradeGgSlider();
                bool not = true;
                if (PlayerManager.I.pData.QuestList.Count > 0)
                {
                    CreateMyQuest();
                    not = false;
                }
                mTexts["MyQuestVal"].text = PlayerManager.I.pData.QuestList.Count.ToString() + " / " + PlayerManager.I.pData.QuestMax.ToString();
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
                {
                    switch (qList[i].State)
                    {
                        case 0:
                            questBtn[i].GetComponent<Image>().color = Color.white;
                            break;
                        case 1:
                            questBtn[i].GetComponent<Image>().color = Color.gray;
                            break;
                        case 2:
                            questBtn[i].GetComponent<Image>().color = Color.green;
                            break;
                    }
                }
                questBtn[curId].GetComponent<Image>().color = Color.yellow;
                qstState = qList[curId].State;
                mButtons["OnQstAccept"].gameObject.SetActive(qstState == 0);
                mButtons["OnQstComplete"].gameObject.SetActive(qstState == 2);
                mGameObject["tObjProgress"].SetActive(qstState == 1);
                break;
        }
    }
    public override void Refresh() { }
    void UpdateGradeGgSlider()
    {
        mTexts["GradeGgVal"].text = PlayerManager.I.pData.GradeExp.ToString() + " / " + PlayerManager.I.pData.GradeNext.ToString();
        float gg = (float)PlayerManager.I.pData.GradeExp / (float)PlayerManager.I.pData.GradeNext * 100;
        if (gg >= 100)
        {
            isUpgrade = true;
            gg = 100;
        }
        mSlider.value = gg;
    }
    void UpdateQuestList(int cnt)
    {
        InitQuestList();
        CreateMyQuest();
        CreateCityQuest();
        mTexts["MyQuestVal"].text = (cnt + 1).ToString() + " / " + PlayerManager.I.pData.QuestMax.ToString();
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