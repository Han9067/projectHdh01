using System.Collections.Generic;
using System;
using UnityEngine;
using GB;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using System.Linq;


public class GsManager : AutoSingleton<GsManager>
{
    public static GameState gameState;
    public static int worldSpd = 1; //월드맵 속도
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
        LoadAttData();
        LoadSkData();
        LoadMakeData();
        #endregion

        #region 매니저 스크립트 초기화
        ItemManager.I.LoadItemManager();
        // PlayerManager.I.LoadPlayerManager();
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
        switch (SceneManager.GetActiveScene().name)
        {
            case "World":
                gameState = GameState.World;
                break;
            case "Battle":
                gameState = GameState.Battle;
                break;
            default:
                gameState = GameState.Menu;
                break;
        }
    }
    #region 메뉴 팝업 상태
    public void StateMenuPopup(string key)
    {
        string str = key.Substring(2);
        switch (str)
        {
            case "CharInfoPop":
                if (CharInfoPop.isActive)
                    UIManager.ClosePopup("CharInfoPop");
                else
                    UIManager.ShowPopup("CharInfoPop");
                break;
            case "InvenPop":
                if (InvenPop.isActive)
                    UIManager.ClosePopup("InvenPop");
                else
                    UIManager.ShowPopup("InvenPop");
                break;
            case "JournalPop":
                if (JournalPop.isActive)
                    UIManager.ClosePopup("JournalPop");
                else
                    UIManager.ShowPopup("JournalPop");
                break;
            case "SkillPop":
                if (SkillPop.isActive)
                    UIManager.ClosePopup("SkillPop");
                else
                    UIManager.ShowPopup("SkillPop");
                break;
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
    public void InitCursor()
    {
        if (!IsCursor("default")) SetCursor("default");
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
    public void SetUiBaseParts(int id, Dictionary<string, GameObject> obj, bool isPrf = false, string addKey = "")
    {
        Dictionary<string, Image> img = new Dictionary<string, Image>();
        string[] str = isPrf ?
            new string[] { "Face", "Eyebrow", "Eye1", "Eye2", "Ear", "Nose", "Mouth", "BaseBody",
            "BaseHand1A", "BaseHand2", "Hair1A", "Hair1B", "Hair2" } :
            new string[] { "Face", "Eyebrow", "Eye1", "Eye2", "Ear", "Nose", "Mouth", "BaseBody",
            "BaseHand1A", "BaseHand1B", "BaseHand2", "BaseBoth", "Hair1A", "Hair1B", "Hair2" };
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
        img["BaseHand2"].color = skinColor;
        if (!isPrf)
        {
            img["BaseHand1B"].color = skinColor;
            img["BaseBoth"].color = skinColor;
        }
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
    public void SetUiEqParts(ICharData data, Dictionary<string, GameObject> mGameObj, string addKey = "")
    {
        var eq = data.EqSlot;
        string[] parts = new string[] { "EqBody", "EqHand1A", "EqHand2" };
        foreach (var v in parts)
            mGameObj[addKey + v].SetActive(false);

        if (eq["Armor"] != null)
        {
            string eqStr = eq["Armor"].ItemId.ToString();
            mGameObj[addKey + "EqBody"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_body");
            mGameObj[addKey + "EqHand2"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand2");
            mGameObj[addKey + "EqBody"].SetActive(true);
            mGameObj[addKey + "EqHand2"].SetActive(true);
            if (eq["Armor"].ItemId < 10501) //상점 npc 복장엔 손1번이 없다.
            {
                mGameObj[addKey + "EqHand1A"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1A");
                mGameObj[addKey + "EqHand1A"].SetActive(true);
            }
        }
    }
    public void SetUiAllEqParts(ICharData data, string backUpKey, Dictionary<string, GameObject> mGameObj)
    {
        var eq = data.EqSlot;
        string[] all = new string[] { "BaseHand1A", "BaseHand1B", "BaseHand2", "BaseBoth",
            "EqBody", "EqHand1A", "EqHand1B", "EqHand2", "EqBoth", "OneWp1", "OneWp2", "OneWp3", "TwoWp1", "TwoWp2", "TwoWp3"};
        foreach (var v in all)
            mGameObj[v].SetActive(false);

        if (eq["Armor"] != null)
        {
            string eqStr = eq["Armor"].ItemId.ToString();
            mGameObj["EqBody"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_body");
            mGameObj["EqBody"].SetActive(true);
            if (backUpKey != eqStr + "_body")
            {
                if (eq["Armor"].ItemId > 10500)
                {
                    mGameObj["EqHand2"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand2");
                    mGameObj["EqHand2"].SetActive(true);
                    mGameObj["BaseHand2"].SetActive(true);
                    mGameObj["BaseHand1A"].SetActive(true);
                    return;
                }
                else
                {
                    mGameObj["EqHand1A"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1A");
                    mGameObj["EqHand1B"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1B");
                    mGameObj["EqHand2"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand2");
                    mGameObj["EqBoth"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_both");
                }
            }
        }
        List<string> parts = GetHandParts(eq);
        foreach (var v in parts)
            mGameObj[v].SetActive(true);
        if (eq["Hand1"] != null)
        {
            switch (eq["Hand1"].Hand)
            {
                case 0:
                    mGameObj["OneWp1"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                    mGameObj["OneWp1"].SetActive(true);
                    break;
                case 1:
                    switch (eq["Hand1"].Type)
                    {
                        case 12:
                            mGameObj["TwoWp1"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                            mGameObj["TwoWp1"].SetActive(true);
                            break;
                        case 14:
                        case 16:
                            mGameObj["TwoWp3"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                            mGameObj["TwoWp3"].SetActive(true);
                            break;
                        case 19:
                            mGameObj["TwoWp2"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                            mGameObj["TwoWp2"].SetActive(true);
                            break;
                    }
                    break;
                case 2:
                    mGameObj["OneWp3"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                    mGameObj["OneWp3"].SetActive(true);
                    break;
            }
        }
        if (eq["Hand2"] != null)
        {
            if (eq["Hand2"].Hand == 3) return;
            mGameObj["OneWp2"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand2"].ItemId.ToString());
            mGameObj["OneWp2"].SetActive(true);
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
            if (eq["Hand1"] != null)
            {
                switch (eq["Hand1"].Hand)
                {
                    case 0:
                    case 2:
                        parts = hasArmor ? new List<string> { "BaseHand1B", "BaseHand2", "EqHand1B", "EqHand2" } : new List<string> { "BaseHand1B", "BaseHand2" };
                        break;
                    case 1:
                        switch (eq["Hand1"].Type)
                        {
                            case 12:
                            case 14:
                            case 16:
                                parts = hasArmor ? new List<string> { "BaseBoth", "EqBoth" } : new List<string> { "BaseBoth" };
                                break; //양손 무기
                            case 19:
                                parts = hasArmor ? new List<string> { "BaseHand1A", "BaseHand2", "EqHand1A", "EqHand2" } : new List<string> { "BaseHand1A", "BaseHand2" };
                                break; //창
                        }
                        break;
                }
            }
            if (eq["Hand2"] != null && parts.Count == 0)
                parts = hasArmor ? new List<string> { "BaseHand1A", "BaseHand2", "EqHand1A", "EqHand2" } : new List<string> { "BaseHand1A", "BaseHand2" };
        }
        return parts;
    }

    HashSet<PtType> noneWpTypes = new HashSet<PtType>
    {
        PtType.BaseHand1B,PtType.BaseBoth, PtType.EqBoth, PtType.EqHand1B,
        PtType.OneWp1, PtType.OneWp2, PtType.OneWp3, PtType.TwoWp1, PtType.TwoWp2, PtType.TwoWp3
    };
    HashSet<PtType> hairTypes = new HashSet<PtType>
    {
        PtType.FrontHair1, PtType.FrontHair2, PtType.BackHair
    };
    HashSet<PtType> skinTypes = new HashSet<PtType>
    {
        PtType.Face, PtType.Ear, PtType.BaseBody, PtType.BaseHand1A,
        PtType.BaseHand1B, PtType.BaseHand2, PtType.BaseBoth
    };
    public void SetObjParts(Dictionary<PtType, SpriteRenderer> ptSpr, GameObject ptMain, bool noneWp = false)
    {
        foreach (PtType PtType in Enum.GetValues(typeof(PtType)))
        {
            if (noneWp && noneWpTypes.Contains(PtType)) continue;
            string partName = PtType.ToString();
            ptSpr[PtType] = ptMain.transform.Find(partName).GetComponent<SpriteRenderer>();
        }
    }
    public void SetObjAppearance(int uid, Dictionary<PtType, SpriteRenderer> ptSpr, bool noneWp = false)
    {
        ICharData data = uid == 0 ? PlayerManager.I.pData : NpcManager.I.NpcDataList[uid]; //uid가 0이면 무조건 플레이어 1부터는 NPC
        ptSpr[PtType.Face].sprite = ResManager.GetSprite($"Face_{data.Face}");
        ptSpr[PtType.Eyebrow].sprite = ResManager.GetSprite($"Eyebrow_{data.Eyebrow}");
        ptSpr[PtType.Eye1].sprite = ResManager.GetSprite($"Eye_{data.Eye}_1");
        ptSpr[PtType.Eye2].sprite = ResManager.GetSprite($"Eye_{data.Eye}_2");
        ptSpr[PtType.Ear].sprite = ResManager.GetSprite($"Ear_{data.Ear}");
        ptSpr[PtType.Nose].sprite = ResManager.GetSprite($"Nose_{data.Nose}");
        ptSpr[PtType.Mouth].sprite = ResManager.GetSprite($"Mouth_{data.Mouth}");

        Color skinColor = ColorData.GetSkinColor(data.Skin);
        Color hairColor = ColorData.GetHairColor(data.HairColor);
        Color eyeColor = ColorData.GetEyeColor(data.EyeColor);

        foreach (PtType PtType in Enum.GetValues(typeof(PtType)))
        {
            if (noneWp && noneWpTypes.Contains(PtType)) continue;
            if (skinTypes.Contains(PtType))
                ptSpr[PtType].color = skinColor;
            else if (hairTypes.Contains(PtType))
            {
                ptSpr[PtType].color = hairColor;
                ptSpr[PtType].gameObject.SetActive(false);
            }
        }
        switch (data.Hair)
        {
            case 1:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(true);
                ptSpr[PtType.BackHair].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair1].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                ptSpr[PtType.BackHair].sprite = ResManager.GetSprite("Hair_2_" + data.Hair);
                break;
            case 2:
            case 3:
                ptSpr[PtType.FrontHair1].gameObject.SetActive(true);
                ptSpr[PtType.BackHair].gameObject.SetActive(false);
                ptSpr[PtType.FrontHair1].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                break;
            case 100:
                ptSpr[PtType.FrontHair2].gameObject.SetActive(true);
                ptSpr[PtType.BackHair].gameObject.SetActive(true);
                ptSpr[PtType.FrontHair2].sprite = ResManager.GetSprite("Hair_1_" + data.Hair);
                ptSpr[PtType.BackHair].sprite = ResManager.GetSprite("Hair_2_" + data.Hair);
                break;
        }
        ptSpr[PtType.Eye2].color = eyeColor;
    }
    public void SetObjBodyEqParts(int uid, Dictionary<PtType, SpriteRenderer> ptSpr)
    {
        ICharData data = uid == 0 ? PlayerManager.I.pData : NpcManager.I.NpcDataList[uid];
        if (data.EqSlot["Armor"] != null)
        {
            ptSpr[PtType.EqBody].sprite = ResManager.GetSprite(data.EqSlot["Armor"].ItemId.ToString() + "_body");
            ptSpr[PtType.EqHand1A].sprite = ResManager.GetSprite(data.EqSlot["Armor"].ItemId.ToString() + "_hand1A");
            ptSpr[PtType.EqHand2].sprite = ResManager.GetSprite(data.EqSlot["Armor"].ItemId.ToString() + "_hand2");
        }
    }
    public void SetObjAllEqParts(int uid, Dictionary<PtType, SpriteRenderer> ptSpr)
    {
        PtType[] bodyParts = new PtType[] {
            PtType.BaseHand1A, PtType.BaseHand1B, PtType.BaseHand2, PtType.BaseBoth,
            PtType.EqBody, PtType.EqHand1A, PtType.EqHand1B, PtType.EqHand2, PtType.EqBoth,
            PtType.OneWp1, PtType.OneWp2, PtType.OneWp3, PtType.TwoWp1, PtType.TwoWp2, PtType.TwoWp3
        };

        foreach (PtType b in bodyParts) ptSpr[b].gameObject.SetActive(false);

        var slot = uid == 0 ? PlayerManager.I.pData.EqSlot : NpcManager.I.NpcDataList[uid].EqSlot;
        int wpState = -1; //0: 맨손. 1 : 손1에 한손 착용. 2 : 손2에 한손 착용. 3 : 손1,2 각각 한손 착용. 4 : 양손검,도끼,둔기. 5 : 창, 지팡이
        //손1 체크
        if (slot["Hand1"] != null)
        {
            Debug.Log("Hand1: " + slot["Hand1"].Hand);
            //손1 착용 상태
            int hand1 = slot["Hand1"].Hand;
            switch (hand1)
            {
                case 0:
                    wpState = 1;
                    ptSpr[PtType.OneWp1].gameObject.SetActive(true);
                    ptSpr[PtType.OneWp1].sprite = ResManager.GetSprite("wp" + slot["Hand1"].ItemId.ToString());
                    break; //손1에 한손 착용
                case 1:
                    switch (slot["Hand1"].Type)
                    {
                        case 12:
                        case 14:
                        case 16:
                            wpState = 4;
                            PtType curWp = slot["Hand1"].Type == 2 ? PtType.TwoWp1 : PtType.TwoWp3;
                            ptSpr[curWp].gameObject.SetActive(true);
                            ptSpr[curWp].sprite = ResManager.GetSprite("wp" + slot["Hand1"].ItemId.ToString());
                            break;
                        case 19:
                            wpState = 5;
                            ptSpr[PtType.TwoWp2].gameObject.SetActive(true);
                            ptSpr[PtType.TwoWp2].sprite = ResManager.GetSprite("wp" + slot["Hand1"].ItemId.ToString());
                            break;
                    }
                    break; //양손무기, 창
                case 2:
                    wpState = 3;
                    ptSpr[PtType.OneWp3].gameObject.SetActive(true);
                    ptSpr[PtType.OneWp3].sprite = ResManager.GetSprite("wp" + slot["Hand1"].ItemId.ToString());
                    break;
            }
        }
        //손1에 양손무기가 있을경우 스킵하기 위한 조건문
        if (wpState < 2)
        {
            if (slot["Hand2"] != null) //손2 체크
            {
                if (wpState == 1)
                    wpState = 3; //손1,2 각각 한손 착용
                else
                    wpState = 2; //손2에 한손 착용
                ptSpr[PtType.OneWp2].gameObject.SetActive(true);
                ptSpr[PtType.OneWp2].sprite = ResManager.GetSprite("wp" + slot["Hand2"].ItemId.ToString());
            }
        }
        //0: 맨손. 1 : 손1에 한손 착용. 2 : 손2에 한손 착용. 3 : 손1,2 각각 한손 착용. 4 : 양손검,도끼,둔기. 5 : 창, 지팡이
        PtType[] arr = null;
        bool hasArmor = slot["Armor"] != null;
        switch (wpState)
        {
            case 0: case 2: arr = hasArmor ? new PtType[] { PtType.BaseHand1A, PtType.BaseHand2, PtType.EqHand1A, PtType.EqHand2 } : new PtType[] { PtType.BaseHand1A, PtType.BaseHand2 }; break;
            case 1: case 3: arr = hasArmor ? new PtType[] { PtType.BaseHand1B, PtType.BaseHand2, PtType.EqHand1B, PtType.EqHand2 } : new PtType[] { PtType.BaseHand1B, PtType.BaseHand2 }; break;
            case 4: arr = hasArmor ? new PtType[] { PtType.BaseBoth, PtType.EqBoth } : new PtType[] { PtType.BaseBoth }; break;
            case 5: arr = hasArmor ? new PtType[] { PtType.BaseHand1A, PtType.BaseHand2, PtType.EqHand1A, PtType.EqHand2 } : new PtType[] { PtType.BaseHand1A, PtType.BaseHand2 }; break;
        }
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
                ptSpr[arr[i]].gameObject.SetActive(true);
        }
        if (hasArmor)
        {
            ptSpr[PtType.EqBody].sprite = ResManager.GetSprite(slot["Armor"].ItemId.ToString() + "_body");
            ptSpr[PtType.EqHand1A].sprite = ResManager.GetSprite(slot["Armor"].ItemId.ToString() + "_hand1A");
            ptSpr[PtType.EqHand1B].sprite = ResManager.GetSprite(slot["Armor"].ItemId.ToString() + "_hand1B");
            ptSpr[PtType.EqHand2].sprite = ResManager.GetSprite(slot["Armor"].ItemId.ToString() + "_hand2");
            ptSpr[PtType.EqBoth].sprite = ResManager.GetSprite(slot["Armor"].ItemId.ToString() + "_both");
            ptSpr[PtType.EqBody].gameObject.SetActive(true);
        }
    }
    #endregion

    #region 스탯 관련
    public int GetWpRng(int wpType)
    {
        switch (wpType)
        {
            case 19:
                return 2;
            case 20:
                return 5;
            default:
                return 1;
        }
    }
    #endregion
    #region 특성 관리
    private AttTable _attTable;
    public AttTable AttTable => _attTable ?? (_attTable = GameDataManager.GetTable<AttTable>());
    public Dictionary<int, AttData> AttDataList = new Dictionary<int, AttData>();

    private void LoadAttData()
    {
        foreach (var att in AttTable.Datas)
        {
            AttDataList[att.AttID] = new AttData(att.AttID, att.Name);
        }
    }
    public string GetAttName(int attId)
    {
        return AttDataList[attId].Name;
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
            string[] att = sk.Att.Split('/');
            List<SkAttData> attList = new List<SkAttData>();
            foreach (var v in att)
            {
                SkAttData attData = new SkAttData(v);
                attList.Add(attData);
            }
            SkDataList[sk.SkID] = new SkData { SkId = sk.SkID, SkType = sk.Type, UseType = sk.Use, Cool = sk.Cool, Name = sk.Name, Att = attList };
        }
    }
    public int GetSkNextExp(int lv)
    {
        int[] limitExp = new int[] { 0, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000,
                                    1500, 2000, 2500, 3000, 3500, 4000, 4500, 5000, 6000, 8000};
        if (lv < 10)
            return limitExp[lv];
        else
            return lv * 5000;
    }
    // var baseSk = GsManager.SkDataList[skId];
    // var npcSk = baseSk.Clone(); // 또는 new SkData(baseSk);
    #endregion

    #region 제작 관리
    private MakeTable _makeTable;
    public MakeTable MakeTable => _makeTable ?? (_makeTable = GameDataManager.GetTable<MakeTable>());
    public Dictionary<int, MakeData> MakeDataList = new Dictionary<int, MakeData>();
    private void LoadMakeData()
    {
        foreach (var make in MakeTable.Datas)
        {
            MakeDataList[make.MakeId] = new MakeData(make.MakeId, make.ShopType, make.ItemId, make.Cnt, make.SkId, make.SkLv, make.Val, make.Itv, make.Recipe);
        }
    }
    public List<int> SortMakeList(List<int> mList)
    {
        List<int> sortedList = mList.OrderBy(x => MakeDataList[x].SkId).ToList();
        return sortedList;
    }
    #endregion
    #region 전투 관련
    public int GetDamage(int att, int def)
    {
        if (att + def <= 0) return 1;
        float result = (float)att * att / (att + def);
        float ran = Random.Range(0.9f, 1.1f);
        return (int)(result * ran);
    }
    #endregion
}
