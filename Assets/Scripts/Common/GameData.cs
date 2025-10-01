using System.Collections.Generic;
using System;
using UnityEngine;
using GB;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public enum PtType
{
    Face, Eyebrow, Eye1, Eye2, Ear, Nose, Mouth,
    BaseBody, BaseHand1A, BaseHand1A2, BaseHand1B, BaseHand2, BaseBoth,
    FrontHair1, FrontHair2, BackHair,
    EqBody, EqHand1A, EqHand1B, EqHand2, EqBoth,
    OneWp1, OneWp2, TwoWp1, TwoWp2, TwoWp3
}
public interface ICharData
{
    int Skin { get; set; }
    int Face { get; set; }
    int Eyebrow { get; set; }
    int Eye { get; set; }
    int EyeColor { get; set; }
    int Ear { get; set; }
    int Nose { get; set; }
    int Mouth { get; set; }
    int Hair { get; set; }
    int HairColor { get; set; }
    int Gen { get; set; }
    Dictionary<string, ItemData> EqSlot { get; set; }
}
[System.Serializable]
public class PlayerData : ICharData
{
    // 기본 정보
    public string Name = "이름";
    public int Age;
    public long Crown; // 게임 재화
    // 레벨 및 경험치
    public int Lv, Exp, NextExp, GainExp, Grade, GradeExp, GradeNext;
    // 상태
    public int HP, MP, SP, AddHP, AddMP, AddSP, MaxHP, MaxMP, MaxSP;
    public int Att, Def;
    public int Crt, CrtRate, Hit, Eva; // 치명타율, 치명타확률, 명중, 회피
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public Dictionary<string, ItemData> EqSlot { get; set; } = new Dictionary<string, ItemData>();
    public List<ItemData> Inven = new List<ItemData>();
    #region ICharData
    public int Gen { get; set; }
    public int Skin { get; set; }
    public int Face { get; set; }
    public int Eyebrow { get; set; }
    public int Eye { get; set; }
    public int EyeColor { get; set; }
    public int Ear { get; set; }
    public int Nose { get; set; }
    public int Mouth { get; set; }
    public int Hair { get; set; }
    public int HairColor { get; set; }
    #endregion
    public PlayerData()
    {
        EqSlot.Add("Hand1", null); EqSlot.Add("Hand2", null); EqSlot.Add("Armor", null); EqSlot.Add("Shoes", null); EqSlot.Add("Helmet", null);
        EqSlot.Add("Gloves", null); EqSlot.Add("Belt", null); EqSlot.Add("Cape", null); EqSlot.Add("Necklace", null); EqSlot.Add("Ring1", null); EqSlot.Add("Ring2", null);

    } // 생성자
}
[System.Serializable]
public class NpcData : ICharData
{
    public string Name;
    public int Age, Fame, Rls; //Relationship
    public int NpcId, Lv, Exp, NextExp, GainExp, Grade, GradeExp, GradeNext;
    public int HP, MP, SP, AddHP, AddMP, AddSP, MaxHP, MaxMP, MaxSP;
    public int Att, Def, Crt, CrtRate, Hit, Eva;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public Dictionary<string, ItemData> EqSlot { get; set; } = new Dictionary<string, ItemData>();
    #region ICharData
    public int Gen { get; set; }
    public int Skin { get; set; }
    public int Face { get; set; }
    public int Eyebrow { get; set; }
    public int Eye { get; set; }
    public int EyeColor { get; set; }
    public int Ear { get; set; }
    public int Nose { get; set; }
    public int Mouth { get; set; }
    public int Hair { get; set; }
    public int HairColor { get; set; }
    #endregion
}

[System.Serializable]
public class MonData
{
    public int MonId, MonType, Lv, Exp, NextExp, GainExp, HP, MP, SP, MaxHP, MaxMP, MaxSP;
    public string Name;
    public int Att, Def, Crt, CrtRate, Hit, Eva;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public int W, H; // 몬스터 오브젝트 크기
    public float SdwScr, GgY; // 몬스터 그림자 스케일, 몬스터 그림자 Y 좌표
    public MonData Clone()
    {
        return new MonData
        {
            MonId = this.MonId,
            MonType = this.MonType,
            Name = this.Name,
            VIT = this.VIT,
            END = this.END,
            STR = this.STR,
            AGI = this.AGI,
            FOR = this.FOR,
            INT = this.INT,
            CHA = this.CHA,
            LUK = this.LUK,

            // HP/MP/SP 관련 (중요!)
            HP = this.HP,
            MP = this.MP,
            SP = this.SP,
            MaxHP = this.MaxHP,
            MaxMP = this.MaxMP,
            MaxSP = this.MaxSP,

            // 전투 스탯
            Att = this.Att,
            Def = this.Def,
            Crt = this.Crt,
            CrtRate = this.CrtRate,
            Hit = this.Hit,
            Eva = this.Eva,

            // 레벨 관련
            Lv = this.Lv,
            Exp = this.Exp,
            NextExp = this.NextExp,
            GainExp = this.GainExp,

            // 표시 관련
            W = this.W,
            H = this.H,
            SdwScr = this.SdwScr,
            GgY = this.GgY
        };
    }
}

