using GB;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityEnterPop : UIScreen
{
    public static bool isActive { get; private set; } = false;
    private int cityId; // 현재 도시 ID
    private List<int> splitData = new List<int>();
    public Dictionary<int, int> shopIdList = new Dictionary<int, int>();
    private int sId = 0; //상점 & 장소 ID
    private string sKey = ""; //상점 & 장소 키
    private void Awake()
    {
        Regist();
        RegistButton();
        RegistText();
    }
    private void OnEnable()
    {
        Presenter.Bind("CityEnterPop", this);
        isActive = true;
        cityId = 0; // 초기화
        WorldCore.I.enabled = false; // 월드맵 카메라 이동 비활성화
        splitData.Clear();
        shopIdList.Clear();
        sId = 0; sKey = "";
        StateCity(0);
    }
    private void OnDisable()
    {
        Presenter.UnBind("CityEnterPop", this);
        isActive = false;
        WorldCore.I.enabled = true; // 월드맵 카메라 이동 활성화
        UnityEngine.Debug.Log("CityEnterPop OnDisable");

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
                case "GoToGuild":
                    sId = 1; sKey = "Guild";
                    break;
                case "GoToInn":
                    sId = 2; sKey = "Inn";
                    break;
                case "GoToSmith": sId = 3; sKey = "Smith"; break;
                case "GoToTailor": sId = 4; sKey = "Tailor"; break;
                case "GoToApothecary": sId = 5; sKey = "Apothecary"; break;
                case "GoToMarket": sId = 6; sKey = "Market"; break;
                case "GoToTG": sId = 7; sKey = "TG"; break;
                case "GoToCath": sId = 8; sKey = "Cath"; break; // 성당
                case "GoToBook": sId = 9; sKey = "Book"; break; // 서점
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
                    break;
                case "OnQuest":
                    break;
                case "OnTrade":
                    OpenTrade();
                    break;
                case "OnMake":
                    break;
                case "OnWork":
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
                case "CityEnterPopClose":
                    Close();
                    break;
            }
        }
    }
    void StateCity(int id)
    {
        mGameObject["OutList"].SetActive(id == 0);
        mGameObject["InList"].SetActive(id == 1);
        if (id == 1)
        {
            var shop = ShopManager.I.GetShopData(shopIdList[sId]);
            mTexts["TitleVal"].text = GetTitleName(shop.Type);
            mTexts["JobVal"].text = GetJobName(shop.Type);
            var npc = NpcManager.I.NpcDataList[shop.NpcId];
            mTexts["NameVal"].text = npc.Name;
            mTexts["RlsVal"].text = npc.Rls.ToString();

            HumanAppearance.I.SetUiBaseParts(shop.NpcId, mGameObject);
            HumanAppearance.I.SetUiEqParts(npc, "NpcEq", mGameObject);
            SetInListPreset();
        }
    }
    void SetInListPreset()
    {
        foreach (var btn in mButtons.Where(b => b.Key.StartsWith("On")))
            btn.Value.gameObject.SetActive(false);

        switch (sId)
        {
            case 1:
                mButtons["OnJoin"].gameObject.SetActive(PlayerManager.I.pData.Grade == 0);
                mButtons["OnQuest"].gameObject.SetActive(true);
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
        StateBaseInList();
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
                mTexts["CityName"].text = CityManager.I.CityDataList[cityId].Name;
                string[] strArr = CityManager.I.CityDataList[cityId].Place.Split('_');
                splitData = strArr.Select(int.Parse).ToList();
                LoadPlace();
                break;
            case "UpdateGuildList":
                SetInListPreset();
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
            var result = ShopManager.I.shopAllData.Values
                .Where(data => data.CityId == cityId && data.Type == v)
                .ToList();
            foreach (var shop in result)
            {
                shopIdList.Add(shop.Type, shop.Id);
            }
        }
    }
    string GetTitleName(int idx)
    {
        switch (idx)
        {
            case 1:
                return "여관";
            case 2:
                return "대장간";
            case 3:
                return "재단소";
            case 4:
                return "약재상";
            case 5:
                return "시장";
        }
        return "모험가 길드";
    }
    string GetJobName(int idx)
    {
        switch (idx)
        {
            case 1:
                return "여관 주인";
            case 2:
                return "대장장이";
            case 3:
                return "재단사";
            case 4:
                return "약재 상인";
            case 5:
                return "상인";
        }
        return "길드 안내원";
    }
    public override void Refresh()
    {
    }
}