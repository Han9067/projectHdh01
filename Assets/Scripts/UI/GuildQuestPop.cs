using System.Collections.Generic;
using GB;
using UnityEngine;
using UnityEngine.UI;

public class GuildQuestPop : UIScreen
{
    [SerializeField] private Transform parent;
    public Slider mSlider;
    private List<QuestListBtn> questBtn = new List<QuestListBtn>(); //퀘스트 버튼 오브젝트
    private List<QuestInstData> qList = new List<QuestInstData>(); //생성된 퀘스트 데이터

    private int curIdx = 0, cityId = 1, qIdx = 0, npcId = 0;
    private int qstState; //현재 퀘스트 상태 0: 수락 가능, 1: 수락 완료, 2: 퀘스트 완료
    private bool isUpgrade = false; //업그레이드 가능 여부
    private Image[] mFilterImg = new Image[10];
    private void Awake()
    {
        Regist();
        RegistButton();
        string[] arr = { "FilterH", "FilterG", "FilterF", "FilterE", "FilterD", "FilterC", "FilterB", "FilterA", "FilterS", "FilterSS" };
        for (int i = 0; i < 10; i++)
            mFilterImg[i] = mButtons[arr[i]].GetComponent<Image>();
    }
    private void OnEnable()
    {
        Presenter.Bind("GuildQuestPop", this);
        Debug.Log("GuildQuestPop OnEnable");
        string[] arr = { "FilterH", "FilterG", "FilterF", "FilterE", "FilterD", "FilterC", "FilterB", "FilterA", "FilterS", "FilterSS" };
        for (int i = 0; i < arr.Length; i++)
            mButtons[arr[i]].gameObject.SetActive(false);
        int cnt = PlayerManager.I.pData.Grade + 1;
        for (int i = 0; i < cnt; i++)
            mButtons[arr[i]].gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        Presenter.UnBind("GuildQuestPop", this);
        InitQuestList();
        qstState = 0;
        cityId = 1;
        npcId = 0;
        curIdx = 0;
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
                int uid = qList[curIdx].QUid;
                qList[curIdx].QNpcId = npcId;
                var qst = PlayerManager.I.pData.GuildQst;
                qst.Add(qList[curIdx]);
                int n = qst.Count - 1;
                qst[n].State = 1;
                var cityQst = QuestManager.I.CityQuest[cityId];
                foreach (var v in cityQst)
                {
                    if (v.QUid == uid)
                    {
                        cityQst.Remove(v);
                        break;
                    }
                }
                UpdateQuestList(qList[curIdx].Grade);
                // switch (qst[n].Qid)
                //     case 31:
                //         WorldCore.I.CreateWorldMarker(qst[n].TgPos, 1); //2번째 매개변수 1~10은 길드 퀘스트용 마커-> 1은 몬스터 토벌용
                //         qst[n].MarkerUid = QuestManager.I.curMkUid;
                //         QuestManager.I.curMkUid = 0;
                //         break;
                // }
                break;
            case "OnQstComplete":
                // Debug.Log("보상내역: " + qList[curId].Crown + " " + qList[curId].Exp + " " + qList[curId].GradeExp);
                // PlayerManager.I.pData.Crown += qList[curId].Crown;
                // PlayerManager.I.pData.Exp += qList[curId].Exp;
                // PlayerManager.I.pData.GradeExp += qList[curId].GradeExp;
                // //퀘스트 삭제 및 UI 갱신
                // //퀘스트 완료 후 삭제 되기전에 퀘스트에 따라 NPC 호감도가 상승해야함
                // switch (qList[curId].Qid)
                // {
                //     case 1:
                //     case 11:
                //     case 21:
                //         Presenter.Send("CityEnterPop", "AddNpcRls", 2); //도시 의뢰자에게 호감도 증가
                //         break;
                //     case 31:
                //         NpcManager.I.AddNpcRls(qList[curId].QNpcId, qList[curId].GradeExp); //출발 도시 의뢰자에게 호감도 증가
                //         Presenter.Send("CityEnterPop", "AddNpcRls", 2);
                //         break;
                // }
                // PlayerManager.I.ClearGuildQst(qList[curId].QUid);
                // UpdateQuestList(curId);
                // UpdateGradeGgSlider();
                // int n2 = curId > qIdx ? qIdx - 1 : curId;
                // questBtn[n2].GetComponent<QuestListBtn>().OnButtonClick();
                break;
            case "OnUpgrade":
                if (!isUpgrade) return;
                Debug.Log("Upgrade");
                break;
            case "FilterH": SelFliterGrade(0); break;
            case "FilterG": SelFliterGrade(1); break;
            case "FilterF": SelFliterGrade(2); break;
            case "FilterE": SelFliterGrade(3); break;
            case "FilterD": SelFliterGrade(4); break;
            case "FilterC": SelFliterGrade(5); break;
            case "FilterB": SelFliterGrade(6); break;
            case "FilterA": SelFliterGrade(7); break;
            case "FilterS": SelFliterGrade(8); break;
            case "FilterSS": SelFliterGrade(9); break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SettingQuestPop":
                SelFliterGrade(PlayerManager.I.pData.Grade - 1); //내부 배열의 인덱스는 0부터 시작하여서 -1 해줘야함
                List<int> rcv = data.Get<List<int>>();
                cityId = rcv[0];
                npcId = rcv[1];
                qIdx = 0;
                UpdateGradeGgSlider();
                break;
            case "ClickQuestListBtn":
                curIdx = data.Get<int>();
                QuestInstData q = qList[curIdx];
                UpdateStars(q.Grade);
                mTMPText["QstName"].text = LocalizationManager.GetValue(q.Name);
                mTMPText["DescVal"].text = q.Desc;
                mTMPText["ExpVal"].text = q.Exp.ToString();
                mTMPText["CrownVal"].text = q.Crown.ToString();
                mTMPText["GdExpVal"].text = q.GradeExp.ToString();
                mTMPText["CurCnt"].gameObject.SetActive(false);
                CheckQstBtn(curIdx);
                if (q.State == 1)
                {
                    switch (q.Qid)
                    {
                        case 2:
                        case 3:
                            mTMPText["CurCnt"].gameObject.SetActive(true);
                            int curCnt = q.CurCnt >= q.TgCnt ? q.TgCnt : q.CurCnt;
                            mTMPText["CurCnt"].text = curCnt.ToString() + " / " + q.TgCnt.ToString();
                            break;
                    }
                }
                qstState = qList[curIdx].State;
                mButtons["OnQstAccept"].gameObject.SetActive(qstState == 0);
                mButtons["OnQstComplete"].gameObject.SetActive(qstState == 2);
                mGameObject["tObjProgress"].SetActive(qstState == 1);
                break;
        }
    }
    public override void Refresh() { }
    private void UpdateGradeGgSlider()
    {
        mTMPText["GradeGgVal"].text = PlayerManager.I.pData.GradeExp.ToString() + " / " + PlayerManager.I.pData.GradeNext.ToString();
        float gg = PlayerManager.I.pData.GradeExp / PlayerManager.I.pData.GradeNext * 100;
        if (gg >= 100)
        {
            isUpgrade = true;
            gg = 100;
        }
        mSlider.value = gg;
    }

    private void SelFliterGrade(int idx)
    {
        string[] arr = { "FilterH", "FilterG", "FilterF", "FilterE", "FilterD", "FilterC", "FilterB", "FilterA", "FilterS", "FilterSS" };
        for (int i = 0; i < arr.Length; i++)
        {
            if (i == idx)
                mFilterImg[i].color = Color.yellow;
            else
                mFilterImg[i].color = Color.white;
        }

        UpdateQuestList(idx + 1); //idx + 1 -> 1~10 등급
    }
    private void InitQuestList()
    {
        foreach (QuestListBtn btn in questBtn)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        questBtn.Clear();
        qList.Clear();
        qIdx = 0;
        isUpgrade = false;
    }
    private void UpdateQuestList(int grade)
    {
        InitQuestList();
        List<QuestInstData> qstList = new List<QuestInstData>();
        var myQst = PlayerManager.I.pData.GuildQst;
        foreach (var v in myQst)
        {
            if (v.Grade == grade)
                qstList.Add(v);
        }
        var cityQst = QuestManager.I.CityQuest[cityId];
        foreach (var v in cityQst)
        {
            if (v.Grade == grade)
                qstList.Add(v);
        }
        foreach (var v in qstList)
        {
            GameObject obj = Instantiate(ResManager.GetGameObject("GuildQstBtn"), parent);
            obj.name = "GuildQuest_" + qIdx;
            QuestListBtn qst = obj.GetComponent<QuestListBtn>();
            qst.SetQuestListBtn(qIdx, v.Grade, v.QType, LocalizationManager.GetValue(v.Name), "GuildQuestPop");
            questBtn.Add(qst);
            qList.Add(v);
            qIdx++;
        }
        mTMPText["MyQstVal"].text = myQst.Count.ToString() + " / " + PlayerManager.I.pData.GuildQstMax.ToString();
        Presenter.Send("GuildQuestPop", "ClickQuestListBtn", curIdx);
    }
    private void CheckQstBtn(int selIdx)
    {
        int cnt = qList.Count;
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