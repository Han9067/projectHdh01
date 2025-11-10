using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using GB;
using UnityEngine.UI;
using System.Linq;
//GameSystemManager
public class GsManager : AutoSingleton<GsManager>
{
    public int worldSpd = 1; //월드맵 속도
    private void Awake()
    {
        #region 초기화
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        #endregion

        #region 커서 관리
        string[] strArr = { "default", "attack", "notMove", "enter" };
        Vector2[] offsetArr = { new Vector2(28, 14), new Vector2(32, 32), new Vector2(28, 14), new Vector2(32, 32) };
        for (int i = 0; i < strArr.Length; i++)
        {
            Texture2D cursorTexture = Resources.Load<Texture2D>($"Images/UI/Cursor/cursor_{strArr[i]}");
            csrTextures.Add(strArr[i], cursorTexture);
            csrOffsets.Add(strArr[i], offsetArr[i]);
        }
        SetCursor("default");
        #endregion

        #region 데이터 로드
        LoadStatData();
        LoadSkData();
        #endregion

        #region 매니저 스크립트 초기화
        ItemManager.I.LoadItemManager();
        PlayerManager.I.LoadPlayerManager();
        MonManager.I.LoadMonManager();
        NpcManager.I.LoadNpcManager();
        QuestManager.I.LoadQuestManager();
        PlaceManager.I.LoadPlaceManager();
        SaveFileManager.I.LoadSaveFileManager();
        #endregion
    }
    void Start()
    {
        if (tDay < 113850)
        {
            tDay = 113850;
            wTime = 20f;
        }
        wYear = tDay / 360;
        wMonth = tDay % 360 / 30;
        wDay = tDay % 30 + 1;
        Presenter.Send("WorldMainUI", "UpdateAllTime");
    }
    #region 메뉴 팝업 상태
    public void StateMenuPopup(string key)
    {
        GameObject go = null;
        string str = "";

        switch (key)
        {
            case "StateCharInfoPop":
                go = GameObject.Find("CharInfoPop");
                str = "CharInfoPop";
                break;
            case "StateInvenPop":
                go = GameObject.Find("InvenPop");
                str = "InvenPop";
                if (GameObject.Find("SkillPop") != null && GameObject.Find("SkillPop").activeSelf)
                    UIManager.ClosePopup("SkillPop");
                break;
            case "StateJournalPop":
                go = GameObject.Find("JournalPop");
                str = "JournalPop";
                break;
            case "StateSkillPop":
                go = GameObject.Find("SkillPop");
                str = "SkillPop";
                if (GameObject.Find("InvenPop") != null && GameObject.Find("InvenPop").activeSelf)
                    UIManager.ClosePopup("InvenPop");
                break;
        }

        if (go == null)
        {
            UIManager.ShowPopup(str);
        }
        else
        {
            if (go.gameObject.activeSelf)
                UIManager.ClosePopup(str);
            else
                UIManager.ShowPopup(str);
        }
    }
    #endregion
    #region 커서 관리
    public string cName = "default";
    Dictionary<string, Texture2D> csrTextures = new Dictionary<string, Texture2D>();
    Dictionary<string, Vector2> csrOffsets = new Dictionary<string, Vector2>();
    public void SetCursor(string name)
    {
        cName = name;
        Cursor.SetCursor(csrTextures[cName], csrOffsets[cName], CursorMode.Auto);
    }
    public bool IsCursor(string name)
    {
        return cName == name;
    }
    #endregion

    #region 레벨 관리
    public int GetLv(int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK)
    {
        int total = (VIT + END + STR + AGI + FOR + INT + CHA + LUK) - 40;
        return total < 1 ? 1 : total;
    }
    public int GetNextExp(int Lv, double s = 30.0, double p = 2.8)
    {
        double C = 1000.0 / Math.Pow(1.0 + s, p);
        double raw = C * Math.Pow(Lv + s, p);
        return (int)(Math.Floor(raw / 10.0 + 0.5) * 10.0);
    }
    public int GetGainExp(int _hp, int _sp, int _mp, int _str, int _agi, int _int, int _cha, int _luk)
    {
        double score = _hp + (_sp + _mp) * 1 + (_str + _agi) * 0.8 + (_int + _cha + _luk) * 0.5;
        return (int)(score * 4);
    }
    #endregion

