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
    private int daysVal, workType, crownVal, rlsVal, skExpVal, expVal, itemVal; //itemVal => 해당 수치가 높을수록 보상이 많아짐
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
        mTMPText["WarningTxt"].gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Presenter.UnBind("WorkPop", this);
        if (!isStart)
        {
            workType = 0;
            itemVal = 0;
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
                if (workType > 100)
                {
                    switch (workType)
                    {
                        case 101:
                            itemVal = GetItemVal(PlayerManager.I.GetSkLv(26)); //추후에는 사냥꾼 활 또는 사냥꾼과 관련된 아이템으로 버프를 받아 더욱 좋은 보상을 받을 수 있도록 수정
                            break;
                    }
                }
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
        skExpVal = GetSkExpVal();
        if (workType < 101)
        {
            crownVal = GetCrownVal();
            rlsVal = GetRlsVal();
            //알바 항목 -> 크라운, 호감도, 스킬 경험치
            CreateWorkReward("Icon_coin", "AddCrown", crownVal, 10002); //크라운
            CreateWorkReward("Icon_heart", "AddRls", rlsVal, 10003); //호감도
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
                case 21:
                    //농장
                    break;
                case 22:
                    //목장
                    break;
                case 23:
                    //제재소
                    break;
                case 24:
                    //채굴장
                    break;
            }
        }
        else
        {
            expVal = GetExpVal();
            switch (workType)
            {
                case 101:
                    CreateWorkReward("icon_exp", "AddExp", expVal, 10001);
                    CreateWorkReward("skIcon_26", "AddSkill", skExpVal, 26);
                    //사냥하기
                    break;
                case 102:
                    //채광하기
                    break;
            }
        }
        mSlider.maxValue = PlayerManager.I.energy / 8f;
        mSlider.value = 1;
        mTMPText["EnergyTxt"].text = GetEnergyTxt(8);
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
                        //경험치
                        expVal = GetExpVal();
                        v.UpdateVal(expVal);
                        break;
                    case 10002:
                        //크라운
                        crownVal = GetCrownVal();
                        v.UpdateVal(crownVal);
                        break;
                    case 10003:
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
        mTMPText["EnergyTxt"].text = GetEnergyTxt(8 * daysVal); ;
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
    private int GetExpVal()
    {
        return daysVal * 100;
    }
    private int GetItemVal(int skLv, int addVal = 0)
    {
        return (daysVal * 10 * skLv) + addVal;
    }
    private string GetEnergyTxt(int num)
    {
        return string.Format(LocalizationManager.GetValue("EnergyConsumedA"), num.ToString());
    }
    private void SetWorkReward()
    {
        if (workType < 101)
        {
            PlayerManager.I.pData.Crown += crownVal; //크라운 보상 적용
            Presenter.Send("CityEnterPop", "AddNpcRls", rlsVal);
            switch (workType)
            {
                case 2:
                    PlayerManager.I.AddSkExp(7, skExpVal);
                    break;
                case 3:
                    PlayerManager.I.AddSkExp(28, skExpVal);
                    break;
                case 4:
                    PlayerManager.I.AddSkExp(29, skExpVal);
                    break;
                case 5:
                    PlayerManager.I.AddSkExp(30, skExpVal);
                    break;
                case 6:
                    PlayerManager.I.AddSkExp(27, skExpVal);
                    break;
                case 7:
                    PlayerManager.I.AddSkExp(21, skExpVal);
                    break;
            }
        }
        else
        {
            switch (workType)
            {
                case 101:
                    //사냥 후 보상
                    PlayerManager.I.AddSkExp(26, skExpVal);
                    ItemManager.I.ShowWorkReward(itemVal, workType, PlayerManager.I.GetSkLv(26));
                    break;
            }
        }
    }
    public override void Refresh() { }
}