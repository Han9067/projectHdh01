using System.Diagnostics;
using GB;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class BattleMainUI : UIScreen
{
    public Slider mSlider_HP, mSlider_MP, mSlider_SP;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        SetHp();
        SetMp();
        SetSp();
    }
    private void OnEnable()
    {
        Presenter.Bind("BattleMainUI", this);
    }
    private void OnDisable()
    {
        Presenter.UnBind("BattleMainUI", this);

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
            switch (key)
            {
                case "GoToWorld":
                    UnityEngine.Debug.Log("GoToWorld");
                    break;
            }
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "GetPlayerHp":
                SetHp();
                break;
            case "GetPlayerMp":
                SetMp();
                break;
            case "GetPlayerSp":
                SetSp();
                break;
        }
    }

    public override void Refresh()
    {
    }

    public void SetHp()
    {
        mSlider_HP.value = (float)PlayerManager.I.pData.HP / PlayerManager.I.pData.MaxHP * 100f;
        mTexts["GgHpTxt"].text = PlayerManager.I.pData.HP.ToString() + " / " + PlayerManager.I.pData.MaxHP.ToString();
    }
    public void SetMp()
    {
        mSlider_MP.value = (float)PlayerManager.I.pData.MP / PlayerManager.I.pData.MaxMP * 100;
        mTexts["GgMpTxt"].text = PlayerManager.I.pData.MP.ToString() + " / " + PlayerManager.I.pData.MaxMP.ToString();
    }
    public void SetSp()
    {
        mSlider_SP.value = (float)PlayerManager.I.pData.SP / PlayerManager.I.pData.MaxSP * 100;
        mTexts["GgSpTxt"].text = PlayerManager.I.pData.SP.ToString() + " / " + PlayerManager.I.pData.MaxSP.ToString();
    }

}