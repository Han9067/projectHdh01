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
            GameObject go = null;
            string str = "";

            switch (key)
            {
                case "StateCharInfoPop":
                    go = GameObject.Find("CharInfoPop");
                    str = "CharInfoPop";
                    break;
                case "StateInvenPop":
                    go = GameObject.Find("InvenPop");
                    str = "InvenPop";
                    break;
                case "StateQuestPop":
                    go = GameObject.Find("QuestPop");
                    str = "QuestPop";
                    break;
                case "StateSkillPop":
                    go = GameObject.Find("SkillPop");
                    str = "SkillPop";
                    break;
            }

            if (go == null)
            {
                UIManager.ShowPopup(str);
            }
            else
            {
                if (go.gameObject.activeSelf)
                    UIManager.ClosePopup(str);
                else
                    UIManager.ShowPopup(str);
            }
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
            case "UpdateGoldTxt":
                mTexts["GoldTxt"].text = data.Get<string>();
                break;
        }
    }

    public override void Refresh()
    {
        // UI가 새로고침될 때 현재 속도에 맞게 버튼 이미지 업데이트
        UpdateSpdBtnImg();
    }
}