using GB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
public class WorldMainUI : UIScreen
{
    public static bool isBlock = false;
    [SerializeField] private GameObject mBlock;
    [SerializeField] private Slider mIngGg, mEnergyGg;
    private float wTime = 0, actTime, endActTime, actTick = 0;
    private int tDay = 0, wYear, wMonth, wDay;
    private bool isAct = false, isRest = false; //일하기, 휴식 상태 유무
    #region 탐험 관련
    public static bool isExplore = false;
    private List<NodeObj> nodeObj = new List<NodeObj>();
    private List<GameObject> nodeLine = new List<GameObject>();
    [SerializeField] private Transform nodeParent;
    [SerializeField] private GameObject pObj;
    private RectTransform pRt;
    private Vector2Int pPos = Vector2Int.zero;
    public static bool isMoveNode = false;
    public List<ExpEvtBtn> evtBtns = new List<ExpEvtBtn>();
    #endregion
    private void Awake()
    {
        Regist();
        RegistButton();
        mGameObject["IngPop"].SetActive(false);
        mGameObject["ExplorePop"].SetActive(false);
        isExplore = false;
        pRt = pObj.GetComponent<RectTransform>();
        pObj.SetActive(false);
    }
    private void Start()
    {
        if (PlayerManager.I.pData.TraceQId == 0)
            mGameObject["QstBox"].SetActive(false);
        else
            SetTraceQst();
        UpdateState();
    }
    private void OnEnable()
    {
        Presenter.Bind("WorldMainUI", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("WorldMainUI", this);
        if (isExplore)
        {
            mGameObject["ExplorePop"].SetActive(false);
            isExplore = false;
            GsManager.I.CheckWorldCmr();
        }
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
            case "OnInvenPop":
            case "OnCharInfoPop":
            case "OnJournalPop":
            case "OnSkillPop":
                GsManager.I.StateMenuPopup(key);
                break;
            case "X0":
            case "X1":
            case "X2":
            case "X4":
                if (CityEnterPop.isActive) return;
                StateGameSpd(key);
                break;
            case "OnStop":
                InitRest();
                break;
        }
    }

    private void Update()
    {
        wTime += Time.deltaTime;
        if (wTime >= 40.0f)
        {
            wTime = 0;
            AddDay();
        }
        if (isAct)
        {
            UpdateWorkGg();
            if (actTime >= endActTime)
                InitWork();
        }
        if (isRest)
        {
            actTime += Time.deltaTime;
            if (actTime >= 1f)
            {
                actTime = 0;
                actTick++;
                PlayerManager.I.pData.EP += 2;
                if (PlayerManager.I.pData.EP >= PlayerManager.I.pData.MaxEP)
                    PlayerManager.I.pData.EP = PlayerManager.I.pData.MaxEP;
                UpdateEp();
            }
            if (actTick >= endActTime)
            {
                actTick = 0;
                if (PlayerManager.I.pData.Crown >= 50)
                {
                    PlayerManager.I.pData.Crown -= 50;
                }
                else
                {
                    InitRest();
                    Presenter.Send("CityEnterPop", "Inn_NotCrown");
                }
            }
        }

        // I 키 입력 감지
        if (Input.GetKeyDown(KeyCode.I))
            GsManager.I.StateMenuPopup("OnInvenPop");
        if (Input.GetKeyDown(KeyCode.C))
            GsManager.I.StateMenuPopup("OnCharInfoPop");
        if (Input.GetKeyDown(KeyCode.J))
            GsManager.I.StateMenuPopup("OnJournalPop");
        if (Input.GetKeyDown(KeyCode.K))
            GsManager.I.StateMenuPopup("OnSkillPop");
        if (Input.GetKeyDown(KeyCode.Alpha1))
            StateGameSpd("X0");
        if (Input.GetKeyDown(KeyCode.Alpha2))
            StateGameSpd("X1");
        if (Input.GetKeyDown(KeyCode.Alpha3))
            StateGameSpd("X2");
        if (Input.GetKeyDown(KeyCode.Alpha4))
            StateGameSpd("X4");
    }

