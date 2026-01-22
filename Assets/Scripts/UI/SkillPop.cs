using GB;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SkillPop : UIScreen
{
    public static int slotLine = -1, slotIdx = -1;

    [SerializeField] private Dictionary<int, SkObj> skPsvList = new Dictionary<int, SkObj>(); //passive skill list
    [SerializeField] private Dictionary<int, SkObj> skWpList = new Dictionary<int, SkObj>(); //weapon skill list
    [SerializeField] private Dictionary<int, SkObj> skMgList = new Dictionary<int, SkObj>(); //magic skill list
    [SerializeField] private Slider mSlider;
    [SerializeField] private SelSkObj selSkObj;
    [SerializeField] private List<List<SkSlotObj>> skSlots = new List<List<SkSlotObj>>();
    public static bool isActive = false;
    private int curWpIdx = 0, curMgIdx = 0;
    private void Awake()
    {
        Regist();
        RegistButton();
        InitSkSlot();
    }
    private void Start()
    {
        StateTab(0);
        StateWp(0);
        StateMg(0);
    }

    private void OnEnable()
    {
        Presenter.Bind("SkillPop", this);
        isActive = true;
        if (InvenPop.isActive)
            UIManager.ClosePopup("InvenPop");
        InitPop();
        if (GsManager.gameState == GameState.World) GsManager.I.InitCursor();
        mGameObject["SkSlots"].SetActive(false);
        mGameObject["SkBlock"].SetActive(false);
        selSkObj.gameObject.SetActive(false);
        UpdateSkSlot();
        slotLine = -1; slotIdx = -1; //선택된 스킬 슬롯 초기화(포인터 오버 시 선택된 스킬 슬롯 정보 저장)
    }

    private void OnDisable()
    {
        Presenter.UnBind("SkillPop", this);
        isActive = false;
        foreach (var v in skWpList)
        {
            if (v.Value != null)
                Destroy(v.Value.gameObject);
        }
        foreach (var v in skMgList)
        {
            if (v.Value != null)
                Destroy(v.Value.gameObject);
        }
        skWpList.Clear();
        skMgList.Clear();
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
        switch (key)
        {
            case "Close":
                Close();
                break;
            case "PsvBtn":
                StateTab(0);
                break;
            case "WpBtn":
                StateTab(1);
                break;
            case "MgBtn":
                StateTab(2);
                break;
            default:
                if (key.StartsWith("Wp"))
                {
                    int wpIdx = 0;
                    switch (key)
                    {
                        case "WpAll": wpIdx = 0; break;
                        case "WpNormal": wpIdx = 1; break;
                        case "WpSword": wpIdx = 2; break;
                        case "WpAxe": wpIdx = 3; break;
                        case "WpBlunt": wpIdx = 4; break;
                        case "WpSpear": wpIdx = 5; break;
                        case "WpBow": wpIdx = 6; break;
                        case "WpShield": wpIdx = 7; break;
                    }
                    if (curWpIdx != wpIdx)
                    {
                        curWpIdx = wpIdx;
                        StateWp(curWpIdx);
                    }
                }
                else if (key.StartsWith("Mg"))
                {
                    int mgIdx = 0;
                    switch (key)
                    {
                        case "MgAll": mgIdx = 0; break;
                        case "MgNormal": mgIdx = 1; break;
                        case "MgFire": mgIdx = 2; break;
                        case "MgIce": mgIdx = 3; break;
                        case "MgElectric": mgIdx = 4; break;
                        case "MgEarth": mgIdx = 5; break;
                        case "MgWind": mgIdx = 6; break;
                        case "MgHoly": mgIdx = 7; break;
                        case "MgDark": mgIdx = 8; break;
                    }
                    if (curMgIdx != mgIdx)
                    {
                        curMgIdx = mgIdx;
                        StateMg(curMgIdx);
                    }
                }
                else
                {
                    switch (key)
                    {
                        case "Close":
                            Close();
                            break;
                        case "PsvBtn":
                            StateTab(0);
                            break;
                        case "WpBtn":
                            StateTab(1);
                            break;
                        case "MgBtn":
                            StateTab(2);
                            break;
                    }
                }
                break;
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
                mGameObject["SkSlots"].SetActive(true);
                mGameObject["SkBlock"].SetActive(true);
                selSkObj.gameObject.SetActive(true);
                selSkObj.SetSelSkObj(data.Get<SkData>());
                break;
            case "EndDragSk":
                mGameObject["SkSlots"].SetActive(false);
                mGameObject["SkBlock"].SetActive(false);
                selSkObj.gameObject.SetActive(false);
                //
                if (slotLine != -1 && slotIdx != -1)
                    RegiSkSlot(slotLine, slotIdx, data.Get<int>());
                break;
            case "MoveSelSkObj":
                selSkObj.transform.position = Input.mousePosition;
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
        CheckPsvSk();
        CheckWpSk();
        CheckMgSk();
    }

    private void CheckPsvSk()
    {
        var mySk = PlayerManager.I.pData.SkList;
        foreach (var v in mySk)
        {
            if (v.Value.SkType == 0)
            {
                if (!skPsvList.ContainsKey(v.Key))
                    CreateSkObj(v.Key, mGameObject["PsvMain"].transform, 0);
                else
                    UpdateSkObj(v.Key, 0);
            }
        }
        ArrangeSkObj(skPsvList);
    }
    private void CheckWpSk()
    {
        var mySk = PlayerManager.I.pData.SkList;
        foreach (var v in mySk)
        {
            if (v.Value.SkType > 0 && v.Value.SkType < 21)
            {
                if (!skWpList.ContainsKey(v.Key))
                    CreateSkObj(v.Key, mGameObject["WpMain"].transform, v.Value.SkType);
                else
                    UpdateSkObj(v.Key, v.Value.SkType);
            }
        }
    }
    private void CheckMgSk()
    {
        var mySk = PlayerManager.I.pData.SkList;
        foreach (var v in mySk)
        {
            if (v.Value.SkType >= 21)
            {
                if (!skMgList.ContainsKey(v.Key))
                    CreateSkObj(v.Key, mGameObject["MgMain"].transform, v.Value.SkType);
                else
                    UpdateSkObj(v.Key, v.Value.SkType);
            }
        }
    }
    private void CreateSkObj(int skId, Transform parent, int type)
    {
        //추후에는 스킬 사용이 안되는 스킬에 대한 대응이 필요함.
        SkData data = PlayerManager.I.pData.SkList[skId];
        GameObject obj = Instantiate(ResManager.GetGameObject("SkObj"), parent);
        obj.GetComponent<SkObj>().SetSk(data);
        if (type < 10)
            skPsvList[skId] = obj.GetComponent<SkObj>();
        else if (type < 21)
            skWpList[skId] = obj.GetComponent<SkObj>();
        else
            skMgList[skId] = obj.GetComponent<SkObj>();
    }
    private void UpdateSkObj(int skId, int type)
    {
        SkData data = PlayerManager.I.pData.SkList[skId];
        if (type < 10)
            skPsvList[skId].SetSk(data);
        else if (type < 21)
            skWpList[skId].SetSk(data);
        else
            skMgList[skId].SetSk(data);
    }
    void ArrangeSkObj(Dictionary<int, SkObj> list)
    {
        var ordered = list.OrderBy(kv => kv.Key).ToList();
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
        string[] btn = new string[] { "PsvBtn", "WpBtn", "MgBtn" };
        string[] obj = new string[] { "PsvObj", "WpObj", "MgObj" };
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
    private void StateWp(int idx)
    {
        string[] btn = new string[] { "WpAll", "WpNormal", "WpSword", "WpAxe", "WpBlunt", "WpSpear", "WpBow", "WpShield" };
        foreach (var v in btn)
            mButtons[v].GetComponent<Image>().color = Color.gray;
        mButtons[btn[idx]].GetComponent<Image>().color = Color.yellow;
        switch (idx)
        {
            case 0:
                //전체
                break;
        }
    }
    private void StateMg(int idx)
    {
        string[] btn = new string[] { "MgAll", "MgNormal", "MgFire", "MgIce", "MgElectric", "MgEarth", "MgWind", "MgHoly", "MgDark" };
        foreach (var v in btn)
            mButtons[v].GetComponent<Image>().color = Color.gray;
        mButtons[btn[idx]].GetComponent<Image>().color = Color.yellow;
        switch (idx)
        {
            case 0:
                //전체
                break;
        }
    }

    private void InitSkSlot()
    {
        skSlots.Clear();
        for (int i = 1; i <= 4; i++)
        {
            string slotGroupName = "SkSlots" + i;
            if (mGameObject.ContainsKey(slotGroupName))
            {
                SkSlotObj[] slots = mGameObject[slotGroupName].GetComponentsInChildren<SkSlotObj>();
                skSlots.Add(new List<SkSlotObj>(slots));
            }
        }
    }

    private void UpdateSkSlot()
    {
        var mySkSlots = PlayerManager.I.pSkSlots;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < mySkSlots[i].Count; j++)
            {
                if (mySkSlots[i][j] == 0) continue;
                SkData data = PlayerManager.I.pData.SkList[mySkSlots[i][j]];
                skSlots[i][j].SetSkSlot(data.SkId, data.SkType, data.UseType);
            }
        }
    }

    private void RegiSkSlot(int line, int idx, int skId)
    {
        SearchRegiSkSlot(skId); //이미 등록된 동일 스킬 슬롯이 있으면 제거
        SkData data = PlayerManager.I.pData.SkList[skId];
        skSlots[line][idx].SetSkSlot(data.SkId, data.SkType, data.UseType);
        PlayerManager.I.pSkSlots[line][idx] = skId;
        if (GsManager.gameState == GameState.Battle)
            Presenter.Send("BattleMainUI", "UpdateSkSlot");
    }
    private void SearchRegiSkSlot(int skId)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < PlayerManager.I.pSkSlots[i].Count; j++)
            {
                if (PlayerManager.I.pSkSlots[i][j] == skId)
                {
                    RemoveSkSlot(i, j);
                    break;
                }
            }
        }
    }
    public void RemoveSkSlot(int line, int idx)
    {
        PlayerManager.I.pSkSlots[line][idx] = 0;
        skSlots[line][idx].SetSkSlot(0, 0, 0);
    }
    public override void Refresh() { }
}