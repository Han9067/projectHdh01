using System.Collections;
using GB;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorkPop : UIScreen
{
    public Transform rewardParent;
    [SerializeField] private Slider mSlider;
    [SerializeField] private List<WorkRewardList> rewardList;
    private int daysVal, workType;

    private void Awake()
    {
        Regist();
        RegistButton();
        mSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnEnable()
    {
        Presenter.Bind("WorkPop", this);
        daysVal = 1;
        foreach (var v in rewardList)
        {
            Destroy(v.gameObject);
        }
        rewardList.Clear();
    }

    private void OnDisable()
    {
        Presenter.UnBind("WorkPop", this);

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
            case "ClickClose":
                Close();
                break;
            case "ClickStart":
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetWork":
                daysVal = 1;
                workType = data.Get<int>();
                mSlider.value = 1;
                mTMPText["DaysVal"].text = string.Format(LocalizationManager.GetValue("DurDays"), daysVal.ToString());
                SetWork();
                break;
        }
    }
    private void SetWork()
    {
        if (workType < 100)
        {
            //알바 항목 -> 크라운, 호감도, 스킬 경험치
            CreateWorkReward("Icon_coin", "AddCrown", daysVal * 100, 0); //크라운
            CreateWorkReward("Icon_heart", "AddRls", daysVal * 10, 0); //호감도
            int skExp = daysVal * 2;
            switch (workType)
            {
                case 2:
                    CreateWorkReward("skIcon_7", "AddSkill", skExp, 7);
                    break;
                case 3:
                    //대장간에서 일하기
                    break;
                case 4:
                    //재단소에서 일하기
                    break;
                case 5:
                    //약제상에서 일하기
                    break;
                case 6:
                    //시장에서 일하기
                    break;
                case 7:
                    //서점에서 일하기
                    break;
            }
        }
        else if (workType < 200)
        {
            switch (workType)
            {
                case 101:
                    //농사하기
                    break;
                case 102:
                    //채굴하기
                    break;
                case 108:
                    //성당에서 신앙수업
                    break;
                case 109:
                    //훈련장에서 훈련
                    break;
            }
        }
        else
        {
            switch (workType)
            {
                case 201:
                    //사냥하기
                    break;
                case 202:
                    //낚시하기
                    break;
            }
        }
    }
    private void CreateWorkReward(string img, string txt, int val, int type)
    {
        GameObject obj = Instantiate(ResManager.GetGameObject("WorkRewardList"), rewardParent);
        obj.GetComponent<WorkRewardList>().SetWorkReward(img, txt, val, type);
        rewardList.Add(obj.GetComponent<WorkRewardList>());
    }
    private void OnSliderChanged(float value)
    {
        daysVal = Mathf.RoundToInt(value);
    }
    public override void Refresh() { }
}