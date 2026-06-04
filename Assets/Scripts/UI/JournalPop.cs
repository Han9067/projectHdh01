using GB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class JournalPop : UIScreen
{
    public static bool isActive = false;
    [SerializeField] private Transform questBtnParent;
    private List<QuestListBtn> questBtn = new List<QuestListBtn>();
    private List<QuestInstData> qList = new List<QuestInstData>();
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

        if (GsManager.gameState == GameState.World) GsManager.I.InitCursor();
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
            case "QstMain": SetQst(0); break;
            case "QstSub": SetQst(1); break;
            case "QstGuild": SetQst(2); break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "ClickQuestListBtn":
                // SelectQuestBtn(data.Get<int>());
                break;
        }
    }
    public override void Refresh() { }

    void InitJournalPop()
    {
        InitQst();
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
                SetQst(-1);
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
    private void InitQst()
    {
        mTMPText["QstName"].text = "";
        mTMPText["DescVal"].text = "";
        mTMPText["TgVal"].text = "";
        foreach (QuestListBtn btn in questBtn)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        questBtn.Clear();
        UpdateStars(0);
    }
    void SetQst(int idx)
    {
        InitQst();
        if (PlayerManager.I.pData.MainQst.Count == 0 &&
        PlayerManager.I.pData.SubQst.Count == 0 &&
        PlayerManager.I.pData.GuildQst.Count == 0) return;
        int showIdx = idx == -1 ? 0 : idx;
        if (idx == -1)
        {
            if (PlayerManager.I.pData.MainQst.Count == 0) showIdx = 1;
            if (PlayerManager.I.pData.SubQst.Count == 0) showIdx = 2;
        }
        else
        {
            if (showIdx == 0 && PlayerManager.I.pData.MainQst.Count == 0) return;
            if (showIdx == 1 && PlayerManager.I.pData.SubQst.Count == 0) return;
            if (showIdx == 2 && PlayerManager.I.pData.GuildQst.Count == 0) return;
        }

        string[] tabs = { "QstMain", "QstSub", "QstGuild" };
        foreach (string v in tabs)
        {
            Image img = mButtons[v].GetComponent<Image>();
            if (v == tabs[showIdx])
                img.color = Color.yellow;
            else
            {
                if (img.color != Color.white)
                    img.color = Color.white;
            }
        }
        var qData = new List<QuestInstData>();
        switch (showIdx)
        {
            case 0: qData = PlayerManager.I.pData.MainQst; break;
            case 1: qData = PlayerManager.I.pData.SubQst; break;
            case 2: qData = PlayerManager.I.pData.GuildQst; break;
        }
        for (int i = 0; i < qData.Count; i++)
        {
            GameObject obj = Instantiate(ResManager.GetGameObject("QstBtn"), questBtnParent);
            obj.name = "MyQst_" + i;
            QuestListBtn qst = obj.GetComponent<QuestListBtn>();
            qst.SetQuestListBtn(i, qData[i].Grade, qData[i].QType, LocalizationManager.GetValue(qData[i].Name), "JournalPop");
            questBtn.Add(qst);
            qList.Add(qData[i]);
        }
        SelQstBtn(0);
    }
    private void SelQstBtn(int idx)
    {
        CheckQstBtn(idx);
        var data = qList[idx];
        UpdateStars(data.Grade);
        mTMPText["QstName"].text = LocalizationManager.GetValue(data.Name);
        mTMPText["DescVal"].text = data.Desc;
        if (data.State == 2)
        {
            mTMPText["TgVal"].gameObject.SetActive(true);
            mTMPText["TgVal"].text = LocalizationManager.GetValue("Completed");
            return;
        }
        switch (data.QType)
        {
            case 1: //메인
                break;
            case 2: //서브
                break;
            default: //길드
                switch (data.Qid)
                {
                    case 2:
                    case 3:
                        mTMPText["TgVal"].gameObject.SetActive(true);
                        int curCnt = data.CurCnt >= data.TgCnt ? data.TgCnt : data.CurCnt;
                        mTMPText["TgVal"].text = curCnt.ToString() + " / " + data.TgCnt.ToString();
                        break;
                    default:
                        mTMPText["TgVal"].gameObject.SetActive(false);
                        break;
                }
                break;
        }
        mTMPText["TgVal"].text = data.CurCnt.ToString() + " / " + data.TgCnt.ToString();
    }
    private void CheckQstBtn(int selIdx)
    {
        int cnt = questBtn.Count;
        for (int i = 0; i < cnt; i++)
        {
            questBtn[i].sel.SetActive(i == selIdx);
            questBtn[i].StateTxtColor(qList[i].State);
        }
    }
    void UpdateStars(int star)
    {
        for (int i = 1; i <= 10; i++)
            mGameObject[$"gqPopStar{i}"].SetActive(i <= star);
    }
}