using GB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
public class WorldMainUI : UIScreen
{
    [SerializeField] private Slider mIngGg, mHpGg, mMpGg, mSpGg, mEnergyGg;
    private float wTime = 0, workTime, endWorkTime;
    private int tDay = 0, wYear, wMonth, wDay;
    private bool isWork = false; //일하기 상태 유무
    private Sequence tstSqc;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        mGameObject["IngPop"].SetActive(false);
        mGameObject["TstBox"].SetActive(false);
        if (PlayerManager.I.pData.TraceQId == 0)
            mGameObject["QstBox"].SetActive(false);
        else
            SetTraceQst();
        UpdateCrownTxt();
        UpdateState();
    }
    private void OnEnable()
    {
        Presenter.Bind("WorldMainUI", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("WorldMainUI", this);
    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }

    public void OnButtonClick(string key)
    {
        if (key.Contains("State"))
        {
            GsManager.I.StateMenuPopup(key);
        }
        else
        {
            if (CityEnterPop.isActive) return;
            StateGameSpd(key);
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
        if (isWork)
        {
            UpdateWorkGg();
            if (workTime >= endWorkTime)
            {
                isWork = false; workTime = 0; endWorkTime = 0;
                mIngGg.value = 0;
                StateWork(false);
                UIManager.ShowPopup("WorkPop");
                Presenter.Send("WorkPop", "EndWork");
                StateGameSpd("X0");
                Presenter.Send("CityEnterPop", "StateVisiblePop", 1);
            }
        }

        // I 키 입력 감지
        if (Input.GetKeyDown(KeyCode.I))
            GsManager.I.StateMenuPopup("StateInvenPop");
        if (Input.GetKeyDown(KeyCode.C))
            GsManager.I.StateMenuPopup("StateCharInfoPop");
        if (Input.GetKeyDown(KeyCode.J))
            GsManager.I.StateMenuPopup("StateJournalPop");
        if (Input.GetKeyDown(KeyCode.K))
            GsManager.I.StateMenuPopup("StateSkillPop");
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
            case "UpdateInfo":
                UpdateState();
                UpdateCrownTxt();
                break;
            case "UpdateCrownTxt": UpdateCrownTxt(); break;
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
            case "StartWork":
                StartWork(data.Get<int>());
                break;
            case "SaveAllTime":
                GsManager.I.SetAllTime(tDay, wYear, wMonth, wDay, wTime);
                break;
            case "SetTraceQst":
                SetTraceQst();
                break;
            case "ShowToastPopup":
                ShowTstBox(LocalizationManager.GetValue(data.Get<string>()));
                break;
        }
    }
    private void UpdateState()
    {
        mTMPText["HpVal"].text = PlayerManager.I.pData.HP.ToString() + "/" + PlayerManager.I.pData.MaxHP.ToString();
        mTMPText["MpVal"].text = PlayerManager.I.pData.MP.ToString() + "/" + PlayerManager.I.pData.MaxMP.ToString();
        mTMPText["SpVal"].text = PlayerManager.I.pData.SP.ToString() + "/" + PlayerManager.I.pData.MaxSP.ToString();
        mTMPText["FatVal"].text = PlayerManager.I.energy.ToString();
        mHpGg.value = (float)PlayerManager.I.pData.HP / PlayerManager.I.pData.MaxHP * 100f;
        mMpGg.value = (float)PlayerManager.I.pData.MP / PlayerManager.I.pData.MaxMP * 100f;
        mSpGg.value = (float)PlayerManager.I.pData.SP / PlayerManager.I.pData.MaxSP * 100f;
        mEnergyGg.value = PlayerManager.I.energy / 100f;
    }
    private void UpdateCrownTxt()
    {
        mTMPText["CrownVal"].text = PlayerManager.I.pData.Crown.ToString();
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
        isWork = true;
        workTime = 0;
        endWorkTime = day * 40;
        mIngGg.maxValue = endWorkTime;
        mIngGg.value = 0;
        StateWork(true);
        Presenter.Send("CityEnterPop", "StateVisiblePop", 0);
    }
    private void StateWork(bool on)
    {
        StateGameSpd(on ? "X4" : "X0");
        mGameObject["IngPop"].SetActive(on);
    }
    private void UpdateWorkGg()
    {
        workTime += Time.deltaTime;
        mIngGg.value = workTime;
    }
    private void SetTraceQst()
    {
        mGameObject["QstBox"].SetActive(true);
        int qid = PlayerManager.I.pData.TraceQId;
        foreach (var v in PlayerManager.I.pData.QuestList)
        {
            if (qid == v.Qid)
            {
                mTMPText["QstName"].text = LocalizationManager.GetValue(v.Name);
                mTMPText["QstDesc"].text = v.Desc;
                break;
            }
        }
    }
    private void ShowTstBox(string msg)
    {
        GameObject tstBox = mGameObject["TstBox"];
        CanvasGroup canvasGroup = tstBox.GetComponent<CanvasGroup>();

        if (tstSqc != null && tstSqc.IsActive())
            tstSqc.Kill();

        tstBox.SetActive(true);
        mTMPText["TstMent"].text = msg;

        // 알파값 초기화
        canvasGroup.alpha = 0f;

        // Sequence로 모든 애니메이션을 한 번에 관리
        tstSqc = DOTween.Sequence()
            .SetUpdate(true) // 실제 시간 사용
            .Append(canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad)) // 페이드 인
            .AppendInterval(1.4f) // 대기 시간
            .Append(canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad)) // 페이드 아웃
            .OnComplete(() =>
            {
                tstBox.SetActive(false);
                tstSqc = null;
            });
    }
    public override void Refresh()
    {
        // UI가 새로고침될 때 현재 속도에 맞게 버튼 이미지 업데이트
        UpdateSpdBtnImg();
    }
}