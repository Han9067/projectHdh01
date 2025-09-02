using System.Collections.Generic;
using System;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using GB;
using Unity.VisualScripting;
using UnityEngine.UI;

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
    public int Age, Gen; // 0: 남성, 1: 여성
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
        EqSlot.Add("Hand1", null); EqSlot.Add("Hand2", null); EqSlot.Add("Armor", null); EqSlot.Add("Shoes", null); EqSlot.Add("Helmet", null);
        EqSlot.Add("Gloves", null); EqSlot.Add("Belt", null); EqSlot.Add("Cape", null); EqSlot.Add("Necklace", null); EqSlot.Add("Ring1", null); EqSlot.Add("Ring2", null);

    } // 생성자
}
[System.Serializable]
public class NpcData
{
    public string Name;
    public int Age, Gen, Fame, Rls; //Relationship
    public int NpcId, Lv, Exp, NextExp;
    public int HP, MP, SP, AddHP, AddMP, AddSP, MaxHP, MaxMP, MaxSP;
    public int Att, Def, Crt, CrtRate, Hit, Eva;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public Dictionary<string, ItemData> EqSlot = new Dictionary<string, ItemData>();
    public int Skin, Face, Eyebrow, Eye, EyeColor, Ear, Nose, Mouth, Hair, HairColor;
}

[System.Serializable]
public class MonData
{
    public int MonId, MonType, Lv, Exp, NextExp, HP, MP, SP, MaxHP, MaxMP, MaxSP;
    public string Name;
    public int Att, Def, Crt, CrtRate, Hit, Eva;
    public int VIT, END, STR, AGI, FOR, INT, CHA, LUK;
    public float W, H, OffX, OffY, SdwScr, SdwY; // 몬스터 오브젝트 크기, 몬스터 오브젝트 X,Y 좌표, 몬스터 그림자 스케일, 몬스터 그림자 Y 좌표
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

            Att = STR * 2,
            Def = VIT,
            Crt = 50 + (LUK * 2),
            CrtRate = LUK + AGI,
            Hit = 60 + (AGI / 4),
            Eva = 10 + (AGI / 4),

            Lv = 1,
            Exp = 0,
            NextExp = 1000,
            HP = VIT * 4,
            MP = INT * 4,
            SP = END * 4,
            MaxHP = HP,
            MaxMP = MP,
            MaxSP = SP,
            W = this.W,
            H = this.H,
            OffX = this.OffX,
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

//외형 색상
[System.Serializable]
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

[System.Serializable]
public class LevelData : AutoSingleton<LevelData>
{
    public int GetLv(int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK)
    {
        int total = (VIT + END + STR + AGI + FOR + INT + CHA + LUK) - 80;
        return total < 1 ? 1 : total;
    }
    public int GetNextExp(int Lv, double s = 30.0, double p = 2.8)
    {
        double C = 1000.0 / Math.Pow(1.0 + s, p);
        double raw = C * Math.Pow(Lv + s, p);
        return (int)(Math.Floor(raw / 10.0 + 0.5) * 10.0);
    }
}

//인간형 오브젝트 외형 설정
[System.Serializable]
public class HumanAppearance : AutoSingleton<HumanAppearance>
{
    //Skin_Face_Eyebrow_Eye_EyeColor_Ear_Nose_Mouth_Hair_HairColor
    public void SetUiBaseParts(PlayerData pData,
        Image Face, Image Eyebrow, Image Eye, Image Ear, Image Nose, Image Mouth,
        Image BaseBody, Image BaseHand1A, Image BaseHand1A2, Image BaseHand1B, Image BaseHand2, Image BaseBoth,
        Image Hair1A, Image Hair1B, Image Hair2)
    {
        Face.sprite = ResManager.GetSprite("Face_" + pData.Face);
        Eyebrow.sprite = ResManager.GetSprite("Eyebrow_" + pData.Eyebrow);
        Eye.sprite = ResManager.GetSprite("Eye_" + pData.Eye);
        Ear.sprite = ResManager.GetSprite("Ear_" + pData.Ear);
        Nose.sprite = ResManager.GetSprite("Nose_" + pData.Nose);
        Mouth.sprite = ResManager.GetSprite("Mouth_" + pData.Mouth);

        Color skinColor = CharColor.GetSkinColor(pData.Skin);
        Color hairColor = CharColor.GetHairColor(pData.HairColor);
        BaseBody.sprite = ResManager.GetSprite("Body" + pData.Gen);
        Face.color = skinColor; Ear.color = skinColor;
        BaseBody.color = skinColor; BaseHand1A.color = skinColor; BaseHand1A2.color = skinColor; BaseHand1B.color = skinColor;
        BaseHand2.color = skinColor; BaseBoth.color = skinColor;

        switch (pData.Hair)
        {
            case 1:
                Hair1A.gameObject.SetActive(true); Hair1B.gameObject.SetActive(false); Hair2.gameObject.SetActive(true);
                Hair1A.sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                Hair2.sprite = ResManager.GetSprite("Hair_2_" + pData.Hair);
                Hair1A.color = hairColor;
                Hair2.color = hairColor;
                break;
            case 2:
            case 3:
                Hair1A.gameObject.SetActive(true); Hair1B.gameObject.SetActive(false); Hair2.gameObject.SetActive(false);
                Hair1A.sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                Hair1A.color = hairColor;
                break;
            case 100:
                Hair1A.gameObject.SetActive(false); Hair1B.gameObject.SetActive(true); Hair2.gameObject.SetActive(true);
                Hair1B.sprite = ResManager.GetSprite("Hair_1_" + pData.Hair);
                Hair2.sprite = ResManager.GetSprite("Hair_2_" + pData.Hair);
                Hair1B.color = hairColor;
                Hair2.color = hairColor;
                break;
        }
    }
    public void SetUiEqParts(PlayerData pData, string backUpKey, Dictionary<string, GameObject> mGameObj)
    {
        var eq = pData.EqSlot;
        string[] allParts = {"BaseHand1A", "BaseHand1A2", "BaseHand1B", "BaseHand2", "BaseBoth",
            "EqBody", "EqHand1A", "EqHand1B", "EqHand2", "EqBoth", "OneWp1", "OneWp2", "TwoWp1", "TwoWp2", "TwoWp3"};
        foreach (var v in allParts)
            mGameObj[v].SetActive(false);

        if (eq["Armor"] != null)
        {
            string eqStr = eq["Armor"].ItemId.ToString();
            if (backUpKey != eqStr + "_body")
            {
                mGameObj["EqBody"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_body");
                mGameObj["EqHand1A"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1A");
                mGameObj["EqHand1B"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand1B");
                mGameObj["EqHand2"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_hand2");
                mGameObj["EqBoth"].GetComponent<Image>().sprite = ResManager.GetSprite(eqStr + "_both");
            }
            mGameObj["EqBody"].SetActive(true);
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