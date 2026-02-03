using GB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

public class CityEnterPop : UIScreen
{
    #region 변수
    public static bool isActive { get; private set; } = false;
    private int cityId; // 현재 도시 ID
    private List<int> splitData = new List<int>();
    public Dictionary<int, int> shopIdList = new Dictionary<int, int>();
    private int sId = 0; //상점 & 장소 ID
    public int npcId = 0; // NPC ID
    private string sKey = ""; //상점 & 장소 키
    private CanvasGroup canvasGrp;
    #endregion
    #region 닷 관련
    private Dictionary<int, List<string>> dotList = new Dictionary<int, List<string>>();
    #endregion
    #region 퀘스트 관련
    private List<QuestInstData> tGQstList = new List<QuestInstData>(); // 길드 안내원과의 대화를 통해 완료 및 진행이 되는 퀘스트들을 담아두는 리스트//변수명의 약자는 talk to the guild guide about my quest
    #endregion
    private void Awake()
    {
        Regist();
        RegistButton();
        RegistText();
        canvasGrp = GetComponent<CanvasGroup>();
        if (canvasGrp == null)
            canvasGrp = gameObject.AddComponent<CanvasGroup>();
        InitGuildQstTalkHandlers(); //길드 퀘스트 채팅 핸들러 초기화
    }
    private void OnEnable()
    {
        Presenter.Bind("CityEnterPop", this);
        isActive = true;
        cityId = 0; // 초기화
        splitData.Clear();
        shopIdList.Clear();
        tGQstList.Clear();
        sId = 0; sKey = "";
        if (GsManager.gameState == GameState.World) GsManager.I.InitCursor(); //월드맵에서 도시 입장 시 커서 초기화
    }
    private void OnDisable()
    {
        Presenter.UnBind("CityEnterPop", this);
        isActive = false;
        //도시 안으로 들어간 플레이어 활성화
        WorldCore.I.StatePlayer(true);
    }
    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });
    }
    public void RegistText()
    {
        foreach (var v in mTexts)
            v.Value.text = v.Key;
    }
    public void OnButtonClick(string key)
    {
        if (key.Contains("GoTo"))
        {
            switch (key)
            {
                case "GoToGuild": sId = 1; sKey = "Guild"; break;
                case "GoToInn": sId = 2; sKey = "Inn"; break;
                case "GoToSmith": sId = 3; sKey = "Smith"; break;
                case "GoToTailor": sId = 4; sKey = "Tailor"; break;
                case "GoToApothecary": sId = 5; sKey = "Apothecary"; break;
                case "GoToBook": sId = 6; sKey = "Book"; break; // 서점
                case "GoToMarket": sId = 7; sKey = "Market"; break; // 시장
            }
            StateCity(1);
        }
        else if (key.Contains("On"))
        {
            switch (key)
            {
                case "OnJoin":
                    if (sId == 1)
                    {
                        UIManager.ShowPopup("YesNoPop");
                        Presenter.Send("YesNoPop", "SendYesNoPopData", "GuildJoin/Al_GuildJoin");
                    }
                    break;
                case "OnRest":
                    if (PlayerManager.I.pData.Crown >= 50)
                    {
                        UIManager.ShowPopup("TalkPop");
                        Presenter.Send("TalkPop", "SetTalk", new TalkData("Rest", npcId));
                    }
                    else
                    {
                        UIManager.ShowPopup("TalkPop");
                        Presenter.Send("TalkPop", "SetTalk", new TalkData("InnNotCrown", npcId));
                    }
                    break;
                case "OnTalk":
                    TalkData tList;
                    switch (sId)
                    {
                        case 1:
                            tList = CheckQstTalkList();
                            UIManager.ShowPopup("TalkPop");
                            Presenter.Send("TalkPop", "SetTalk", tList);
                            break;
                    }
                    break;
                case "OnQuest":
                    List<int> qList = new List<int> { cityId, npcId };
                    UIManager.ShowPopup("GuildQuestPop");
                    Presenter.Send("GuildQuestPop", "SettingQuestPop", qList);
                    break;
                case "OnTrade":
                    OpenTrade();
                    break;
                case "OnMake":
                    StateVisiblePop(0);
                    UIManager.ShowPopup("InvenPop");
                    Presenter.Send("InvenPop", "OpenMakePop", sId);
                    break;
                case "OnWork":
                    //일하기 팝업 발생
                    UIManager.ShowPopup("WorkPop");
                    Presenter.Send("WorkPop", "SetWork", sId);
                    break;
                case "OnGetOut":
                    StateCity(0);
                    break;
            }
        }
        else
        {
            switch (key)
            {
                case "GetOutCity":
                    Close();
                    break;
            }
        }
    }
    void StateCity(int id)
    {
        mGameObject["OutList"].SetActive(id == 0);
        mGameObject["InList"].SetActive(id == 1);
        switch (id)
        {
            case 0:
                InitAllDots();
                break;
            case 1:
                var shop = PlaceManager.I.GetShopData(shopIdList[sId]);
                mTMPText["TitleVal"].text = LocalizationManager.GetValue(GetTitleName(shop.Type));
                mTMPText["JobVal"].text = LocalizationManager.GetValue(GetJobName(shop.Type));
                npcId = shop.NpcId;
                var npc = NpcManager.I.NpcDataList[npcId];
                mTMPText["NameVal"].text = npc.Name;
                mTMPText["RlsVal"].text = GetRlsState(npc.Rls);
                GsManager.I.SetUiBaseParts(npcId, mGameObject, true);
                GsManager.I.SetUiEqParts(npc, mGameObject);
                UpdateInListPreset();
                break;
        }
    }
    void UpdateInListPreset()
    {
        foreach (var btn in mButtons.Where(b => b.Key.StartsWith("On")))
            btn.Value.gameObject.SetActive(false);
        foreach (var obj in mGameObject.Where(b => b.Key.StartsWith("DI_")))
            obj.Value.SetActive(false);
        for (int i = 0; i < dotList[sId].Count; i++)
            mGameObject[dotList[sId][i]].SetActive(true);
        StateBaseInList();
        // 1 길드 2 여관 3 대장간 4 재단소 5 약재상 6 시장
        switch (sId)
        {
            case 1:
                mButtons["OnJoin"].gameObject.SetActive(PlayerManager.I.pData.Grade == 0);
                mButtons["OnQuest"].gameObject.SetActive(PlayerManager.I.pData.Grade > 0);
                mButtons["OnWork"].gameObject.SetActive(false);
                if (tGQstList.Any(x => x.Qid == 101)) //튜토리얼
                {
                    // int n = tGQstList.IndexOf(101);
                    switch (PlayerManager.I.pData.QuestList.Find(q => q.Qid == 101).Order)
                    {
                        case 1:
                            mButtons["OnJoin"].gameObject.SetActive(false);
                            mButtons["OnGetOut"].gameObject.SetActive(false);
                            break;
                        case 2:
                            mButtons["OnTalk"].gameObject.SetActive(false);
                            mButtons["OnGetOut"].gameObject.SetActive(false);
                            break;
                        case 3:
                        case 5:
                            mButtons["OnQuest"].gameObject.SetActive(false);
                            mButtons["OnGetOut"].gameObject.SetActive(false);
                            break;
                    }
                }
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                if (sId == 2) mButtons["OnRest"].gameObject.SetActive(true);
                mButtons["OnTrade"].gameObject.SetActive(true);
                mButtons["OnMake"].gameObject.SetActive(true);
                break;
            case 6:
            case 7:
                mButtons["OnTrade"].gameObject.SetActive(true);
                break;
        }
    }
    void StateBaseInList()
    {
        string[] keys = { "OnTalk", "OnWork", "OnGetOut" };
        foreach (var key in keys)
            mButtons[key].gameObject.SetActive(true);
    }
    void OpenTrade()
    {
        // int shopId = shopList[sId];
        UIManager.ShowPopup("ShopInvenPop");
        UIManager.ShowPopup("InvenPop");
        Presenter.Send("ShopInvenPop", "Load" + sKey, shopIdList[sId]);
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "EnterCity":
                cityId = data.Get<int>();
                mTMPText["CityName"].text = LocalizationManager.GetValue(PlaceManager.I.CityDic[cityId].Name);
                string[] strArr = PlaceManager.I.CityDic[cityId].Place.Split('_');
                splitData = strArr.Select(int.Parse).ToList();
                LoadPlace();
                StateCity(0); //도시 입장 상태여서 한번 도시 정보 업데이트
                break;
            case "UpdateCityList":
                InitAllDots();
                UpdateInListPreset();
                break;
            case "StateVisiblePop":
                StateVisiblePop(data.Get<int>());
                break;
            case "AddNpcRls":
                NpcManager.I.NpcDataList[npcId].Rls += data.Get<int>();
                mTMPText["RlsVal"].text = GetRlsState(NpcManager.I.NpcDataList[npcId].Rls);
                break;
            case "Tuto_Join":
                mButtons["OnJoin"].gameObject.SetActive(true);
                tGQstList.Clear();
                InitAllDots();
                UpdateInListPreset();
                break;
            case "Inn_NotCrown":
                UIManager.ShowPopup("TalkPop");
                Presenter.Send("TalkPop", "SetTalk", new TalkData("InnNotCrown", npcId));
                break;
        }
    }
    private void LoadPlace()
    {
        foreach (var btn in mButtons.Where(b => b.Key.StartsWith("GoTo")))
            btn.Value.gameObject.SetActive(false);

        int y = 350;
        foreach (var v in splitData)
        {
            string btnKey = null;
            switch (v)
            {
                case 1: btnKey = "GoToGuild"; break;
                case 2: btnKey = "GoToInn"; break;
                case 3: btnKey = "GoToSmith"; break;
                case 4: btnKey = "GoToTailor"; break;
                case 5: btnKey = "GoToApothecary"; break;
                case 6: btnKey = "GoToBook"; break;
                case 7: btnKey = "GoToMarket"; break;
            }

            if (btnKey != null && mButtons.ContainsKey(btnKey))
            {
                var btn = mButtons[btnKey];
                btn.gameObject.SetActive(true);

                // 버튼의 위치를 y값으로 지정
                var pos = btn.transform.localPosition;
                btn.transform.localPosition = new UnityEngine.Vector3(pos.x, y, pos.z);

                y -= 100;
            }
            // shopAllData에서 조건에 맞는 데이터 찾기
            var result = PlaceManager.I.shopAllData.Values
                .Where(data => data.CityId == cityId && data.Type == v)
                .ToList();
            foreach (var shop in result)
            {
                shopIdList.Add(shop.Type, shop.Id);
            }
        }
        //튜토리얼 체크
        if (PlayerManager.I.pData.QuestList.FindIndex(q => q.Qid == 101) != -1)
        {
            foreach (var btn in mButtons.Where(b => b.Key.StartsWith("GoTo")))
                btn.Value.gameObject.SetActive(false);
            mButtons["GoToGuild"].gameObject.SetActive(true);
        }
    }

    void StateVisiblePop(int alpha)
    {
        canvasGrp.alpha = alpha;
    }
    string GetTitleName(int idx)
    {
        switch (idx)
        {
            case 1: return "Guild";
            case 2: return "Inn";
            case 3: return "Smith";
            case 4: return "TailorShop";
            case 5: return "Apothecary1";
            case 6: return "Book";
            case 7: return "Market";
            case 8: return "Church";
            case 21: return "Farm";
            case 22: return "Ranch";
            case 23: return "Sawmill";
            case 24: return "Mine";
        }
        return "Guild";
    }
    string GetJobName(int idx)
    {
        switch (idx)
        {
            case 1: return "GuildReceptionist";
            case 2: return "Innkeeper";
            case 3: return "Blacksmith";
            case 4: return "Tailor";
            case 5: return "Apothecary2";
            case 6: return "Bookseller";
            case 7: return "Merchant";
            case 8: return "Priest";
            case 21: return "Farmer";
            case 22: return "Rancher";
            case 23: return "Carpenter";
            case 24: return "Miner";
        }
        return "GuildReceptionist";
    }
    string GetRlsState(int num)
    {
        if (num > 4000)
            return "매우 좋음";
        else if (num > 1000)
            return "좋음";
        else if (num > 100)
            return "약간 좋음";
        else if (num >= 0)
            return "보통";
        else if (num >= -100)
            return "약간 나쁨";
        else if (num >= -500)
            return "나쁨";
        else
            return "매우 나쁨";
    }
    public override void Refresh() { }
    #region 기타
    void InitAllDots()
    {
        foreach (var obj in mGameObject.Where(b => b.Key.StartsWith("DO_")))
            obj.Value.SetActive(false);
        dotList.Clear();
        int idx = 1;
        foreach (var p in mButtons.Where(b => b.Key.StartsWith("GoTo")))
        {
            if (!p.Value.gameObject.activeSelf) continue;
            dotList.Add(idx, new List<string>());
            switch (p.Key)
            {
                case "GoToGuild":
                    foreach (var q in PlayerManager.I.pData.QuestList)
                    {
                        switch (q.Qid)
                        {
                            case 1:
                                if (q.State == 1 && q.CityId == cityId)
                                {
                                    tGQstList.Add(q);
                                    if (!dotList[idx].Contains("DI_Talk"))
                                        dotList[idx].Add("DI_Talk");
                                }
                                break;
                            case 101:
                                tGQstList.Add(q);
                                switch (q.Order)
                                {
                                    case 1:
                                    case 3:
                                    case 5:
                                        if (!dotList[idx].Contains("DI_Talk"))
                                            dotList[idx].Add("DI_Talk");
                                        break;
                                }
                                break;
                        }
                        if (q.State == 2 && !dotList[idx].Contains("DI_Quest"))
                            dotList[idx].Add("DI_Quest");
                    }
                    break;
                case "GoToInn":
                    break;
                case "GoToSmith":
                    break;
                case "GoToTailor":
                    break;
                case "GoToApothecary":
                    break;
                case "GoToMarket":
                    break;
                case "GoToTG":
                    break;
                case "GoToArena":
                    break;
            }
            idx++;
        }
        foreach (var d in dotList)
        {
            if (d.Value.Count > 0)
            {
                switch (d.Key)
                {
                    case 1: mGameObject["DO_Guild"].SetActive(true); break;
                    case 2: mGameObject["DO_Inn"].SetActive(true); break;
                    case 3: mGameObject["DO_Smith"].SetActive(true); break;
                    case 4: mGameObject["DO_Tailor"].SetActive(true); break;
                    case 5: mGameObject["DO_Apoth"].SetActive(true); break;
                    case 6: mGameObject["DO_Market"].SetActive(true); break;
                    case 7: mGameObject["DO_TG"].SetActive(true); break;
                    case 8: mGameObject["DO_Arena"].SetActive(true); break;
                }
            }
        }
    }
    #endregion
    #region 대화 관련
    private Dictionary<int, Func<int, TalkData>> qstTalkHandlers = new Dictionary<int, Func<int, TalkData>>();
    private void InitGuildQstTalkHandlers()
    {
        // 퀘스트 ID별 처리 로직을 딕셔너리에 등록
        qstTalkHandlers.Clear();
        qstTalkHandlers[101] = hQst101;
        qstTalkHandlers[1] = hQst1;
        // 새로운 퀘스트 추가 시 여기에만 등록하면 됨
        // questTalkHandlers[102] = HandleQuest10
    }
    private TalkData hQst101(int order)
    {
        return new TalkData("Qst", npcId, 101, order);
    }
    private TalkData hQst1(int order)
    {
        return new TalkData("Qst", npcId, 1, order);
    }
    private TalkData CheckQstTalkList()
    {
        //대화 타입, 퀘스트 ID, NPC ID, Order ID
        List<TalkData> priority = new List<TalkData>();
        switch (sId)
        {
            case 1:
                priority.Add(hQst101(0));
                priority.Add(hQst1(0));
                break;
            case 2:
                break;
            case 3:
                break;
        }
        if (priority.Count == 0)
            return new TalkData("Normal", npcId, 0, 0);
        foreach (TalkData talkData in priority)
        {
            if (tGQstList.Any(x => x.Qid == talkData.Tid) && qstTalkHandlers.ContainsKey(talkData.Tid))
                return qstTalkHandlers[talkData.Tid](npcId);
        }

        // 일반 채팅
        return new TalkData("Normal", npcId, 0, 0);
    }
    #endregion
}