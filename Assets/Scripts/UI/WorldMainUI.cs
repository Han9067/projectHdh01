using System.Diagnostics;
using GB;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldMainUI : UIScreen
{
    private int currentSpeed = 1; // 현재 활성화된 속도 (기본값 1)
    private void Awake()
    {
        Regist();
        RegistButton();
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
            stateGameSpd(key);
        }
    }

    public void stateGameSpd(string key)
    {
        if (CityEnterPop.isActive) return;
        string numberStr = key.Replace("X", "");
        int val = int.Parse(numberStr);
        Time.timeScale = val;
        currentSpeed = val;

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
        string curSpdKey = "X" + currentSpeed.ToString();
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
                UpdateCrownTxt();
                break;
            case "UpdateCrownTxt": mTexts["MainCrownTxt"].text = data.Get<string>(); break;
            case "UpdateTime":
                UpdateTime();
                break;
        }
    }
    private void UpdateCrownTxt()
    {
        mTexts["MainCrownTxt"].text = PlayerManager.I.pData.Crown.ToString();
    }
    private void UpdateTime()
    {
        mTexts["YearVal"].text = GsManager.I.wYear.ToString();
        mTexts["MonVal"].text = GsManager.I.wMonth.ToString();
        mTexts["DayVal"].text = GsManager.I.wDay.ToString();
    }
    public override void Refresh()
    {
        // UI가 새로고침될 때 현재 속도에 맞게 버튼 이미지 업데이트
        UpdateSpdBtnImg();
    }
}