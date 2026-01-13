using GB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class JournalPop : UIScreen
{
    public static bool isActive = false;
    [SerializeField] private Transform qListParent;
    private List<GameObject> qBtnList = new List<GameObject>();
    private string curTab = "OnTabQuest";

    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("JournalPop", this);
        isActive = true;
        if (InvenPop.isActive) UIManager.ClosePopup("InvenPop");
        if (SkillPop.isActive) UIManager.ClosePopup("SkillPop");
        if (CharInfoPop.isActive) UIManager.ClosePopup("CharInfoPop");

        StatePannel(curTab);
        StateTabsColor(curTab);
    }

    private void OnDisable()
    {
        Presenter.UnBind("JournalPop", this);
        isActive = false;
        InitJournalPop();
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
            case "OnJnClose":
                Close();
                break;
            case "OnTabQuest":
            case "OnTabFact":
            case "OnTabRls":
            case "OnTabStats":
            case "OnTabAch":
                curTab = key;
                StateTabsColor(key);
                StatePannel(key);
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "ClickQuestListBtn":
                SelectQuestBtn(data.Get<int>());
                break;
        }
    }
    public override void Refresh() { }

    void InitJournalPop()
    {
        if (qBtnList.Count == 0) return;
        for (int i = 0; i < qBtnList.Count; i++)
            Destroy(qBtnList[i]);
        qBtnList.Clear();
    }
    void StatePannel(string key)
    {
        string[] pannels = { "QuestUiPannel" };
        foreach (string v in pannels)
            mGameObject[v].SetActive(false);
        switch (key)
        {
            case "OnTabQuest":
                mGameObject["QuestUiPannel"].SetActive(true);
                SetQuestPannel();
                break;
        }
    }
    void StateTabsColor(string key)
    {
        string[] tabs = { "OnTabQuest", "OnTabFact", "OnTabRls", "OnTabStats", "OnTabAch" };
        foreach (string v in tabs)
        {
            Image img = mButtons[v].GetComponent<Image>();
            if (img.color != Color.white)
                img.color = Color.white;
        }
        mButtons[key].GetComponent<Image>().color = Color.yellow;
    }
    void SetQuestPannel()
    {
        if (PlayerManager.I.pData.QuestList.Count == 0) return;
        var qData = PlayerManager.I.pData.QuestList;
        for (int i = 0; i < qData.Count; i++)
        {
            GameObject obj = Instantiate(ResManager.GetGameObject("MyQstBtn"), qListParent);
            obj.name = "MyQuest_" + i;
            obj.GetComponent<QuestListBtn>().SetQuestListBtn(i, qData[i].Star, qData[i].QType, LocalizationManager.GetValue(qData[i].Name), "JournalPop");
            switch (qData[i].QType)
            {
                case 1:
                    obj.transform.SetSiblingIndex(mGameObject["SubQst"].transform.GetSiblingIndex());
                    break;
                case 2:
                    obj.transform.SetSiblingIndex(mGameObject["GuildQst"].transform.GetSiblingIndex());
                    break;
                case 3:
                    obj.transform.SetAsLastSibling();
                    break;
            }
            qBtnList.Add(obj);
        }
        SelectQuestBtn(0);
    }
    void SelectQuestBtn(int idx)
    {
        for (int i = 0; i < qBtnList.Count; i++)
            qBtnList[i].GetComponent<Image>().color = Color.white;
        qBtnList[idx].GetComponent<Image>().color = Color.yellow;
        QuestInstData data = PlayerManager.I.pData.QuestList[idx];
        UpdateStars(data.Star);
        mTexts["DescVal"].text = data.Desc;
        if (data.State == 2)
        {
            mTexts["TgVal"].gameObject.SetActive(true);
            mTexts["TgVal"].text = LocalizationManager.GetValue("Completed");
        }
        else
        {
            mTexts["TgVal"].gameObject.SetActive(false);
            switch (data.Qid)
            {
                case 2:
                    mTexts["TgVal"].gameObject.SetActive(true);
                    mTexts["TgVal"].text = data.CurCnt.ToString() + " / " + data.TgCnt.ToString();
                    break;
            }
        }
    }
    void UpdateStars(int star)
    {
        for (int i = 1; i <= 10; i++)
            mGameObject[$"gqPopStar{i}"].SetActive(i <= star);
    }
}