[System.Serializable]
public class CityData
{
    public int CityId;
    public string Name, Place;
    public int[] Near;
    public CityData(int id, string name, string place, string str)
    {
        this.CityId = id;
        this.Name = name;
        this.Place = place;
        Near = str.Split('_').Select(int.Parse).ToArray();
    }
}

[System.Serializable]
public class ShopData
{
    public int Id, CityId, Type, NpcId; // 상점ID, 도시ID, 상점타입, 상점주인
    public List<ShopItemData> items; // 상점아이템
    public ShopData()
    {
        items = new List<ShopItemData>();
    }
}

[System.Serializable]
public class ShopItemData
{
    public int ItemId, Type;
    public ShopItemData(int itemId, int type)
    {
        this.ItemId = itemId;
        this.Type = type;
    }
}

[System.Serializable]
public class ItemData
{
    public string Name, Res;
    public int ItemId, Type, Price, Val, W, H, Dur, X, Y;
    public int Dir, Uid; //dir: 0은 세로 1은 가로 모든 장비,무기,아이템은 디폴트가 0
    public int Both; // 0: 한손무기, 1: 양손무기 // 무기에만 적용
    public ItemData Clone()
    {
        return new ItemData
        {
            Name = this.Name,
            Res = this.Res,
            ItemId = this.ItemId,
            Type = this.Type,
            Price = this.Price,
            Val = this.Val,
            W = this.W,
            H = this.H,
            Dur = this.Dur,
            X = this.X,
            Y = this.Y,
            Dir = this.Dir
        };
    }
}

//퀘스트
[System.Serializable]
public class QuestData
{
    public int QuestID, Days;
    public string Name;
    public QuestData(int id, string name, int days)
    {
        this.QuestID = id;
        this.Name = name;
        this.Days = days;
    }
}

//길드에서 제시하는 퀘스트용 데이터 클래스
[System.Serializable]
public class QuestInstData
{
    //QSCid 는 퀘스트를 수락하여 시작한 도시 ID
    public int Qid, QSCid, Days, Star;
    public int Exp, Crown, GradeExp;
    public string Desc;
    public int Cnt, MonId, ItemId, CityId, RoadId, NpcGrpId;
    public QuestInstData(int qid, int qscid)
    {
        this.Qid = qid;
        this.QSCid = qscid;
    }
    public void SetQuestBase(string desc, int days, int star, int exp, int crown, int gradeExp)
    {
        this.Desc = desc;
        this.Days = days;
        this.Star = star;
        this.Exp = exp;
        this.Crown = crown;
        this.GradeExp = gradeExp;
    }
}

