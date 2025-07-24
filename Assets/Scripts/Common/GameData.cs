using System.Collections.Generic;
using System;
using UnityEngine;

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
    //public int WpState; // 0: 무기 없음, 1: 손1에만 무기, 2: 손2에만 무기, 3: 손1,손2 각각 다른 무기 착용, 4: 양손
    public PlayerData() 
    {
        EqSlot.Add("Hand1", null);EqSlot.Add("Hand2", null);EqSlot.Add("Armor", null);EqSlot.Add("Shoes", null);EqSlot.Add("Helmet", null);EqSlot.Add("Gloves", null);EqSlot.Add("Belt", null);EqSlot.Add("Cape", null);EqSlot.Add("Necklace", null);EqSlot.Add("Ring1", null);EqSlot.Add("Ring2", null);
    } // 생성자
}

[System.Serializable]
public class ShopData
{
    public string name; // 상점이름
    public int id, cityId, type; // 상점ID, 도시ID, 상점타입
    public List<ShopItemData> items; // 상점아이템
    public ShopData()
    {
        items = new List<ShopItemData>();
    }
}

[System.Serializable]
public class ShopItemData
{
    public int itemId, type, cnt;
    public ShopItemData(int iId, int type, int cnt)
    {
        this.itemId = iId;
        this.type = type;
        this.cnt = cnt;
    }
}

[System.Serializable]
public class ItemData
{
    public string Name, Res, Path;
    public int ItemId, Type, Price, Val, W, H, Dur, X, Y, Dir, Uid;
    public int Both; // 0: 한손무기, 1: 양손무기 // 무기에만 적용
    //dir: 0은 세로 1은 가로 모든 장비,무기,아이템은 디폴트가 0
    public ItemData Clone()
    {
        return new ItemData { 
            Name = this.Name, Res = this.Res, Path = this.Path, ItemId = this.ItemId, Type = this.Type, 
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