    #region 오브젝트 레이어 관리
    public int GetObjLayer(float y)
    {
        return (int)((80 - y) * 100);
    }
    #endregion

    #region 시간 관리
    public int tDay = 0;
    public float wTime = 0;
    public int wYear, wMonth, wDay;
    public void SetAllTime(int tot, int y, int m, int d, float t)
    {
        tDay = tot;
        wYear = y;
        wMonth = m;
        wDay = d;
        wTime = t;
    }
    #endregion

    #region 캐릭터 외형 관리
    public void SetUiBaseParts(int id, Dictionary<string, GameObject> obj, string addKey = "")
    {
        Dictionary<string, Image> img = new Dictionary<string, Image>();
        string[] str = { "Face", "Eyebrow", "Eye1", "Eye2", "Ear", "Nose", "Mouth", "BaseBody",
            "BaseHand1A", "BaseHand1A2", "BaseHand1B", "BaseHand2", "BaseBoth", "Hair1A", "Hair1B", "Hair2" };
        for (int i = 0; i < str.Length; i++)
            img[str[i]] = obj[addKey + str[i]].GetComponent<Image>();


        ICharData data = id == 0 ? PlayerManager.I.pData : NpcManager.I.NpcDataList[id];
        img["Face"].sprite = ResManager.GetSprite($"Face_{data.Face}");
        img["Eyebrow"].sprite = ResManager.GetSprite($"Eyebrow_{data.Eyebrow}");
        img["Eye1"].sprite = ResManager.GetSprite($"Eye_{data.Eye}_1");
        img["Eye2"].sprite = ResManager.GetSprite($"Eye_{data.Eye}_2");
        img["Ear"].sprite = ResManager.GetSprite($"Ear_{data.Ear}");
        img["Nose"].sprite = ResManager.GetSprite($"Nose_{data.Nose}");
        img["Mouth"].sprite = ResManager.GetSprite($"Mouth_{data.Mouth}");

        Color skinColor = ColorData.GetSkinColor(data.Skin);
        Color hairColor = ColorData.GetHairColor(data.HairColor);
        Color eyeColor = ColorData.GetEyeColor(data.EyeColor);
        img["BaseBody"].sprite = ResManager.GetSprite($"Body{data.Gen}");
        img["Face"].color = skinColor; img["Ear"].color = skinColor;
        img["BaseBody"].color = skinColor; img["BaseHand1A"].color = skinColor;
        img["BaseHand1A2"].color = skinColor; img["BaseHand1B"].color = skinColor;
        img["BaseHand2"].color = skinColor; img["BaseBoth"].color = skinColor;
        //*1~100 남자 머리A타입(앞머리,뒷머리 존재)/ 101~200 남자 머리B타입(앞머리만 존재)
        //*201~300 여자 머리A타입(앞머리, 뒷머리 존재)/301~400 여자 머리B타입(앞머리만 존재)
        switch (data.Hair)
        {
            case 1:
                img["Hair1A"].gameObject.SetActive(true); img["Hair1B"].gameObject.SetActive(false); img["Hair2"].gameObject.SetActive(true);
                img["Hair1A"].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                img["Hair2"].sprite = ResManager.GetSprite("Hair_2_" + data.Hair);
                img["Hair1A"].color = hairColor;
                img["Hair2"].color = hairColor;
                break;
            case 101:
            case 102:
                img["Hair1A"].gameObject.SetActive(true); img["Hair1B"].gameObject.SetActive(false); img["Hair2"].gameObject.SetActive(false);
                img["Hair1A"].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                img["Hair1A"].color = hairColor;
                break;
            case 201:
            case 202:
            case 203:
            case 204:
                img["Hair1A"].gameObject.SetActive(false); img["Hair1B"].gameObject.SetActive(true); img["Hair2"].gameObject.SetActive(true);
                img["Hair1B"].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                img["Hair2"].sprite = ResManager.GetSprite("Hair_2_" + data.Hair);
                img["Hair1B"].color = hairColor;
                img["Hair2"].color = hairColor;
                break;
            case 301:
                img["Hair1A"].gameObject.SetActive(false); img["Hair1B"].gameObject.SetActive(true); img["Hair2"].gameObject.SetActive(false);
                img["Hair1B"].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                img["Hair1B"].color = hairColor;
                break;
        }
        img["Eye2"].color = eyeColor;
    }
    public void SetUiEqParts(ICharData data, string backUpKey, Dictionary<string, GameObject> mGameObj, string addKey = "")
    {
        var eq = data.EqSlot;
        string[] allParts = {"BaseHand1A", "BaseHand1A2", "BaseHand1B", "BaseHand2", "BaseBoth",
            "EqBody", "EqHand1A", "EqHand1B", "EqHand2", "EqBoth", "OneWp1", "OneWp2", "TwoWp1", "TwoWp2", "TwoWp3"};
        foreach (var v in allParts)
            mGameObj[addKey + v].SetActive(false);

        if (eq["Armor"] != null)
        {
            string eqStr = eq["Armor"].ItemId.ToString();
            mGameObj[addKey + "EqBody"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_body");
            mGameObj[addKey + "EqBody"].SetActive(true);
            if (backUpKey != eqStr + "_body")
            {
                if (eq["Armor"].ItemId > 10500)
                {
                    mGameObj[addKey + "EqHand2"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand2");
                    mGameObj[addKey + "EqHand2"].SetActive(true);
                    mGameObj[addKey + "BaseHand2"].SetActive(true);
                    mGameObj[addKey + "BaseHand1A"].SetActive(true);
                    return;
                }
                else
                {
                    mGameObj[addKey + "EqHand1A"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1A");
                    mGameObj[addKey + "EqHand1B"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1B");
                    mGameObj[addKey + "EqHand2"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand2");
                    mGameObj[addKey + "EqBoth"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_both");
                }
            }
        }

        List<string> parts = GetHandParts(eq);
        foreach (var v in parts)
            mGameObj[addKey + v].SetActive(true);
        string[] hKey = { "Hand1", "Hand2" };
        foreach (var v in hKey)
        {
            if (eq[v] != null)
            {
                switch (eq[v].Both)
                {
                    case 0:
                        string one = v == "Hand1" ? "OneWp1" : "OneWp2";
                        mGameObj[addKey + one].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq[v].ItemId.ToString());
                        mGameObj[addKey + one].SetActive(true);
                        break;
                    case 1:
                        mGameObj[addKey + "TwoWp1"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                        mGameObj[addKey + "TwoWp1"].SetActive(true);
                        break;
                    case 2:
                        mGameObj[addKey + "TwoWp2"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                        mGameObj[addKey + "TwoWp2"].SetActive(true);
                        break;
                }
            }
        }
    }
    List<string> GetHandParts(Dictionary<string, ItemData> eq)
    {
        bool hasArmor = eq["Armor"] != null;
        List<string> parts = new List<string>();
        if (eq["Hand1"] == null && eq["Hand2"] == null)
            parts = hasArmor ? new List<string> { "BaseHand1A", "BaseHand2", "EqHand1A", "EqHand2" } : new List<string> { "BaseHand1A", "BaseHand2" };
        else
        {
            string[] eqHand = { "Hand1", "Hand2" };
            foreach (var v in eqHand)
            {
                if (eq[v] != null)
                {
                    switch (eq[v].Both)
                    {
                        case 1: parts = hasArmor ? new List<string> { "BaseBoth", "EqBoth" } : new List<string> { "BaseBoth" }; break;
                        case 2:
                            parts = hasArmor ? new List<string> { "BaseHand1A", "BaseHand1A2", "BaseHand2", "EqHand1A", "EqHand2" } :
                         new List<string> { "BaseHand1A", "BaseHand1A2", "BaseHand2" }; break;
                    }
                }
            }
            if (parts.Count == 0)
            {
                parts = hasArmor ? new List<string> { "BaseHand2", "EqHand2" } : new List<string> { "BaseHand2" };
                if (eq["Hand1"] != null)
                {
                    parts.Add("BaseHand1B");
                    if (hasArmor) parts.Add("EqHand1B");
                }
                else
                {
                    parts.Add("BaseHand1A");
                    if (hasArmor) parts.Add("EqHand1A");
                }
            }
        }
        return parts;
    }
    public void InitParts(Dictionary<PtType, SpriteRenderer> ptSpr, GameObject ptMain)
    {
        foreach (PtType PtType in System.Enum.GetValues(typeof(PtType)))
        {
            string partName = PtType.ToString();
            ptSpr[PtType] = ptMain.transform.Find(partName).GetComponent<SpriteRenderer>();
        }
    }
    public void SetObjAppearance(int uid, Dictionary<PtType, SpriteRenderer> ptSpr)
    {
        //uid가 0이면 무조건 플레이어 1부터는 NPC
        ICharData data = uid == 0 ? PlayerManager.I.pData : NpcManager.I.NpcDataList[uid];

        Color skinColor = ColorData.GetSkinColor(data.Skin);
        Color hairColor = ColorData.GetHairColor(data.HairColor);
        Color eyeColor = ColorData.GetEyeColor(data.EyeColor);
        PtType[] skinParts = new PtType[] { PtType.Face, PtType.Ear, PtType.BaseBody, PtType.BaseHand1A, PtType.BaseHand1A2, PtType.BaseHand1B, PtType.BaseHand2, PtType.BaseBoth };
        foreach (PtType skinPart in skinParts)
            ptSpr[skinPart].color = skinColor;
        PtType[] hairParts = new PtType[] { PtType.FrontHair1, PtType.FrontHair2, PtType.BackHair };
        foreach (PtType hairPart in hairParts)
            ptSpr[hairPart].color = hairColor;

        switch (data.Hair)
        {
            case 1:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(false);
                ptSpr[PtType.BackHair].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair1].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                ptSpr[PtType.BackHair].sprite = ResManager.GetSprite("Hair_2_" + data.Hair);
                ptSpr[PtType.FrontHair1].color = hairColor;
                ptSpr[PtType.BackHair].color = hairColor;
                break;
            case 2:
            case 3:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(false);
                ptSpr[PtType.BackHair].gameObject.SetActive(false);
                ptSpr[PtType.FrontHair1].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                ptSpr[PtType.FrontHair1].color = hairColor;
                break;
            case 100:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(false);
                ptSpr[PtType.FrontHair2].gameObject.SetActive(true);
                ptSpr[PtType.BackHair].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                ptSpr[PtType.FrontHair2].color = hairColor;
                ptSpr[PtType.BackHair].sprite = ResManager.GetSprite("Hair_2_" + data.Hair);
                ptSpr[PtType.BackHair].color = hairColor;
                break;
        }
        ptSpr[PtType.Eye2].color = eyeColor;
        PtType[] bParts = new PtType[] {PtType.BaseHand1A, PtType.BaseHand1A2, PtType.BaseHand1B, PtType.BaseHand2, PtType.BaseBoth,
            PtType.EqBody, PtType.EqHand1A, PtType.EqHand1B, PtType.EqHand2, PtType.EqBoth, PtType.OneWp1, PtType.OneWp2, PtType.TwoWp1, PtType.TwoWp2, PtType.TwoWp3 };
        foreach (PtType bbb in bParts)
            ptSpr[bbb].gameObject.SetActive(false);

        int[] handState = new int[2] { -1, -1 };
        string[] handKeys = new string[] { "Hand1", "Hand2" };
        for (int i = 0; i < 2; i++)
        {
            if (data.EqSlot[handKeys[i]] != null)
            {
                handState[i] = data.EqSlot[handKeys[i]].Both;
                SetHandSprite(ptSpr, data.EqSlot[handKeys[i]], handState[i]);
            }
        }

        if (data.EqSlot["Armor"] != null)
        {
            string eqStr = data.EqSlot["Armor"].ItemId.ToString();
            ptSpr[PtType.EqBody].sprite = ResManager.GetSprite(eqStr + "_body");
            ptSpr[PtType.EqHand1A].sprite = ResManager.GetSprite(eqStr + "_hand1A");
            ptSpr[PtType.EqHand1B].sprite = ResManager.GetSprite(eqStr + "_hand1B");
            ptSpr[PtType.EqHand2].sprite = ResManager.GetSprite(eqStr + "_hand2");
            ptSpr[PtType.EqBoth].sprite = ResManager.GetSprite(eqStr + "_both");

            ptSpr[PtType.EqBody].gameObject.SetActive(true);
            switch (handState[0])
            {
                case -1: ptSpr[PtType.BaseHand1A].gameObject.SetActive(true); ptSpr[PtType.EqHand1A].gameObject.SetActive(true); break;
                case 0: ptSpr[PtType.BaseHand1B].gameObject.SetActive(true); ptSpr[PtType.EqHand1B].gameObject.SetActive(true); break;
                case 1: ptSpr[PtType.BaseBoth].gameObject.SetActive(true); ptSpr[PtType.EqBoth].gameObject.SetActive(true); break;
                case 2: ptSpr[PtType.BaseHand1A].gameObject.SetActive(true); ptSpr[PtType.BaseHand1A2].gameObject.SetActive(true); ptSpr[PtType.EqHand1A].gameObject.SetActive(true); break;
            }
            if (handState[1] != 1)
            {
                ptSpr[PtType.BaseHand2].gameObject.SetActive(true);
                ptSpr[PtType.EqHand2].gameObject.SetActive(true);
            }
        }
    }
    private void SetHandSprite(Dictionary<PtType, SpriteRenderer> ptSpr, ItemData data, int handIndex)
    {
        var sprType = data.Both switch
        {
            0 => handIndex == 0 ? PtType.OneWp1 : PtType.OneWp2,
            1 => (handIndex == 0 && data.Type == 2) ? PtType.TwoWp1 : PtType.TwoWp2,
            _ => PtType.TwoWp3
        };

        ptSpr[sprType].gameObject.SetActive(true);
        ptSpr[sprType].sprite = ResManager.GetSprite("wp" + data.ItemId.ToString());
    }
    #endregion

    #region 스탯 관리
    private StatTable _statTable;
    public StatTable StatTable => _statTable ?? (_statTable = GameDataManager.GetTable<StatTable>());
    public Dictionary<int, StatData> StatDataList = new Dictionary<int, StatData>();

    private void LoadStatData()
    {
        foreach (var stat in StatTable.Datas)
        {
            StatDataList[stat.StatID] = new StatData(stat.StatID, stat.Name);
        }
    }
    #endregion
    #region 스킬 관리
    private SkTable _skTable;
    public SkTable SkTable => _skTable ?? (_skTable = GameDataManager.GetTable<SkTable>());
    public Dictionary<int, SkData> SkDataList = new Dictionary<int, SkData>();
    private void LoadSkData()
    {
        foreach (var sk in SkTable.Datas)
        {
            SkDataList[sk.SkID] = new SkData { SkId = sk.SkID, Type = sk.Type, Cool = sk.Cool, Name = sk.Name };
        }
    }
    public int GetSkNextExp(int lv)
    {
        int[] limitExp = new int[] { 0, 100, 200, 300, 400, 500, 600, 700, 800, 1000 };
        if (lv < 10)
            return limitExp[lv];
        else
            return lv * 10000;
    }
    // var baseSk = GsManager.SkDataList[skId];
    // var npcSk = baseSk.Clone(); // 또는 new SkData(baseSk);
    #endregion
}
