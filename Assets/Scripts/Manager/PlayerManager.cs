using GB;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;
public class PlayerManager : AutoSingleton<PlayerManager>
{

    [Header("플레이어 데이터")]
    public int currentCity = 0;
    public int fatigue = 0; // 피로도
    public PlayerData pData;
    public List<List<InvenGrid>> grids;
    public Vector3 worldPos = new Vector3(0, 0, 0);
    [Header("기타")]
    public bool isObjCreated = false; // 월드 오브젝트 생성 여부

    [Header("테스트")]
    public int testSkin = 1;
    public int testHairColor = 1;
    public void LoadPlayerManager()
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
        if (CharInfoPop.isActive)
            Presenter.Send("CharInfoPop", "UpdateCharInfo");
    }
    // 플레이어 데이터 초기화
    public void ApplyPlayerData(PlayerData data, Vector3 pos)
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
        pData.EqSlot = data.EqSlot;

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

        pData.QuestList = data.QuestList;
        // pData.QuestMax = data.QuestMax;
        pData.QuestMax = 5;

        pData.SkList = data.SkList;

        CalcPlayerStat();

        isObjCreated = true; //저장된 데이터이기에 해당 불대수 true로 설정
        worldPos = pos;
    }
    public void DummyPlayerData()
    {
        pData = new PlayerData();
        pData.Name = "주인공";
        pData.Age = 17;
        pData.Gen = 0;
        pData.Crown = 2000;
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

        // ItemManager.I.CreateInvenItem(30001, -1, -1); //옷, 장착 아이템은 -1, -1로 설정
        ItemManager.I.CreateInvenItem(30002, -1, -1); //무기
        ItemManager.I.CreateInvenItem(10001, -1, -1); //옷
        pData.EqSlot["Hand1"] = pData.Inven[0]; // 손1
        pData.EqSlot["Armor"] = pData.Inven[1]; // 갑옷
        ItemManager.I.CreateInvenItem(60001, 0, 0); //물약
        // ItemManager.I.CreateInvenItem(68001, 0, 1);
        ItemManager.I.CreateInvenItem(30001, 2, 0); //무기
        ItemManager.I.CreateInvenItem(32001, 3, 0);

        CalcPlayerStat();
        pData.HP = pData.MaxHP;
        pData.MP = pData.MaxMP;
        pData.SP = pData.MaxSP;

        pData.QuestList = new List<QuestInstData>();
        pData.QuestMax = 5;

        fatigue = 100; //기본이 100
        // pData.SkList = new Dictionary<int, SkData>();
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
                        pData.Att += pData.EqSlot[e].Att[13]; // 공격력
                        break;
                    case "Necklace":
                    case "Ring1":
                    case "Ring2":
                        break;
                    default:
                        pData.Def += pData.EqSlot[e].Att[12]; // 방어력
                        break;
                }
            }
        }
    }
    public Vector2 CanAddItem(int w, int h)
    {
        // 1. 매개변수 유효성 검사
        if (w <= 0 || h <= 0 || w > 10 || h > 10)
        {
            Debug.LogWarning($"CanAddItem: 잘못된 크기 입력 w={w}, h={h}");
            return new Vector2(-1, -1);
        }

        // 3. 빈 공간 탐색
        for (int y = 0; y < 10; y++)
        {
            // 세로 범위 체크 (경계 초과 시 더 이상 탐색 불필요)
            if (y + h > 10) break;

            // grids[y]가 null이거나 크기가 맞지 않는 경우 방어
            if (grids[y] == null || grids[y].Count != 10)
            {
                Debug.LogError($"CanAddItem: grids[{y}]가 초기화되지 않았습니다!");
                continue;
            }

            for (int x = 0; x < 10; x++)
            {
                // 가로 범위 체크
                if (x + w > 10) break;

                bool isAdd = true;

                // 아이템이 들어갈 영역 검사
                for (int i = y; i < y + h; i++)
                {
                    for (int j = x; j < x + w; j++)
                    {
                        if (grids[i][j].slotId != -1)
                        {
                            isAdd = false;
                            break; // 안쪽 j 루프 탈출
                        }
                    }
                    if (!isAdd) break;
                }

                // 빈 공간을 찾았다면 해당 좌표 반환
                if (isAdd)
                {
                    // Debug.Log($"빈 공간 발견: ({x}, {y}), 크기: {w}x{h}");
                    return new Vector2(x, y);
                }
            }
        }

        // 빈 공간을 찾지 못함
        // Debug.Log($"빈 공간 없음: 크기 {w}x{h}");
        return new Vector2(-1, -1);

        // 추후에는 빈칸일때 회전된 상태로도 검색하는 기능도 추가해야함
    }

    public void CompleteQuest(int qid)
    {
        foreach (var q in pData.QuestList)
        {
            if (q.Qid == qid)
            {
                q.State = 2;
                break;
            }
        }
    }

    public void AddSkExp(int skId, int val)
    {
        if (pData.SkList.ContainsKey(skId))
        {
            pData.SkList[skId].Exp += val;
        }
        else
        {
            pData.SkList[skId] = GsManager.I.SkDataList[skId].Clone();
            pData.SkList[skId].Lv = 1;
            pData.SkList[skId].Exp = val;
            pData.SkList[skId].NextExp = GsManager.I.GetSkNextExp(1);
            //새로 획득한 스킬이라 팝업을 표시...표시는 메세지박스에 언급되도록
        }
    }
    #region 🎨 TESTING
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
    public void TestAddSkExp()
    {
        AddSkExp(1, 20);
    }
    public void TestDropItem()
    {
        ItemManager.I.TestDropItem();
    }
    #endregion
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
        if (GUILayout.Button("스킬 추가"))
        {
            myScript.TestAddSkExp();
        }
        if (GUILayout.Button("아이템 드랍"))
        {
            myScript.TestDropItem();
        }
    }
}
