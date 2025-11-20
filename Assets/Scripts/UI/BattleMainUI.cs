using System.Diagnostics;
using GB;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


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
        // mButtons["GoToWorld"].gameObject.SetActive(false); //테스트 후 정상화
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
            GsManager.I.StateMenuPopup(key);
        }
        else
        {
            switch (key)
            {
                case "GoToWorld":
                    BattleCore.I.MoveToWorld();
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
            case "OnGameClear":
                UnityEngine.Debug.Log("GameClear");
                mButtons["GoToWorld"].gameObject.SetActive(true);
                WorldObjManager.I.RemoveWorldMonGrp(); //모든 몬스터를 처치하여 전투에 참여된 모든 몬스터 그룹을 제거
                break;
            case "OnGameOver":
                UnityEngine.Debug.Log("GameOver");
                mButtons["GoToWorld"].gameObject.SetActive(true);
                break;
        }
    }

    public override void Refresh()
    {
    }

    public void SetHp()
    {
        mSlider_HP.value = (float)PlayerManager.I.pData.HP / PlayerManager.I.pData.MaxHP * 100f;
        mTMPText["GgHpVal"].text = PlayerManager.I.pData.HP.ToString() + " / " + PlayerManager.I.pData.MaxHP.ToString();
    }
    public void SetMp()
    {
        mSlider_MP.value = (float)PlayerManager.I.pData.MP / PlayerManager.I.pData.MaxMP * 100;
        mTMPText["GgMpVal"].text = PlayerManager.I.pData.MP.ToString() + " / " + PlayerManager.I.pData.MaxMP.ToString();
    }
    public void SetSp()
    {
        mSlider_SP.value = (float)PlayerManager.I.pData.SP / PlayerManager.I.pData.MaxSP * 100;
        mTMPText["GgSpVal"].text = PlayerManager.I.pData.SP.ToString() + " / " + PlayerManager.I.pData.MaxSP.ToString();
    }

}