    // I 키를 눌렀을 때 호출되는 함수 (예제)
    private void AddDay()
    {
        tDay++;
        CalcCalender();
        if (wDay % 2 == 0)
        {
            //몬스터 리스폰
            UnityEngine.Debug.Log("몬스터 스폰 체크");
            WorldCore.I.CheckAllAreaWorldMon();
        }
        if (wDay % 7 == 0)
        {
            //도시 퀘스트 재생성
            UnityEngine.Debug.Log("도시 퀘스트 재생성");
        }
    }
    public void StateGameSpd(string key)
    {
        string numberStr = key.Replace("X", "");
        int val = int.Parse(numberStr);
        Time.timeScale = val;
        GsManager.worldSpd = val;

        // 버튼 이미지 업데이트
        UpdateSpdBtnImg();
    }

    private void UpdateSpdBtnImg()
    {
        // 모든 속도 버튼을 기본 이미지로 초기화
        foreach (var button in mButtons)
        {
            if (button.Key.StartsWith("X"))
            {
                Image buttonImage = button.Value.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = Color.white;
                }
            }
        }

        // 현재 활성화된 속도 버튼만 노란색 이미지로 변경
        string curSpdKey = "X" + GsManager.worldSpd.ToString();
        if (mButtons.ContainsKey(curSpdKey))
        {
            Image curBtnImg = mButtons[curSpdKey].GetComponent<Image>();
            if (curBtnImg != null)
            {
                curBtnImg.color = Color.yellow;
            }
        }
    }

    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetBlock":
                isBlock = data.Get<bool>();
                mBlock.SetActive(isBlock);
                break;
            case "UpdateInfo":
                UpdateState();
                break;
            case "UpdateState": UpdateState(); break;
            case "UpdateAllTime":
                tDay = GsManager.I.tDay;
                wTime = GsManager.I.wTime;
                wYear = GsManager.I.wYear;
                wMonth = GsManager.I.wMonth;
                wDay = GsManager.I.wDay;
                UpdateTime();
                break;
            case "ChangeGameSpd":
                StateGameSpd(data.Get<string>());
                break;
            case "OnWork":
                StartWork(data.Get<int>());
                break;
            case "OnRest":
                StartRest();
                break;
            case "SaveAllTime":
                GsManager.I.SetAllTime(tDay, wYear, wMonth, wDay, wTime);
                break;
            case "SetTraceQst":
                SetTraceQst();
                break;
            case "ShowExplorePop":
                isMoveNode = false;
                GsManager.I.CurExpId = data.Get<int>();
                GsManager.I.SetCurNodeData();
                mGameObject["ExplorePop"].SetActive(true);
                isExplore = true;
                GsManager.I.CheckWorldCmr();
                DrawExpMap();
                break;
            case "ClickNode":
                isMoveNode = true;
                MoveNode(data.Get<Vector2Int>());
                break;
            case "StartBattle":
                break;
            case "TakeRest":
                break;
            case "OpenBox":
                break;
        }
    }
    #region 메인 UI
    private void UpdateState()
    {
        mTMPText["HpVal"].text = PlayerManager.I.pData.HP.ToString() + "/" + PlayerManager.I.pData.MaxHP.ToString();
        mTMPText["MpVal"].text = PlayerManager.I.pData.MP.ToString() + "/" + PlayerManager.I.pData.MaxMP.ToString();
        mTMPText["SpVal"].text = PlayerManager.I.pData.SP.ToString() + "/" + PlayerManager.I.pData.MaxSP.ToString();
        UpdateEp();
    }
    private void UpdateEp()
    {
        mTMPText["EpVal"].text = PlayerManager.I.pData.EP.ToString();
        mEnergyGg.value = (float)PlayerManager.I.pData.EP / PlayerManager.I.pData.MaxEP * 100f;
    }
    private void CalcCalender()
    {
        wYear = tDay / 360;
        wMonth = tDay % 360 / 30;
        wDay = tDay % 30 + 1;
        UpdateTime();
    }
    private void UpdateTime()
    {
        mTexts["YearVal"].text = wYear.ToString();
        mTexts["MonVal"].text = wMonth.ToString();
        mTexts["DayVal"].text = wDay.ToString();
    }

    private void StartWork(int day)
    {
        isAct = true;
        mTMPText["IngMent"].text = LocalizationManager.GetValue("Ing_Work");
        actTime = 0;
        endActTime = day * 40;
        mIngGg.maxValue = endActTime;
        mIngGg.value = 0;
        mIngGg.gameObject.SetActive(true);
        mButtons["OnStop"].gameObject.SetActive(false);
        StateAct(true);
        Presenter.Send("CityEnterPop", "StateVisiblePop", 0);
    }
    private void InitWork()
    {
        isAct = false; actTime = 0; endActTime = 0;
        StateAct(false);
        UIManager.ShowPopup("WorkPop");
        Presenter.Send("WorkPop", "EndWork");
        Presenter.Send("CityEnterPop", "StateVisiblePop", 1);
    }
    private void InitRest()
    {
        isRest = false; actTime = 0; endActTime = 0;
        StateAct(false);
        Presenter.Send("CityEnterPop", "StateVisiblePop", 1);
    }
    private void StateAct(bool on)
    {
        StateGameSpd(on ? "X4" : "X0");
        mGameObject["IngPop"].SetActive(on);
    }
    private void StartRest()
    {
        isRest = true;
        mTMPText["IngMent"].text = LocalizationManager.GetValue("Ing_Rest");
        actTime = 0;
        endActTime = 40;
        mIngGg.gameObject.SetActive(false);
        mButtons["OnStop"].gameObject.SetActive(true);
        StateAct(true);
        Presenter.Send("CityEnterPop", "StateVisiblePop", 0);
    }
    private void UpdateWorkGg()
    {
        actTime += Time.deltaTime;
        mIngGg.value = actTime;
    }
    private void SetTraceQst()
    {
        mGameObject["QstBox"].SetActive(true);
        int qid = PlayerManager.I.pData.TraceQId;
        foreach (var v in PlayerManager.I.pData.MainQst)
        {
            if (qid == v.Qid)
            {
                mTMPText["QstName"].text = LocalizationManager.GetValue(v.Name);
                mTMPText["QstDesc"].text = v.Desc;
                break;
            }
        }
        foreach (var v in PlayerManager.I.pData.GuildQst)
        {
            if (qid == v.Qid)
            {
                mTMPText["QstName"].text = LocalizationManager.GetValue(v.Name);
                mTMPText["QstDesc"].text = v.Desc;
                break;
            }
        }
    }
    #endregion
    #region 탐험
    private void DrawExpMap()
    {
        InitNode();
        mTMPText["EvtDesc"].gameObject.SetActive(false);
        foreach (var v in evtBtns)
            v.gameObject.SetActive(false);
        List<CurNodeData> nodeList = GsManager.I.CurNodeList;
        List<List<Vector2Int>> lineList = new List<List<Vector2Int>>();

        var dir4 = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(1, 0),
            new Vector2Int(0, -1), new Vector2Int(-1, 0)
        };
        var drawn = new HashSet<string>();
        //lineList 수집
        foreach (var n in nodeList)
        {
            foreach (var d in dir4)
            {
                Vector2Int np = n.pos + d;
                CurNodeData target = nodeList.Find(x => x.pos == np);
                if (target == null) continue;
                Vector2Int posA = n.pos;
                Vector2Int posB = target.pos;
                bool canAtoB = target.nType != 3 || target.prev == posA;
                bool canBtoA = n.nType != 3 || n.prev == posB;
                // 양방향 중 하나라도 가능할 때만 연결
                if (!canAtoB && !canBtoA) continue;
                // (A,B) / (B,A) 중복 제거
                string key = GetEdgeKey(posA, posB);
                if (drawn.Contains(key)) continue;
                drawn.Add(key);
                lineList.Add(new List<Vector2Int> { posA, posB });
            }
        }

        foreach (var line in lineList)
        {
            GameObject lObj = Instantiate(ResManager.GetGameObject("NodeLine"), nodeParent);
            SetupNodeLine(lObj.GetComponent<RectTransform>(), line[0], line[1]);
            nodeLine.Add(lObj);
        }

        foreach (var n in nodeList)
        {
            GameObject obj = Instantiate(ResManager.GetGameObject("NodeObj"), nodeParent);
            obj.GetComponent<NodeObj>().SetNode(n.pos.x, n.pos.y, n.nType, n.eType, n.isClear);
            nodeObj.Add(obj.GetComponent<NodeObj>());
        }
        pRt.anchoredPosition = new Vector2(-400 + nodeList[0].pos.x * 160, 240 - nodeList[0].pos.y * 160);
        pObj.SetActive(true);
        pObj.transform.SetAsLastSibling();
        pPos = nodeList[0].pos;
        CheckMoveableNode();
    }
    string GetEdgeKey(Vector2Int a, Vector2Int b)
    {
        if (a.x < b.x || (a.x == b.x && a.y < b.y))
            return $"{a.x}_{a.y}-{b.x}_{b.y}";
        return $"{b.x}_{b.y}-{a.x}_{a.y}";
    }
    Vector2 GridToAnchored(Vector2Int gridPos)
    {
        return new Vector2(-400 + gridPos.x * 160, 240 - gridPos.y * 160);
    }
    void SetupNodeLine(RectTransform lineRt, Vector2Int posA, Vector2Int posB)
    {
        Vector2 anchoredA = GridToAnchored(posA);
        Vector2 anchoredB = GridToAnchored(posB);
        Vector2 delta = anchoredB - anchoredA;
        lineRt.anchoredPosition = (anchoredA + anchoredB) * 0.5f;
        if (Mathf.Abs(delta.x) > 0)
            lineRt.localEulerAngles = Vector3.zero;       // 가로
        else
            lineRt.localEulerAngles = new Vector3(0, 0, 90); // 세로
        lineRt.sizeDelta = new Vector2(120, 30);
        lineRt.SetAsFirstSibling();
    }
    private void InitNode()
    {
        foreach (var n in nodeObj) Destroy(n.gameObject);
        foreach (var l in nodeLine) Destroy(l);
        nodeObj.Clear();
        nodeLine.Clear();
    }
    private void CheckMoveableNode()
    {
        var dir4 = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        foreach (var n in nodeObj)
            n.StateMoveable(false);
        List<CurNodeData> nodeList = GsManager.I.CurNodeList;
        foreach (var d in dir4)
        {
            Vector2Int np = pPos + d;
            foreach (var n in nodeList)
            {
                if (n.pos == np)
                {
                    if (n.nType == 3 && n.prev != pPos) continue;
                    nodeObj.Find(x => x.pos == n.pos).StateMoveable(true);
                    break;
                }
            }
        }
    }
    private void MoveNode(Vector2Int pos)
    {
        foreach (var n in nodeObj)
            n.StateHighlight(false);
        InitEvtPannel();
        //pPos 에서 pos 로 이동
        pRt.DOAnchorPos(new Vector2(-400 + pos.x * 160, 240 - pos.y * 160), 0.5f)
        .SetEase(Ease.Linear)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            pPos = pos;
            isMoveNode = false;
            CheckMoveableNode();
            CheckCurNodeEvt();
        });
    }
    private void InitEvtPannel()
    {
        foreach (var v in evtBtns)
            v.gameObject.SetActive(false);
        mTMPText["EvtDesc"].gameObject.SetActive(false);
    }
    private void CheckCurNodeEvt()
    {
        foreach (var n in nodeObj)
        {
            if (n.pos == pPos)
            {
                Debug.Log("CheckCurNodeEvt: " + n.eType);
                //GsManager.I.CurExpId 증가
                // GsManager.I.CurExpId++;
                evtBtns[0].gameObject.SetActive(true);
                string desc = "";
                switch (n.eType)
                {
                    default:
                        evtBtns[0].gameObject.SetActive(false);
                        desc = "Evt_Exp_Empty";
                        break; //빈 공간인 경우
                    case 1:
                        evtBtns[0].SetEvtBtn("StartBattle");
                        desc = "Evt_Exp_Battle";
                        break; //전투 이벤트인 경우
                    case 2:
                        evtBtns[0].SetEvtBtn("TakeRest");
                        desc = "Evt_Exp_Rest";
                        break; //회복 이벤트인 경우
                    case 11:
                        evtBtns[0].SetEvtBtn("OpenBox");
                        desc = "Evt_Exp_RanBox";
                        break; //랜덤 상자 이벤트인 경우
                    case 12:
                        evtBtns[0].SetEvtBtn("StartBattle");
                        desc = "Evt_Exp_BattleBox";
                        break; // 전투 후 보물상자 획득 이벤트 발생
                        // case 13:
                        //     // 퍼즐 보상 이벤트 발생
                        //     break;
                }
                mTMPText["EvtDesc"].text = LocalizationManager.GetValue(desc);
                mTMPText["EvtDesc"].gameObject.SetActive(true);
                break;
            }
        }
    }
    #endregion
    public override void Refresh()
    {
        // UI가 새로고침될 때 현재 속도에 맞게 버튼 이미지 업데이트
        UpdateSpdBtnImg();
    }
}