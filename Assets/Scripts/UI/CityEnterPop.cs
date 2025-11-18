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
    private int npcId = 0; // NPC ID
    private string sKey = ""; //상점 & 장소 키
    private CanvasGroup canvasGrp;
    #endregion
    #region 닷 관련
    private Dictionary<int, List<string>> dotList = new Dictionary<int, List<string>>();
    #endregion
    #region 퀘스트 관련
    private bool isChatGQst1 = false; // 편지 전달 퀘스트 체크
    #endregion
    private void Awake()
    {
        Regist();
        RegistButton();
        RegistText();
        canvasGrp = GetComponent<CanvasGroup>();
        if (canvasGrp == null)
            canvasGrp = gameObject.AddComponent<CanvasGroup>();
    }
    private void OnEnable()
    {
        Presenter.Bind("CityEnterPop", this);
        isActive = true;
        cityId = 0; // 초기화
        splitData.Clear();
        shopIdList.Clear();
        sId = 0; sKey = "";
    }
    private void OnDisable()
    {
        Presenter.UnBind("CityEnterPop", this);
        isActive = false;
        InitQuestCheck(); // 퀘스트 체크 초기화
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
                case "GoToMarket": sId = 6; sKey = "Market"; break;
                case "GoToBook": sId = 7; sKey = "Book"; break; // 서점
                case "GoToCath": sId = 8; sKey = "Cath"; break; // 성당
                case "GoToTG": sId = 9; sKey = "TG"; break;
                case "GoToArena": sId = 10; sKey = "Arena"; break; // 투기장
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
                case "OnChat":
                    switch (sId)
                    {
                        case 1:
                            if (isChatGQst1)
                            {
                                isChatGQst1 = false;
                                UIManager.ShowPopup("ChatPop");
                                Presenter.Send("ChatPop", "ChatGQst1", npcId);
                            }
                            break;
                    }
                    break;
                case "OnQuest":
                    UIManager.ShowPopup("GuildQuestPop");
                    Presenter.Send("GuildQuestPop", "SettingQuestPop", cityId);
                    break;
                case "OnTrade":
                    OpenTrade();
                    break;
                case "OnMake":
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
                mTMPText["TitleVal"].text = GetTitleName(shop.Type);
                mTMPText["JobVal"].text = GetJobName(shop.Type);
                npcId = shop.NpcId;
                var npc = NpcManager.I.NpcDataList[npcId];
                mTMPText["NameVal"].text = npc.Name;
                mTMPText["RlsVal"].text = GetRlsState(npc.Rls);
                GsManager.I.SetUiBaseParts(npcId, mGameObject);
                GsManager.I.SetUiEqParts(npc, "NpcEq", mGameObject);
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
                mButtons["OnQuest"].gameObject.SetActive(true);
                mButtons["OnWork"].gameObject.SetActive(false);
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                mButtons["OnTrade"].gameObject.SetActive(true);
                mButtons["OnMake"].gameObject.SetActive(true);
                break;
            case 6:
                mButtons["OnTrade"].gameObject.SetActive(true);
                break;
        }
    }
    void StateBaseInList()
    {
        string[] keys = { "OnChat", "OnWork", "OnGetOut", "OnSend" };
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
        }
    }
    private void LoadPlace()
    {
        string[] keys = { "GoToGuild", "GoToInn", "GoToSmith", "GoToTailor", "GoToApothecary", "GoToMarket", "GoToTG", "GoToArena" };
        foreach (var key in keys)
            mButtons[key].gameObject.SetActive(false);
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
                case 6: btnKey = "GoToMarket"; break;
                case 7: btnKey = "GoToTG"; break;
                case 8: btnKey = "GoToCath"; break;
                case 9: btnKey = "GoToBook"; break;
                case 10: btnKey = "GoToArena"; break;
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
    }

    void StateVisiblePop(int alpha)
    {
        canvasGrp.alpha = alpha;
    }
    string GetTitleName(int idx)
    {
        switch (idx)
        {
            case 1:
                return "길드";
            case 2:
                return "여관";
            case 3:
                return "대장간";
            case 4:
                return "재단소";
            case 5:
                return "약재상";
            case 6:
                return "시장";
        }
        return "길드";
    }
    string GetJobName(int idx)
    {
        switch (idx)
        {
            case 1:
                return "길드 안내원";
            case 2:
                return "여관 주인";
            case 3:
                return "대장장이";
            case 4:
                return "재단사";
            case 5:
                return "약재 상인";
            case 6:
                return "상인";
        }
        return "길드 안내원";
    }
    string GetRlsState(int num)
    {
        if (num > 1000)
            return "매우 좋음";
        else if (num > 500)
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
    public override void Refresh()
    {
    }
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
                                    isChatGQst1 = true;
                                    dotList[idx].Add("DI_Chat");
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
    void InitQuestCheck()
    {
        isChatGQst1 = false;
    }
    #endregion
}