using System.Collections.Generic;
using System;
using UnityEngine;
using GB;
using UnityEngine.UI;
using System.Linq;
using JetBrains.Annotations;

[System.Serializable]
public class InvenGrid
{
    public int x;
    public int y;
    public int slotId = -1; // -1이면 비어있음
}
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
public class AttData
{
    public int StatID;
    public string Name;
    public AttData(int id, string name)
    {
        this.StatID = id;
        this.Name = name;
    }
}
[System.Serializable]
public class SkData
{
    public int SkId, Type, Cool, Exp, NextExp, Lv;
    public string Name;
    public List<SkAttData> Att;
    public SkData Clone()
    {
        return new SkData
        {
            SkId = this.SkId,
            Type = this.Type,
            Cool = this.Cool,
            Name = this.Name,
            Exp = 0,
            NextExp = 0,
            Lv = 1,
            Att = this.Att,
        };
    }
}
[System.Serializable]
public class SkAttData
{
    //0 : AttID -> 특성ID, 1 : Val -> 스킬 내 특성 초기값, 2 : Lim -> 특성 활성 레벨, 3 : Itv -> 레벨별 특성값 증가치
    public int AttID, Val, Lim, Itv;
    public string Name, Str;
    public SkAttData(string att)
    {
        if (att == "0")
        {
            AttID = 0; Val = 0; Lim = 0; Itv = 0;
            return;
        }
        string[] attVal = att.Split('_');
        AttID = int.Parse(attVal[0]);
        Name = GsManager.I.GetAttName(AttID);
        Val = int.Parse(attVal[1]);
        Lim = int.Parse(attVal[2]);
        Itv = int.Parse(attVal[3]);
        Str = attVal[4];
    }
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
    public int QuestMax;
    public List<QuestInstData> QuestList;
    public Dictionary<string, ItemData> EqSlot { get; set; } = new Dictionary<string, ItemData>();
    public List<ItemData> Inven = new List<ItemData>();
    public Dictionary<int, SkData> SkList = new Dictionary<int, SkData>();
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
    public Dictionary<int, SkData> SkList = new Dictionary<int, SkData>();
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
    public class DropData
    {
        public int ItemId, Val;
        public float Rate;
        public DropData(int itemId, float rate, int val)
        {
            this.ItemId = itemId;
            this.Rate = rate;
            this.Val = val;
        }
    }
    public int MonId, MonType, Lv, Exp, NextExp, GainExp, HP, MP, SP, MaxHP, MaxMP, MaxSP;
    public string Name;
    public int Att, Def, Crt, CrtRate, Hit, Eva;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public int W, H; // 몬스터 오브젝트 크기
    public float SdwScr, GgY; // 몬스터 그림자 스케일, 몬스터 그림자 Y 좌표
    public Dictionary<int, SkData> SkList = new Dictionary<int, SkData>(); //추후 적용 251109
    public List<DropData> DropList = new List<DropData>();
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
            GgY = this.GgY,

            DropList = this.DropList
        };
    }
}

[System.Serializable]
public class CityData
{
    public int CityId;
    public string Name, Place;
    public List<int> Area;
    public Dictionary<string, List<Vector3>> Road;
    public CityData(int id, string name, string place, string str)
    {
        this.CityId = id;
        this.Name = name;
        this.Place = place;
        Area = str.Split('_').Select(int.Parse).ToList();
        Road = new Dictionary<string, List<Vector3>>();
    }
}

[System.Serializable]
public class MonGrpData
{
    //GrpID	Grade	Type	Min	Max	LeaderID	List
    public int GrpID, Grade, Type, Min, Max, LeaderID;
    public List<int> List;
    public MonGrpData(int grpID, int grade, int type, int min, int max, int leaderID, string list)
    {
        this.GrpID = grpID;
        this.Grade = grade;
        this.Type = type;
        this.Min = min;
        this.Max = max;
        this.LeaderID = leaderID;
        this.List = list.Split(',').Select(int.Parse).ToList();
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
    public int ItemId, Type, Price, W, H, Dur, MaxDur, X, Y, Grade;
    public int Dir, Uid; //dir: 0은 세로 1은 가로 모든 장비,무기,아이템은 디폴트가 0
    public int Both; // 0: 한손무기, 1: 양손무기 // 무기에만 적용
    public Dictionary<int, int> Att;
    public ItemData Clone()
    {
        return new ItemData
        {
            Name = this.Name,
            Res = this.Res,
            ItemId = this.ItemId,
            Type = this.Type,
            Price = this.Price,
            Att = this.Att,
            W = this.W,
            H = this.H,
            Dur = this.Dur,
            MaxDur = this.Dur,
            X = this.X,
            Y = this.Y,
            Dir = this.Dir,
            Grade = this.Grade
        };
    }
}

//퀘스트
[System.Serializable]
public class QuestData
{
    public int QuestID, Days, Type;
    public string Name;
    public QuestData(int id, string name, int days, int type)
    {
        this.QuestID = id;
        this.Name = name;
        this.Days = days;
        this.Type = type;
    }
}

//길드에서 제시하는 퀘스트용 데이터 클래스
[System.Serializable]
public class QuestInstData
{
    //Qid는 퀘스트 ID,QSCid는 퀘스트를 수락하여 시작한 도시 ID
    public int Qid, QSCid, QType, Days, Star;
    public int Exp, Crown, GradeExp;
    public int sDay, eDay; //퀘스트 시작일, 종료일
    public string Name, Desc;
    public int CurCnt, TgCnt, MonId, ItemId, CityId, RoadId, NpcGrpId;
    public int State; //0: 미수락, 1: 수락, 2: 완료
    public QuestInstData(int qid, int qscid, int qtype, string name)
    {
        this.Qid = qid;
        this.QSCid = qscid;
        this.QType = qtype;
        this.Name = name;
        this.State = 0;
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

    private static readonly Dictionary<int, Color> itemGradeColor = new Dictionary<int, Color>()
    {
        { 1,  new Color(197f/255f, 197f/255f, 197f/255f) }, // H급
        { 2,  new Color(230f/255f, 234f/255f, 240f/255f) }, // G급
        { 3,  new Color(96f/255f, 152f/255f, 72f/255f) }, // F급
        { 4,  new Color(54f/255f, 108f/255f, 191f/255f) }, // E급
        { 5,  new Color(125f/255f, 111f/255f, 176f/255f) }, // D급
        { 6,  new Color(215f/255f, 180f/255f, 183f/255f) }, // C급
        { 7,  new Color(229f/255f, 194f/255f, 96f/255f) }, // B급
        { 8,  new Color(221f/255f, 162f/255f, 96f/255f) }, // A급
        { 9,  new Color(186f/255f, 11f/255f, 6f/255f) }, // SS급
        { 10, new Color(70f/255f, 55f/255f, 55f/255f) }, // SSS급
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
    public static Color GetBadgeGradeColor(int grade)
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
    public static Color GetItemGradeColor(int grade)
    {
        return itemGradeColor.TryGetValue(grade, out Color color) ? color : new Color(197f / 255f, 197f / 255f, 197f / 255f);
    }
}