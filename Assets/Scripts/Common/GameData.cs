using System.Collections.Generic;
using System;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;

[System.Serializable]
public class PlayerData
{
    // 기본 정보
    public string Name = "이름";
    public int Age, Gender; // 0: 남성, 1: 여성
    public long Silver; // 게임 재화
    // 레벨 및 경험치
    public int Lv, Exp, NextExp;
    // 상태
    public int HP, MP, SP, AddHP, AddMP, AddSP, MaxHP, MaxMP, MaxSP;
    public int Att, Def;
    public int Crt, CrtRate, Acc, Dod;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public Dictionary<string, ItemData> EqSlot = new Dictionary<string, ItemData>();
    public List<ItemData> Inven = new List<ItemData>();
    //외형 데이터
    public int Skin, Face, Eyebrow, Eye, EyeColor, Ear, Nose, Mouth, Hair, HairColor;
    public PlayerData() 
    {
        EqSlot.Add("Hand1", null);EqSlot.Add("Hand2", null);EqSlot.Add("Armor", null);EqSlot.Add("Shoes", null);EqSlot.Add("Helmet", null);EqSlot.Add("Gloves", null);EqSlot.Add("Belt", null);EqSlot.Add("Cape", null);EqSlot.Add("Necklace", null);EqSlot.Add("Ring1", null);EqSlot.Add("Ring2", null);
    } // 생성자
}
[System.Serializable]
public class NpcData
{
    public int NpcId, Lv, Exp, NextExp, Hp, Mp, Sp, MaxHP, MaxMP, MaxSP;
    public int Att, Def, Crt, CrtRate, Acc, Dod;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
}
[System.Serializable]
public class MonData
{
    public int MonId, Lv, Exp, NextExp, HP, MP, SP, MaxHP, MaxMP, MaxSP;
    public string Name;
    public int Att, Def, Crt, CrtRate, Acc, Dod;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public MonData Clone()
    {
        return new MonData { 
            MonId = this.MonId, Name = this.Name,
             VIT = this.VIT, END = this.END, STR = this.STR, AGI = this.AGI, FOR = this.FOR, 
             INT = this.INT, CHA = this.CHA, LUK = this.LUK,
             Att = this.Att, Def = this.Def, Crt = this.Crt, CrtRate = this.CrtRate, Acc = this.Acc, Dod = this.Dod,
             Lv = this.Lv, Exp = this.Exp, NextExp = this.NextExp, 
             HP = this.HP, MP = this.MP, SP = this.SP, MaxHP = this.MaxHP, MaxMP = this.MaxMP, MaxSP = this.MaxSP };
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
    public float H1AX, H1AY, H1BX, H1BY, H2X, H2Y, BX, BY; // 무기 좌표
    public ItemData Clone()
    {
        return new ItemData { 
            Name = this.Name, Res = this.Res, ItemId = this.ItemId, Type = this.Type, 
            Price = this.Price, Val = this.Val, W = this.W, H = this.H, 
            Dur = this.Dur, X = this.X, Y = this.Y, Dir = this.Dir};
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