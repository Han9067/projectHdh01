using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Collections;

[System.Serializable]
public class PlayerData
{
    // 기본 정보
    public string Name = "유저";
    public int Age, Gender; // 0: 남성, 1: 여성
    public int Sylin; // 게임 재화
    // 레벨 및 경험치
    public int Lv, Exp, NextExp;
    // 상태
    public int HP, MP, SP;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    
    //1번손,2번손,갑옷,신발,투구,장갑,벨트,망토,목걸이,반지1,반지2,무기
    // 생성자
    public PlayerData() { }
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

public class ItemInfo
{
    public string Name, Res;
    public int itemId, Type, Price, Val, W, H, Dur, x, y;
    
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