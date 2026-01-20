using GB;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SkillPop : UIScreen
{
    [SerializeField] private Dictionary<int, SkObj> skPassiveList = new Dictionary<int, SkObj>();
    [SerializeField] private Dictionary<int, SkObj> skCombatList = new Dictionary<int, SkObj>();
    [SerializeField] private Dictionary<int, SkObj> skMagicList = new Dictionary<int, SkObj>();
    [SerializeField] private Slider mSlider;
    public static bool isActive = false;
    private int curWpIdx = 0, curMgIdx = 0;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        StateTab(0);
        StateCombat(0);
        StateMagic(0);
    }

    private void OnEnable()
    {
        Presenter.Bind("SkillPop", this);
        isActive = true;
        if (InvenPop.isActive)
            UIManager.ClosePopup("InvenPop");
        InitPop();
        if (GsManager.gameState == GameState.World) GsManager.I.InitCursor();
    }

    private void OnDisable()
    {
        Presenter.UnBind("SkillPop", this);
        isActive = false;
        foreach (var v in skCombatList)
        {
            if (v.Value != null)
                Destroy(v.Value.gameObject);
        }
        foreach (var v in skMagicList)
        {
            if (v.Value != null)
                Destroy(v.Value.gameObject);
        }
        skCombatList.Clear();
        skMagicList.Clear();
        curWpIdx = 0;
        curMgIdx = 0;
    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }

    public void OnButtonClick(string key)
    {
        if (key.StartsWith("Wp"))
        {
            int wpIdx = 0;
            switch (key)
            {
                case "WpNormal": wpIdx = 0; break;
                case "WpSword": wpIdx = 1; break;
                case "WpAxe": wpIdx = 2; break;
                case "WpBlunt": wpIdx = 3; break;
                case "WpSpear": wpIdx = 4; break;
                case "WpBow": wpIdx = 5; break;
                case "WpShield": wpIdx = 6; break;
            }
            if (curWpIdx != wpIdx)
            {
                curWpIdx = wpIdx;
                StateCombat(curWpIdx);
            }
        }
        else if (key.StartsWith("Mg"))
        {
            int mgIdx = 0;
            switch (key)
            {
                case "MgNormal": mgIdx = 0; break;
                case "MgFire": mgIdx = 1; break;
                case "MgIce": mgIdx = 2; break;
                case "MgElectric": mgIdx = 3; break;
                case "MgEarth": mgIdx = 4; break;
                case "MgWind": mgIdx = 5; break;
                case "MgHoly": mgIdx = 6; break;
                case "MgDark": mgIdx = 7; break;
            }
            if (curMgIdx != mgIdx)
            {
                curMgIdx = mgIdx;
                StateMagic(curMgIdx);
            }
        }
        else
        {
            switch (key)
            {
                case "Close":
                    Close();
                    break;
                case "PassiveBtn":
                    StateTab(0);
                    break;
                case "CombatBtn":
                    StateTab(1);
                    break;
                case "MagicBtn":
                    StateTab(2);
                    break;
            }
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SelectSk":
                ShowSkDesc(data.Get<SkData>());
                break;
            case "DragSk":
                mGameObject["SkDesc"].SetActive(false);
                mGameObject["SkSlots"].SetActive(true);
                break;
            case "EndDragSk":
                mGameObject["SkSlots"].SetActive(false);
                mGameObject["SkDesc"].SetActive(true);
                break;
        }
    }

    private void InitPop()
    {
        mTMPText["LvVal"].gameObject.SetActive(false);
        mTMPText["NameVal"].gameObject.SetActive(false);
        mTMPText["ExpVal"].gameObject.SetActive(false);
        mTMPText["AttVal"].gameObject.SetActive(false);
        mGameObject["IconObj"].SetActive(false);
        mGameObject["DescMain"].SetActive(false);
        mSlider.value = 0;
        mSlider.gameObject.SetActive(false);
        CheckPassiveSk();
        mGameObject["SkSlots"].SetActive(false);
    }

    private void CheckPassiveSk()
    {
        Dictionary<int, SkData> mySk = PlayerManager.I.pData.SkList;
        foreach (var v in mySk)
        {
            if (v.Value.SkType == 0)
            {
                if (!skPassiveList.ContainsKey(v.Key))
                {
                    CreateSkObj(v.Key, mGameObject["PassiveMain"].transform, 0);
                }
                else
                    UpdateSkObj(v.Key, 0);
            }
            else
                break;
        }
        ArrangeSkObj();
    }
    private void CreateSkObj(int skId, Transform parent, int type)
    {
        SkData data = PlayerManager.I.pData.SkList[skId];
        GameObject obj = Instantiate(ResManager.GetGameObject("SkObj"), parent);
        obj.GetComponent<SkObj>().SetSk(data);
        if (type < 10)
        {
            skPassiveList[skId] = obj.GetComponent<SkObj>();
        }
        else if (type < 40)
        {
            skCombatList[skId] = obj.GetComponent<SkObj>();
        }
        else
        {
            skMagicList[skId] = obj.GetComponent<SkObj>();
        }
    }
    private void UpdateSkObj(int skId, int type)
    {
        SkData data = PlayerManager.I.pData.SkList[skId];
        if (type == 0)
        {
            skPassiveList[skId].SetSk(data);
        }
        else if (type < 20)
        {
            skCombatList[skId].SetSk(data);
        }
        else
        {
            skMagicList[skId].SetSk(data);
        }
    }
    void ArrangeSkObj()
    {
        // skPassiveList: Dictionary<int, GameObject> 가정
        var ordered = skPassiveList.OrderBy(kv => kv.Key).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            var slotObj = ordered[i].Value;
            slotObj.transform.SetSiblingIndex(i);
        }
    }
    private void ShowSkDesc(SkData data)
    {
        if (!mGameObject["DescMain"].gameObject.activeSelf)
        {
            mTMPText["LvVal"].gameObject.SetActive(true);
            mTMPText["NameVal"].gameObject.SetActive(true);
            mTMPText["ExpVal"].gameObject.SetActive(true);
            mGameObject["IconObj"].SetActive(true);
            mSlider.gameObject.SetActive(true);
            mGameObject["DescMain"].SetActive(true);
            mTMPText["AttVal"].gameObject.SetActive(true);
        }
        mTMPText["NameVal"].text = LocalizationManager.GetValue(data.Name);
        mTMPText["DescVal"].text = LocalizationManager.GetValue(data.Name + "_Desc");
        mTMPText["LvVal"].text = string.Format(LocalizationManager.GetValue("LvisA"), data.Lv);
        mTMPText["ExpVal"].text = data.Exp.ToString() + "/" + data.NextExp.ToString();
        mSlider.value = (float)data.Exp / (float)data.NextExp;
        mImages["Icon"].sprite = ResManager.GetSprite("skIcon_" + data.SkId);
        string str = "";
        foreach (var v in data.Att)
        {
            string name = LocalizationManager.GetValue(v.Name);
            int cnt = data.Lv / v.ItvLv;
            int val = v.Val + (v.ItvVal * cnt);
            str += string.Format(LocalizationManager.GetValue(v.Str), name, val) + "\n";
        }
        mTMPText["AttVal"].text = str;
    }
    private void StateTab(int idx)
    {
        string[] btn = new string[] { "PassiveBtn", "CombatBtn", "MagicBtn" };
        string[] obj = new string[] { "PassiveObj", "CombatObj", "MagicObj" };
        for (int i = 0; i < 3; i++)
        {
            if (i == idx)
            {
                mButtons[btn[i]].GetComponent<Image>().color = new Color((200 / 255f), (185 / 255f), (155 / 255f), 1);
                mGameObject[obj[i]].SetActive(true);
            }
            else
            {
                mButtons[btn[i]].GetComponent<Image>().color = Color.gray;
                mGameObject[obj[i]].SetActive(false);
            }
        }
    }
    private void StateCombat(int idx)
    {
        string[] btn = new string[] { "WpNormal", "WpSword", "WpAxe", "WpBlunt", "WpSpear", "WpBow", "WpShield" };
        foreach (var v in btn)
            mButtons[v].GetComponent<Image>().color = Color.gray;
        mButtons[btn[idx]].GetComponent<Image>().color = Color.yellow;
        //밑에 일반 공격 스킬 리스트 보여주기
    }
    private void StateMagic(int idx)
    {
        string[] btn = new string[] { "MgNormal", "MgFire", "MgIce", "MgElectric", "MgEarth", "MgWind", "MgHoly", "MgDark" };
        foreach (var v in btn)
            mButtons[v].GetComponent<Image>().color = Color.gray;
        mButtons[btn[idx]].GetComponent<Image>().color = Color.yellow;
        //밑에 일반 마법 스킬 리스트 보여주기
    }

    public override void Refresh() { }
}