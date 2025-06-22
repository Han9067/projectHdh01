using System.Diagnostics;
using GB;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldMainUI : UIScreen
{
    [Header("Speed Button Images")]
    [SerializeField] private Sprite btnSpdWhite;
    [SerializeField] private Sprite btnSpdYellow;
    
    private int currentSpeed = 1; // 현재 활성화된 속도 (기본값 1)
    
    private void Awake()
    {
        Regist();
        RegistButton();
        LoadSpdBtnImg();
    }

    private void OnEnable()
    {
        Presenter.Bind("WorldMainUI", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("WorldMainUI", this);
    }

    private void LoadSpdBtnImg()
    {
        // Resources 폴더에서 이미지 로드
        if (btnSpdWhite == null)
            btnSpdWhite = Resources.Load<Sprite>("Images/World/UI/btn_spd_white");
        if (btnSpdYellow == null)
            btnSpdYellow = Resources.Load<Sprite>("Images/World/UI/btn_spd_yellow");
    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }

    public void OnButtonClick(string key)
    {
        if (key.Contains("state"))
        {
            GameObject go = null;
            string str = "";
            
            switch (key)
            {
                case "stateChar":
                    go = GameObject.Find("CharInfoPop");
                    str = "CharInfoPop";
                    break;
                case "stateInven":
                    go = GameObject.Find("InvenPop");
                    str = "InvenPop";
                    break;
                case "stateQuest":
                    go = GameObject.Find("QuestPop");
                    str = "QuestPop";
                    break;
                case "stateSkill":
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
        else{
            stateGameSpd(key);
        }
    }
    public void stateGameSpd(string key){
        string numberStr = key.Replace("x", "");
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
            if (button.Key.StartsWith("x"))
            {
                Image buttonImage = button.Value.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = btnSpdWhite;
                }
            }
        }
        
        // 현재 활성화된 속도 버튼만 노란색 이미지로 변경
        string curSpdKey = "x" + currentSpeed.ToString();
        if (mButtons.ContainsKey(curSpdKey))
        {
            Image curBtnImg = mButtons[curSpdKey].GetComponent<Image>();
            if (curBtnImg != null)
            {
                curBtnImg.sprite = btnSpdYellow;
            }
        }
    }
    
    public override void ViewQuick(string key, IOData data)
    {
    }

    public override void Refresh()
    {
        // UI가 새로고침될 때 현재 속도에 맞게 버튼 이미지 업데이트
        UpdateSpdBtnImg();
    }
}