using GB;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
public class PlayerManager : AutoSingleton<PlayerManager>
{

    [Header("플레이어 데이터")]
    public int currentCity = 0;
    public PlayerData pData;
    public List<List<InvenGrid>> grids;
    public Vector3 worldPos = new Vector3(0, 0, 0);
    [Header("기타")]
    public bool isObjCreated = false; // 월드 오브젝트 생성 여부

    [Header("테스트")]
    public int testSkin = 1;
    public int testHairColor = 1;
    protected void Awake()
    {
        InitGrid();
    }
    // 인벤토리 그리드 초기화
    private void InitGrid()
    {
        grids = new List<List<InvenGrid>>();
        for (int y = 0; y < 10; y++)
        {
            List<InvenGrid> row = new List<InvenGrid>();
            for (int x = 0; x < 10; x++)
            {
                row.Add(new InvenGrid { x = x, y = y, slotId = -1 });
            }
            grids.Add(row);
        }
    }
    public void ApplyEqSlot(string eq, ItemData data)
    {
        // UnityEngine.Debug.Log(eq + " 장착");
        pData.EqSlot[eq] = data;
        CalcPlayerStat();
        CheckCharInfoPop();
    }
    public void TakeoffEq(string eq)
    {
        // UnityEngine.Debug.Log(eq + " 해제");
        pData.EqSlot[eq] = null;
        CalcPlayerStat();
        CheckCharInfoPop();
    }
    private void CheckCharInfoPop()
    {
        if (GameObject.Find("CharInfoPop") && GameObject.Find("CharInfoPop").GetComponent<CharInfoPop>().isActive)
            GameObject.Find("CharInfoPop").GetComponent<CharInfoPop>().UpdateCharInfo();
    }
    // 플레이어 데이터 초기화
    public void ApplyPlayerData(PlayerData data)
    {
        pData = new PlayerData();
        pData.Name = data.Name;
        pData.Age = data.Age;
        pData.Gen = data.Gen;
        pData.Crown = data.Crown;
        pData.Grade = data.Grade;
        pData.GradeExp = data.GradeExp;
        pData.GradeNext = data.GradeNext;
        pData.Lv = data.Lv;
        pData.Exp = data.Exp;
        pData.NextExp = GsManager.I.GetNextExp(data.Lv);
        pData.HP = data.HP; pData.MP = data.MP; pData.SP = data.SP;
        pData.AddHP = data.AddHP; pData.AddMP = data.AddMP; pData.AddSP = data.AddSP;

        pData.VIT = data.VIT; pData.END = data.END; pData.STR = data.STR; pData.AGI = data.AGI; pData.FOR = data.FOR; pData.INT = data.INT; pData.CHA = data.CHA; pData.LUK = data.LUK;

        pData.Inven = data.Inven;

        pData.Skin = data.Skin;
        pData.Face = data.Face;
        pData.Eyebrow = data.Eyebrow;
        pData.Eye = data.Eye;
        pData.EyeColor = data.EyeColor;
        pData.Ear = data.Ear;
        pData.Nose = data.Nose;
        pData.Mouth = data.Mouth;
        pData.Hair = data.Hair;
        pData.HairColor = data.HairColor;

        CalcPlayerStat();
        Presenter.Send("WorldMainUI", "UpdateInfo");
    }
    public void DummyPlayerData()
    {
        if (pData == null)
            pData = new PlayerData();
        pData.Name = "주인공";
        pData.Age = 17;
        pData.Gen = 0;
        pData.Crown = 20000;
        pData.Grade = 1;
        pData.GradeExp = 0;
        pData.GradeNext = 1000;
        pData.Lv = 1;
        pData.Exp = 0;
        pData.NextExp = GsManager.I.GetNextExp(pData.Lv);
        pData.GainExp = 0;
        pData.AddHP = 0; pData.AddMP = 0; pData.AddSP = 0;
        pData.VIT = 5; pData.END = 5; pData.STR = 5; pData.AGI = 5; pData.FOR = 5; pData.INT = 5; pData.CHA = 5; pData.LUK = 5;

        pData.Skin = 1; pData.Face = 1;
        pData.Eyebrow = 1; pData.Eye = 1;
        pData.EyeColor = 1; pData.Ear = 1;
        pData.Nose = 1; pData.Mouth = 1;
        pData.Hair = 1; pData.HairColor = 1;

        ItemManager.I.CreateInvenItem(30002, 1, 0);
        ItemManager.I.CreateInvenItem(10002, 3, 0);
        ItemManager.I.CreateInvenItem(30001, -1, -1); // 장착 아이템은 -1, -1로 설정
        ItemManager.I.CreateInvenItem(10001, -1, -1);

        pData.EqSlot["Hand1"] = pData.Inven[2]; // 손1
        pData.EqSlot["Armor"] = pData.Inven[3]; // 갑옷

        CalcPlayerStat();
        pData.HP = pData.MaxHP;
        pData.MP = pData.MaxMP;
        pData.SP = pData.MaxSP;
        Presenter.Send("WorldMainUI", "UpdateInfo");
    }
    private void CalcPlayerStat()
    {
        pData.MaxHP = pData.VIT * 4 + pData.AddHP;
        pData.MaxMP = pData.INT * 4 + pData.AddMP;
        pData.MaxSP = pData.END * 4 + pData.AddSP;
        if (pData.HP > pData.MaxHP) pData.HP = pData.MaxHP;
        if (pData.MP > pData.MaxMP) pData.MP = pData.MaxMP;
        if (pData.SP > pData.MaxSP) pData.SP = pData.MaxSP;

        pData.Att = pData.STR * 2;
        pData.Def = pData.VIT;
        pData.Crt = 50 + (pData.LUK * 2);
        pData.CrtRate = pData.LUK;
        int agi = pData.AGI / 4;
        pData.Hit = 60 + agi;
        pData.Eva = 10 + agi;
        //////
        string[] eq = new string[] { "Hand1", "Hand2", "Armor", "Shoes", "Helmet", "Gloves", "Belt", "Cape", "Necklace", "Ring1", "Ring2" };
        foreach (string e in eq)
        {
            if (pData.EqSlot[e] != null)
            {
                switch (e)
                {
                    case "Hand1":
                    case "Hand2":
                        // Debug.Log(pData.EqSlot[e].Val);
                        pData.Att += pData.EqSlot[e].Val;
                        break;
                    case "Necklace":
                    case "Ring1":
                    case "Ring2":
                        break;
                    default:
                        pData.Def += pData.EqSlot[e].Val;
                        break;
                }
            }
        }
    }
    public Vector2 CanAddItem(int w, int h)
    {
        // Debug.Log(w + "   " + h);
        for (int y = 0; y < 10; y++)
        {
            if (y + h > 10) break;
            for (int x = 0; x < 10; x++)
            {
                if (x + w > 10) break;
                bool isAdd = true;
                for (int i = y; i < y + h; i++)
                {
                    for (int j = x; j < x + w; j++)
                    {
                        if (grids[i][j].slotId != -1)
                        {
                            isAdd = false;
                            break;
                        }
                    }
                }
                if (isAdd) return new Vector2(x, y);
            }
        }
        return new Vector2(-1, -1);
        //현재 버그있음
        //추후에는 빈칸일때 회전된 상태로도 검색하는 기능도 추가해야함
    }

    public void ChangePlayerSkin()
    {
        testSkin++;
        if (testSkin > 10) testSkin = 1;
        pData.Skin = testSkin;
        Presenter.Send("CharInfoPop", "UpdateCharAppearance");
    }
    public void ChangePlayerHairColor()
    {
        testHairColor++;
        if (testHairColor > 27) testHairColor = 1;
        pData.HairColor = testHairColor;
        Presenter.Send("CharInfoPop", "UpdateCharAppearance");
    }
}

[CustomEditor(typeof(PlayerManager))]
public class PlayerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerManager myScript = (PlayerManager)target;

        if (GUILayout.Button("플레이어 피부 변경"))
        {
            myScript.ChangePlayerSkin();
        }
        if (GUILayout.Button("플레이어 머리 색상 변경"))
        {
            myScript.ChangePlayerHairColor();
        }
    }
}
