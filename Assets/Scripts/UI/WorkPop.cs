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
    private int daysVal, workType, crownVal, rlsVal, skExpVal; //expVal,itemVal
    private bool isStart = false; //시작 여부
    private string strDays;

    private void Awake()
    {
        isStart = false;
        Regist();
        RegistButton();
        mSlider.onValueChanged.AddListener(OnSliderChanged);
        strDays = LocalizationManager.GetValue("DurDays");
    }

    private void OnEnable()
    {
        Presenter.Bind("WorkPop", this);
        daysVal = 1;
    }

    private void OnDisable()
    {
        Presenter.UnBind("WorkPop", this);
        if (!isStart)
        {
            workType = 0;
            foreach (var v in rewardList)
                Destroy(v.gameObject);
            rewardList.Clear();
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
            case "ClickClose":
                Close();
                break;
            case "ClickStart":
                isStart = true;
                Presenter.Send("WorldMainUI", "StartWork", daysVal);
                Close();
                break;
            case "ClickConfirm":
                isStart = false;
                SetWorkReward(); //플레이어가 받을 보상 적용
                Close();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetWork":
                StatePop(0);
                daysVal = 1;
                workType = data.Get<int>();
                mSlider.value = 1;
                mTMPText["DaysVal"].text = string.Format(strDays, daysVal.ToString());
                SetWork();
                break;
            case "EndWork":
                StatePop(1);
                break;
        }
    }
    private void StatePop(int type)
    {
        mGameObject["Pop1"].SetActive(type == 0);
        mGameObject["Pop2"].SetActive(type == 1);
        mGameObject["RwdBox"].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, type == 0 ? -100 : 20);
    }
    private void SetWork()
    {
        if (workType < 200)
        {
            crownVal = GetCrownVal();
            rlsVal = GetRlsVal();
            skExpVal = GetSkExpVal();
            //알바 항목 -> 크라운, 호감도, 스킬 경험치
            CreateWorkReward("Icon_coin", "AddCrown", crownVal, 10001); //크라운
            CreateWorkReward("Icon_heart", "AddRls", rlsVal, 10002); //호감도
            switch (workType)
            {
                case 2:
                    CreateWorkReward("skIcon_7", "AddSkill", skExpVal, 7);
                    break;
                case 3:
                    CreateWorkReward("skIcon_28", "AddSkill", skExpVal, 28);
                    break;
                case 4:
                    CreateWorkReward("skIcon_29", "AddSkill", skExpVal, 29);
                    break;
                case 5:
                    CreateWorkReward("skIcon_30", "AddSkill", skExpVal, 30);
                    break;
                case 6:
                    CreateWorkReward("skIcon_27", "AddSkill", skExpVal, 27);
                    break;
                case 7:
                    CreateWorkReward("skIcon_21", "AddSkill", skExpVal, 21);
                    break;
                case 101:
                    //농장
                    break;
                case 102:
                    //채굴장
                    break;
                case 103:
                    //벌목장
                    break;
                case 108:
                    break;
                case 109:
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
        crownVal = GetCrownVal();
        rlsVal = GetRlsVal();
        skExpVal = GetSkExpVal();
        mTMPText["DaysVal"].text = string.Format(strDays, daysVal.ToString());
        foreach (var v in rewardList)
        {
            if (v.id < 10000)
            {
                //스킬
                skExpVal = GetSkExpVal();
                v.UpdateVal(skExpVal);
            }
            else
            {
                switch (v.id)
                {
                    case 10001:
                        //크라운
                        crownVal = GetCrownVal();
                        v.UpdateVal(crownVal);
                        break;
                    case 10002:
                        //호감도
                        rlsVal = GetRlsVal();
                        v.UpdateVal(rlsVal);
                        break;
                    default:
                        // itemVal = daysVal * 10;
                        break;
                }
            }
        }
    }
    private int GetCrownVal()
    {
        return daysVal * 40;
    }
    private int GetRlsVal()
    {
        return daysVal * 10;
    }
    private int GetSkExpVal()
    {
        return daysVal * 4;
    }
    private void SetWorkReward()
    {
        if (workType < 200)
        {
            // crownVal = GetCrownVal();
            // rlsVal = GetRlsVal();
            // skExpVal = GetSkExpVal();
            PlayerManager.I.pData.Crown += crownVal; //크라운 보상 적용
            Presenter.Send("CityEnterPop", "AddNpcRls", rlsVal);
            switch (workType)
            {
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 101:
                    //농장
                    break;
                case 102:
                    //채굴장
                    break;
                case 103:
                    //벌목장
                    break;
                case 108:
                    break;
                case 109:
                    break;
            }
        }
    }
    public override void Refresh() { }
}