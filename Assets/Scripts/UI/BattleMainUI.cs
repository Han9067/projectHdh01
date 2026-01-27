using GB;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class BattleMainUI : UIScreen
{
    public Slider mSlider_HP, mSlider_MP, mSlider_SP;
    public List<SkSlotObj> skSlots = new List<SkSlotObj>();
    private Sequence tstSqc;
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
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Debug.Log(PlayerManager.I.pSkSlots[PlayerManager.I.curSlotLine][0]);
            BattleCore.I.StateSk(PlayerManager.I.pSkSlots[PlayerManager.I.curSlotLine][0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BattleCore.I.StateSk(PlayerManager.I.pSkSlots[PlayerManager.I.curSlotLine][1]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log(PlayerManager.I.pSkSlots[PlayerManager.I.curSlotLine][2]);
        }
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
            case "ShowToastPopup":
                ShowTstBox(LocalizationManager.GetValue(data.Get<string>()));
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
            mButtons["Line" + i].GetComponent<Image>().color = Color.gray;
        mButtons["Line" + (line + 1)].GetComponent<Image>().color = Color.yellow;
    }
    private void ShowTstBox(string msg)
    {
        GameObject tstBox = mGameObject["TstBox"];
        CanvasGroup canvasGroup = tstBox.GetComponent<CanvasGroup>();

        if (tstSqc != null && tstSqc.IsActive())
            tstSqc.Kill();

        tstBox.SetActive(true);
        mTMPText["TstMent"].text = msg;

        // 알파값 초기화
        canvasGroup.alpha = 0f;

        // Sequence로 모든 애니메이션을 한 번에 관리
        tstSqc = DOTween.Sequence()
            .SetUpdate(true) // 실제 시간 사용
            .Append(canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad)) // 페이드 인
            .AppendInterval(1.4f) // 대기 시간
            .Append(canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad)) // 페이드 아웃
            .OnComplete(() =>
            {
                tstBox.SetActive(false);
                tstSqc = null;
            });
    }
}