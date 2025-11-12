using System.Diagnostics;
using GB;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldMainUI : UIScreen
{
    [SerializeField] private Slider mIngGg, mHpGg, mMpGg, mSpGg, mFatGg;
    private float wTime = 0, workTime, endWorkTime;
    private int tDay = 0, wYear, wMonth, wDay;
    private bool isWork = false; //일하기 상태 유무

    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        mGameObject["IngPop"].SetActive(false);
    }
    private void OnEnable()
    {
        Presenter.Bind("WorldMainUI", this);
        UpdateCrownTxt();
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
    }
    private void AddDay()
    {
        tDay++;
        CalcCalender();
    }
    public void StateGameSpd(string key)
    {
        string numberStr = key.Replace("X", "");
        int val = int.Parse(numberStr);
        Time.timeScale = val;
        GsManager.I.worldSpd = val;

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
        string curSpdKey = "X" + GsManager.I.worldSpd.ToString();
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
        }
    }
    private void UpdateState()
    {
        mTMPText["HpVal"].text = PlayerManager.I.pData.HP.ToString() + "/" + PlayerManager.I.pData.MaxHP.ToString();
        mTMPText["MpVal"].text = PlayerManager.I.pData.MP.ToString() + "/" + PlayerManager.I.pData.MaxMP.ToString();
        mTMPText["SpVal"].text = PlayerManager.I.pData.SP.ToString() + "/" + PlayerManager.I.pData.MaxSP.ToString();
        // mTMPText["FatVal"].text = PlayerManager.I.pData.Fat.ToString() + "/" + PlayerManager.I.pData.MaxFat.ToString();
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
    public override void Refresh()
    {
        // UI가 새로고침될 때 현재 속도에 맞게 버튼 이미지 업데이트
        UpdateSpdBtnImg();
    }
}