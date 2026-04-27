using GB;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;

public class BattleMainUI : UIScreen
{
    public Slider mSlider_HP, mSlider_MP, mSlider_SP;
    public List<SkSlotObj> skSlots = new List<SkSlotObj>();
    public List<GameObject> slotBlk = new List<GameObject>();
    public MsgBox msgBox;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void Start()
    {
        UpdateHp();
        UpdateMp();
        UpdateSp();
        mButtons["GoToWorld"].gameObject.SetActive(false); //테스트 후 정상화
        UpdateSlotList(); //슬롯 목록 업데이트
        UpdateMainUiSkSlot(); //스킬 슬롯 업데이트
        UpdateWpSk(); //웨폰 스킬 관련 제한 업데이트
        for (int i = 1; i <= 10; i++)
            mTMPText["skCt" + i].gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BattleSkManager.I.ClickSk(skSlots[0].skId, 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BattleSkManager.I.ClickSk(skSlots[1].skId, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            BattleSkManager.I.ClickSk(skSlots[2].skId, 2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            BattleSkManager.I.ClickSk(skSlots[3].skId, 3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
            BattleSkManager.I.ClickSk(skSlots[4].skId, 4);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            BattleSkManager.I.ClickSk(skSlots[5].skId, 5);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            BattleSkManager.I.ClickSk(skSlots[6].skId, 6);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            BattleSkManager.I.ClickSk(skSlots[7].skId, 7);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            BattleSkManager.I.ClickSk(skSlots[8].skId, 8);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            BattleSkManager.I.ClickSk(skSlots[9].skId, 9);

        if (Input.GetKeyDown(KeyCode.I))
            GsManager.I.StateMenuPopup("OnInvenPop");
        if (Input.GetKeyDown(KeyCode.C))
            GsManager.I.StateMenuPopup("OnCharInfoPop");
        if (Input.GetKeyDown(KeyCode.J))
            GsManager.I.StateMenuPopup("OnJournalPop");
        if (Input.GetKeyDown(KeyCode.K))
            GsManager.I.StateMenuPopup("OnSkillPop");
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
        switch (key)
        {
            case "OnInvenPop":
            case "OnCharInfoPop":
            case "OnJournalPop":
            case "OnSkillPop":
                GsManager.I.StateMenuPopup(key);
                break;
            case "GoToWorld":
                BattleCore.I.MoveToWorld();
                break;
            case "Line1":
            case "Line2":
            case "Line3":
            case "Line4":
                PlayerManager.I.curSlotLine = int.Parse(key.Replace("Line", "")) - 1;
                UpdateSlotList();
                UpdateMainUiSkSlot();
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "UpdateInfo":
                UpdateHp();
                UpdateMp();
                UpdateSp();
                break;
            case "GetPlayerHp": UpdateHp(); break;
            case "GetPlayerMp": UpdateMp(); break;
            case "GetPlayerSp": UpdateSp(); break;
            case "OnGameClear":
                UnityEngine.Debug.Log("GameClear");
                mButtons["GoToWorld"].gameObject.SetActive(true);
                WorldObjManager.I.RemoveWorldMonGrp(); //모든 몬스터를 처치하여 전투에 참여된 모든 몬스터 그룹을 제거
                if (PlayerManager.I.pData.QuestList.FindIndex(x => x.Qid == 101) != -1)
                    PlayerManager.I.NextQuestOrder(101);
                break;
            case "OnGameOver":
                UnityEngine.Debug.Log("GameOver");
                mButtons["GoToWorld"].gameObject.SetActive(true);
                break;
            case "UpdateSkSlot":
                UpdateMainUiSkSlot();
                break;
            case "UpdateSkCt":
                var arr = data.Get<int[]>();
                UpdateSkCt(arr[0], arr[1]); //idx, ct
                break;
            case "UpdateWpSk":
                UpdateWpSk();
                break;
            case "ReduceSkCt":
                ReduceSkCt();
                break;
            case "ShowMsg":
                msgBox.ShowMsg(data.Get<string>());
                break;
        }
    }

    public override void Refresh() { }

    public void UpdateHp()
    {
        mSlider_HP.value = (float)PlayerManager.I.pData.HP / PlayerManager.I.pData.MaxHP * 100f;
        mTMPText["GgHpVal"].text = PlayerManager.I.pData.HP.ToString() + " / " + PlayerManager.I.pData.MaxHP.ToString();
    }
    public void UpdateMp()
    {
        mSlider_MP.value = (float)PlayerManager.I.pData.MP / PlayerManager.I.pData.MaxMP * 100;
        mTMPText["GgMpVal"].text = PlayerManager.I.pData.MP.ToString() + " / " + PlayerManager.I.pData.MaxMP.ToString();
    }
    public void UpdateSp()
    {
        mSlider_SP.value = (float)PlayerManager.I.pData.SP / PlayerManager.I.pData.MaxSP * 100;
        mTMPText["GgSpVal"].text = PlayerManager.I.pData.SP.ToString() + " / " + PlayerManager.I.pData.MaxSP.ToString();
    }
    public void UpdateMainUiSkSlot()
    {
        int line = PlayerManager.I.curSlotLine;
        for (int i = 0; i < skSlots.Count; i++)
        {
            if (PlayerManager.I.pSkSlots[line][i] == 0)
                skSlots[i].SetSkSlot(0, 0, 0);
            else
            {
                SkData data = PlayerManager.I.pData.SkList[PlayerManager.I.pSkSlots[line][i]];
                skSlots[i].SetSkSlot(data.SkId, data.SkType, data.UseType);
            }
            skSlots[i].SetSkIdx(line, i);
        }
    }
    public void UpdateSlotList()
    {
        int line = PlayerManager.I.curSlotLine;
        for (int i = 1; i <= 4; i++)
            mButtons["Line" + i].GetComponent<Image>().color = Color.white;
        mButtons["Line" + (line + 1)].GetComponent<Image>().color = Color.yellow;
    }
    public void UpdateWpSk()
    {
        for (int i = 0; i < 10; i++)
        {
            if (skSlots[i].skId == 0) continue;
            SkData data = PlayerManager.I.pData.SkList[skSlots[i].skId];
            if (data.SkType > 1 && data.SkType < 20)
                slotBlk[i].SetActive(!GsManager.I.GetAvailableWpSk(data.SkType));
        }
    }
    public void ReduceSkCt()
    {
        var skList = PlayerManager.I.pData.SkList;
        var slots = PlayerManager.I.pSkSlots;
        var l = PlayerManager.I.curSlotLine;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (slots[i][j] == 0) continue;
                SkData data = skList[slots[i][j]];
                if (data.CurCt <= 0)
                {
                    data.CurCt = 0;
                    continue;
                }
                data.CurCt--;
                if (i == l)
                    UpdateSkCt(j, data.CurCt);
            }
        }
    }
    public void UpdateSkCt(int idx, int ct)
    {
        var on = ct > 0;
        int n = idx + 1;
        if (on)
        {
            if (!mTMPText["skCt" + n].gameObject.activeSelf)
            {
                mTMPText["skCt" + n].gameObject.SetActive(true);
                // skSlots[idx].StateBlock(true);
                slotBlk[idx].SetActive(true);
            }
            mTMPText["skCt" + n].text = ct.ToString();
        }
        else
        {
            if (mTMPText["skCt" + n].gameObject.activeSelf)
            {
                mTMPText["skCt" + n].gameObject.SetActive(false);
                // skSlots[idx].StateBlock(false);
                slotBlk[idx].SetActive(false);
            }
        }
    }
}