//색상 클래스
[System.Serializable]
public static class ColorData
{
    // 스킨 컬러
    private static readonly Dictionary<int, Color> skinColor = new Dictionary<int, Color>()
    {
        { 1, new Color(255f/255f, 235f/255f, 210f/255f) }, // 아이보리 베이지
        { 2, new Color(255f/255f, 220f/255f, 190f/255f) }, // 피치 베이지
        { 3, new Color(250f/255f, 210f/255f, 185f/255f) }, // 라이트 핑크 베이지
        { 4, new Color(240f/255f, 200f/255f, 165f/255f) }, // 웜 베이지
        { 5, new Color(235f/255f, 205f/255f, 170f/255f) }, // 피치 베이지/라이트 탠 ⭐
        { 6, new Color(220f/255f, 180f/255f, 145f/255f) }, // 내추럴 탠
        { 7, new Color(184f/255f, 120f/255f, 82f/255f) },  // 미디엄 브라운
        { 8, new Color(153f/255f, 99f/255f, 71f/255f) },   // 딥 브라운
        { 9, new Color(92f/255f, 58f/255f, 37f/255f) },    // 다크 초콜릿
        { 10, new Color(62f/255f, 39f/255f, 32f/255f) },   // 쿨 다크 브라운
    };
    // 헤어 컬러
    private static readonly Dictionary<int, Color> hairColor = new Dictionary<int, Color>()
    {
        // 현실 계열 (1~14)
        { 1,  new Color(28f/255f, 28f/255f, 28f/255f) },     // 블랙
        { 2,  new Color(45f/255f, 35f/255f, 35f/255f) },     // 소프트 블랙
        { 3,  new Color(60f/255f, 40f/255f, 30f/255f) },     // 다크 브라운
        { 4,  new Color(92f/255f, 58f/255f, 37f/255f) },     // 초콜릿 브라운
        { 5,  new Color(120f/255f, 72f/255f, 50f/255f) },    // 체스트넛
        { 6,  new Color(150f/255f, 100f/255f, 70f/255f) },   // 라이트 브라운
        { 7,  new Color(181f/255f, 120f/255f, 82f/255f) },   // 골든 브라운
        { 8,  new Color(201f/255f, 160f/255f, 95f/255f) },   // 다크 블론드
        { 9,  new Color(220f/255f, 180f/255f, 110f/255f) },  // 허니 블론드
        { 10, new Color(220f/255f, 170f/255f, 95f/255f) },   // 리치 골드 ⭐
        { 11, new Color(230f/255f, 210f/255f, 170f/255f) },  // 애쉬 블론드
        { 12, new Color(240f/255f, 225f/255f, 190f/255f) },  // 플래티넘 블론드
        { 13, new Color(110f/255f, 40f/255f, 40f/255f) },    // 다크 레드(마호가니)
        { 14, new Color(170f/255f, 75f/255f, 45f/255f) },    // 코퍼 레드
        { 15, new Color(235f/255f, 235f/255f, 235f/255f) },  // 화이트/실버

        // 판타지 계열 (16~25)
        { 16, new Color(138f/255f, 40f/255f, 40f/255f) },    // 딥 레드
        { 17, new Color(180f/255f, 60f/255f, 70f/255f) },    // 루비 레드
        { 18, new Color(204f/255f, 112f/255f, 50f/255f) },   // 딥 오렌지
        { 19, new Color(200f/255f, 120f/255f, 140f/255f) },  // 로즈 핑크
        { 20, new Color(235f/255f, 180f/255f, 200f/255f) },  // 파스텔 핑크
        { 21, new Color(90f/255f, 60f/255f, 110f/255f) },    // 다크 퍼플
        { 22, new Color(150f/255f, 120f/255f, 180f/255f) },  // 라벤더 퍼플
        { 23, new Color(40f/255f, 60f/255f, 120f/255f) },    // 네이비 블루
        { 24, new Color(100f/255f, 130f/255f, 170f/255f) },  // 애쉬 블루
        { 25, new Color(45f/255f, 90f/255f, 60f/255f) },     // 딥 그린
        { 26, new Color(170f/255f, 255f/255f, 216f/255f) },  // 민트 그린 ⭐
        { 27, new Color(150f/255f, 200f/255f, 235f/255f) },  // 스카이 블루
        { 28, new Color(180f/255f, 240f/255f, 200f/255f) },  // 파스텔 민트
        { 29, new Color(230f/255f, 210f/255f, 120f/255f) },  // 샌드 옐로우
        { 30, new Color(200f/255f, 170f/255f, 230f/255f) },  // 라일락 퍼플
    };

    private static readonly Dictionary<int, Color> eyeColor = new Dictionary<int, Color>()
    {
        // ===== Eyes: Reality (1–12) =====
        { 1,  new Color(50f/255f,  50f/255f,  50f/255f) },   // 뉴트럴 그레이 (요청값)
        { 2,  new Color(80f/255f,  55f/255f,  40f/255f) },   // 미디엄 브라운
        { 3,  new Color(105f/255f, 75f/255f,  50f/255f) },   // 라이트 브라운(헤이즐 기반)
        { 4,  new Color(120f/255f, 95f/255f,  55f/255f) },   // 엠버(골드기)
        { 5,  new Color(75f/255f,  100f/255f, 80f/255f) },   // 올리브 그린(저채도)
        { 6,  new Color(95f/255f,  135f/255f, 105f/255f) },  // 포레스트 그린
        { 7,  new Color(90f/255f,  120f/255f, 150f/255f) },  // 그레이시 블루
        { 8,  new Color(70f/255f,  105f/255f, 145f/255f) },  // 딥 블루
        { 9,  new Color(145f/255f, 165f/255f, 180f/255f) },  // 라이트 그레이
        {10,  new Color(180f/255f, 190f/255f, 200f/255f) },  // 실버 그레이
        {11,  new Color(135f/255f, 100f/255f, 60f/255f) },   // 웜 헤이즐(골드·브라운 믹스)
        {12,  new Color(85f/255f,  120f/255f, 125f/255f) },  // 슬레이트 틸(블루-그린 저채도)

        // ===== Eyes: Fantasy (13–20) =====
        {13,  new Color(210f/255f, 170f/255f, 60f/255f) },   // 골드 아이
        {14,  new Color(160f/255f, 90f/255f,  160f/255f) },  // 바이올렛
        {15,  new Color(150f/255f, 30f/255f,  40f/255f) },   // 루비 레드
        {16,  new Color(60f/255f,  90f/255f,  160f/255f) },  // 사파이어
        {17,  new Color(40f/255f,  110f/255f, 95f/255f) },   // 딥 틸
        {18,  new Color(180f/255f, 235f/255f, 255f/255f) },  // 아이스 블루(밝고 저채도)
        {19,  new Color(120f/255f, 200f/255f, 170f/255f) },  // 민트 그린
        {20,  new Color(235f/255f, 235f/255f, 245f/255f) },  // 펄 화이트
    };

