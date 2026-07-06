using GB;
using UnityEngine.UI;


public class NpcInfoPop : UIScreen
{
    private NpcData npcData;
    private int npcId;
    private void Awake()
    {
        Regist();
        RegistButton();
    }
    private void OnEnable()
    {
        Presenter.Bind("NpcInfoPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("NpcInfoPop", this);
        npcId = 0;
        npcData = null;
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
            case "OnTalk":
                break;
            case "OnGift":
                break;
            case "OnSpar":
                break;
            case "OnShare":
                break;
            case "OnInvite":
                break;
            case "OnKick":
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetInfo":
                npcData = data.Get<NpcData>();
                npcId = npcData.NpcId;
                SetNpcInfo();
                break;
        }
    }
    private void SetNpcInfo()
    {
        #region 기본 정보
        mTMPText["NameVal"].text = npcData.Name;
        mTMPText["LvVal"].text = npcData.Lv.ToString();
        mTMPText["ExpVal"].text = npcData.Exp.ToString();
        mTMPText["NextExpVal"].text = npcData.NextExp.ToString();
        mTMPText["GenVal"].text = GsManager.I.GetGen(npcData.Gen);

        GsManager.I.SetUiBaseParts(npcId, mGameObject, true);
        GsManager.I.SetUiEqParts(npcData, mGameObject);
        #endregion

        #region 상세 정보
        #endregion
        #region 상태 정보
        #endregion
        #region 설명 정보
        mTMPText["DescVal"].text = "";
        #endregion
        SetBotInfo(2);
    }
    private void SetBotInfo(int idx)
    {
        string[] arr = new string[] { "InfoObj", "StateObj", "DescObj" };
        foreach (var v in arr)
            mGameObject[v].SetActive(false);
        mGameObject[arr[idx]].SetActive(true);
    }
    public override void Refresh() { }
}