using System.Collections.Generic;
using System;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;

[System.Serializable]
public enum PtType
{
    Face, Eyebrow, Eye, Ear, Nose, Mouth,
    BaseBody, BaseHand1A, BaseHand1A2, BaseHand1B, BaseHand2, BaseBoth,
    FrontHair1, FrontHair2, BackHair,
    EqBody, EqHand1A, EqHand1B, EqHand2, EqBoth,
    OneWp1, OneWp2, TwoWp1, TwoWp2, TwoWp3
}

[System.Serializable]
public class PlayerData
{
    // 기본 정보
    public string Name = "이름";
    public int Age, Gender; // 0: 남성, 1: 여성
    public long Crown; // 게임 재화
    // 레벨 및 경험치
    public int Lv, Exp, NextExp;
    // 상태
    public int HP, MP, SP, AddHP, AddMP, AddSP, MaxHP, MaxMP, MaxSP;
    public int Att, Def;
    public int Crt, CrtRate, Hit, Eva; // 치명타율, 치명타확률, 명중, 회피
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public Dictionary<string, ItemData> EqSlot = new Dictionary<string, ItemData>();
    public List<ItemData> Inven = new List<ItemData>();
    //외형 데이터
    public int Skin, Face, Eyebrow, Eye, EyeColor, Ear, Nose, Mouth, Hair, HairColor;
    public PlayerData()
    {
        EqSlot.Add("Hand1", null); EqSlot.Add("Hand2", null); EqSlot.Add("Armor", null); EqSlot.Add("Shoes", null); EqSlot.Add("Helmet", null); EqSlot.Add("Gloves", null); EqSlot.Add("Belt", null); EqSlot.Add("Cape", null); EqSlot.Add("Necklace", null); EqSlot.Add("Ring1", null); EqSlot.Add("Ring2", null);
    } // 생성자
}
[System.Serializable]
public class NpcData
{
    public int NpcId, Lv, Exp, NextExp, Hp, Mp, Sp, MaxHP, MaxMP, MaxSP;
    public int Att, Def, Crt, CrtRate, Hit, Eva;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
}
[System.Serializable]
public class MonData
{
    public int MonId, MonType, Lv, Exp, NextExp, HP, MP, SP, MaxHP, MaxMP, MaxSP;
    public string Name;
    public int Att, Def, Crt, CrtRate, Hit, Eva;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public float OffY, SdwScr, SdwY; // 몬스터 오브젝트 Y 좌표, 몬스터 그림자 스케일, 몬스터 그림자 Y 좌표
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
            Att = this.Att,
            Def = this.Def,
            Crt = this.Crt,
            CrtRate = this.CrtRate,
            Hit = this.Hit,
            Eva = this.Eva,
            Lv = this.Lv,
            Exp = this.Exp,
            NextExp = this.NextExp,
            HP = this.HP,
            MP = this.MP,
            SP = this.SP,
            MaxHP = this.MaxHP,
            MaxMP = this.MaxMP,
            MaxSP = this.MaxSP,
            OffY = this.OffY,
            SdwScr = this.SdwScr,
            SdwY = this.SdwY
        };
    }
}

[System.Serializable]
public class CityData
{
    public int CityId;
    public string Name, Place;
    public CityData(int id, string name, string place)
    {
        this.CityId = id;
        this.Name = name;
        this.Place = place;
    }
}

[System.Serializable]
public class ShopData
{
    public int Id, CityId, Type; // 상점ID, 도시ID, 상점타입
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

//외형 색상
public static class CharColor
{
    // 스킨 컬러
    private static readonly Dictionary<int, Color> skinColor = new Dictionary<int, Color>()
    {
        { 1, new Color(255f/255f, 235f/255f, 210f/255f) },
        { 2, new Color(236f/255f, 204f/255f, 169f/255f) },
        { 3, new Color(251f/255f, 206f/255f, 177f/255f) },
        { 4, new Color(199f/255f, 165f/255f, 137f/255f) },
        { 5, new Color(92f/255f, 73f/255f, 57f/255f) }
    };
    // 헤어 컬러
    private static readonly Dictionary<int, Color> hairColor = new Dictionary<int, Color>()
    {
        { 1, new Color(30f/255f, 30f/255f, 30f/255f) },    // 어두운 회색
        { 2, new Color(57f/255f, 41f/255f, 28f/255f) },    // 어두운 갈색
        { 3, new Color(104f/255f, 69f/255f, 35f/255f) },   // 중간 갈색
        { 4, new Color(138f/255f, 97f/255f, 69f/255f) },   // 밝은 갈색
        { 5, new Color(219f/255f, 169f/255f, 93f/255f) },  // 금발
        { 6, new Color(245f/255f, 238f/255f, 216f/255f) }, // 플래티넘 블론드
        { 7, new Color(170f/255f, 255f/255f, 216f/255f) }  // 민트색 (특이한 색상)
    };

    public static Color GetSkinColor(int skinId)
    {
        return skinColor.TryGetValue(skinId, out Color color) ? color : new Color(236f / 255f, 204f / 255f, 169f / 255f);
    }

    public static Color GetHairColor(int hairId)
    {
        return hairColor.TryGetValue(hairId, out Color color) ? color : new Color(30f / 255f, 30f / 255f, 30f / 255f);
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