    public static Color GetSkinColor(int skinId)
    {
        return skinColor.TryGetValue(skinId, out Color color) ? color : new Color(236f / 255f, 204f / 255f, 169f / 255f);
    }

    public static Color GetHairColor(int hairId)
    {
        return hairColor.TryGetValue(hairId, out Color color) ? color : new Color(30f / 255f, 30f / 255f, 30f / 255f);
    }

    public static Color GetEyeColor(int eyeId)
    {
        return eyeColor.TryGetValue(eyeId, out Color color) ? color : new Color(30f / 255f, 30f / 255f, 30f / 255f);
    }
    public static Color GetGradeColor(int grade)
    {
        switch (grade)
        {
            case 1: return new Color(180f / 255f, 110f / 255f, 60f / 255f);
            case 2: return new Color(60f / 60f, 60f / 255f, 60f / 255f);
            case 3: return new Color(230f / 255f, 230f / 255f, 230f / 255f);
            case 4: return new Color(150f / 255f, 150f / 255f, 150f / 255f);
            case 5: return new Color(215f / 255f, 140f / 255f, 110f / 255f);
            case 6: return new Color(225f / 255f, 225f / 255f, 225f / 255f);
            case 7: return new Color(255f / 255f, 210f / 255f, 110f / 255f);
            case 8: return new Color(220f / 255f, 245f / 255f, 255f / 255f);
            case 9: return new Color(85f / 255f, 140f / 255f, 200f / 255f);
            case 10: return new Color(30f / 255f, 30f / 255f, 30f / 255f);
        }
        return Color.white;
    }
}

//레벨, 경험치 관련 클래스
[System.Serializable]
public class LevelData : AutoSingleton<LevelData>
{
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
}

//오브젝트 레이어 클래스
[System.Serializable]
public class ObjLayerData : AutoSingleton<ObjLayerData>
{
    public int GetObjLayer(float y)
    {
        return (int)((80 - y) * 100);
    }
}

//인간형 오브젝트 외형 설정
[System.Serializable]
public class HumanAppearance : AutoSingleton<HumanAppearance>
{
    //Skin_Face_Eyebrow_Eye_EyeColor_Ear_Nose_Mouth_Hair_HairColor
    public void SetUiBaseParts(int id, Dictionary<string, GameObject> obj)
    {
        Dictionary<string, Image> img = new Dictionary<string, Image>();
        string[] str = { "Face", "Eyebrow", "Eye1", "Eye2", "Ear", "Nose", "Mouth", "BaseBody",
            "BaseHand1A", "BaseHand1A2", "BaseHand1B", "BaseHand2", "BaseBoth", "Hair1A", "Hair1B", "Hair2" };
        foreach (var v in str)
            img[v] = obj[v].GetComponent<Image>();

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
    public void SetUiEqParts(ICharData data, string backUpKey, Dictionary<string, GameObject> mGameObj)
    {
        var eq = data.EqSlot;
        string[] allParts = {"BaseHand1A", "BaseHand1A2", "BaseHand1B", "BaseHand2", "BaseBoth",
            "EqBody", "EqHand1A", "EqHand1B", "EqHand2", "EqBoth", "OneWp1", "OneWp2", "TwoWp1", "TwoWp2", "TwoWp3"};
        foreach (var v in allParts)
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
        string[] hKey = { "Hand1", "Hand2" };
        foreach (var v in hKey)
        {
            if (eq[v] != null)
            {
                switch (eq[v].Both)
                {
                    case 0:
                        string one = v == "Hand1" ? "OneWp1" : "OneWp2";
                        mGameObj[one].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq[v].ItemId.ToString());
                        mGameObj[one].SetActive(true);
                        break;
                    case 1:
                        mGameObj["TwoWp1"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                        mGameObj["TwoWp1"].SetActive(true);
                        break;
                    case 2:
                        mGameObj["TwoWp2"].GetComponent<Image>().sprite = ResManager.GetSprite("wp" + eq["Hand1"].ItemId.ToString());
                        mGameObj["TwoWp2"].SetActive(true);
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
}
// 통합 저장 데이터 클래스
[System.Serializable]
public class GameSaveData
{
    public PlayerData playerData;
    public GameSaveData()
    {
        playerData = new PlayerData();
    }
}