using GB;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
public class PlayerManager : AutoSingleton<PlayerManager>
{

    [Header("플레이어 데이터")]
    public int currentCity = 0;
    public PlayerData pData;
    public List<List<InvenGrid>> grids;
    public Vector3 worldPos = new Vector3(0, 0, 0);
    public List<List<SkSlot>> skSlots;
    [Header("기타")]
    public bool isObjCreated = false; // 월드 오브젝트 생성 여부
    public bool isGate1Open = false; // 관문 1 통행 여부
    public int qstEventId = 0; //퀘스트 이벤트 ID

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
        pData.QuestClearList = data.QuestClearList;
        // pData.QuestMax = data.QuestMax;
        pData.QuestMax = 5;
        pData.TraceQId = data.TraceQId;

        pData.SkList = data.SkList;

        CalcPlayerStat();
        pData.HP = data.HP;
        pData.MP = data.MP;
        pData.SP = data.SP;
        pData.EP = data.EP; //원래 최대값 100이지만 추후 능력치 향상으로 증가할 수 있음

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
        pData.Grade = 1; //디폴트 0
        pData.GradeExp = 0;
        pData.GradeNext = 1000;
        pData.Lv = 1;
        pData.Exp = 0;
        pData.NextExp = GsManager.I.GetNextExp(pData.Lv);
        pData.GainExp = 0;
        pData.AddHP = 0; pData.AddMP = 0; pData.AddSP = 0;
        pData.VIT = 55; pData.END = 5; pData.STR = 5; pData.AGI = 5; pData.FOR = 5; pData.INT = 5; pData.CHA = 5; pData.LUK = 5;

        pData.Skin = 1; pData.Face = 1;
        pData.Eyebrow = 1; pData.Eye = 1;
        pData.EyeColor = 1; pData.Ear = 1;
        pData.Nose = 1; pData.Mouth = 1;
        pData.Hair = 1; pData.HairColor = 1;

        // ItemManager.I.CreateInvenItem(30001, -1, -1); //옷, 장착 아이템은 -1, -1로 설정
        ItemManager.I.CreateInvenItem(32001, -1, -1); //무기
        ItemManager.I.CreateInvenItem(10001, -1, -1); //옷
        pData.EqSlot["Hand1"] = pData.Inven[0]; // 손1
        pData.EqSlot["Armor"] = pData.Inven[1]; // 갑옷
        ItemManager.I.CreateInvenItem(60001, 0, 0); //물약
        // ItemManager.I.CreateInvenItem(65001, 0, 1);
        // ItemManager.I.CreateInvenItem(32001, 3, 0); //무기
        ItemManager.I.CreateInvenItem(60101, 1, 0); //스킬북
        ItemManager.I.CreateInvenItem(42001, 3, 1); //지팡이
        ItemManager.I.CreateInvenItem(46001, 4, 1); //창
        ItemManager.I.CreateInvenItem(48001, 5, 1); //활
        // ItemManager.I.CreateInvenItem(65001, 3, 1); //슬라임 젤
        // ItemManager.I.CreateInvenItem(65001, 4, 2);
        // ItemManager.I.CreateInvenItem(65001, 5, 2);
        // ItemManager.I.CreateInvenItem(65001, 6, 2);
        CalcPlayerStat();
        pData.HP = pData.MaxHP;
        pData.MP = pData.MaxMP;
        pData.SP = pData.MaxSP;

        pData.QuestList = new List<QuestInstData>();
        pData.QuestClearList = new List<int>();
        pData.QuestMax = 5;
        pData.TraceQId = 0;

        pData.EP = 70; //기본이 100
        pData.MaxEP = 100;
        // pData.SkList = new Dictionary<int, SkData>();

        // StartCoroutine(DelayedStartTutorial(0.2f)); //추후 튜토리얼 조건이 된다면 튜토리얼을 시작시킴
    }
    private void CalcPlayerStat()
    {
        pData.MaxHP = pData.VIT * SV.HpVal + pData.AddHP;
        pData.MaxMP = pData.INT * SV.MpVal + pData.AddMP;
        pData.MaxSP = pData.END * SV.SpVal + pData.AddSP;
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
    public void ClearMainQst(int qid)
    {
        pData.QuestClearList.Add(qid);
        pData.QuestList.Sort((a, b) => a.Qid.CompareTo(b.Qid)); //혹시 몰라 클리어 퀘스트 정렬
        foreach (var q in pData.QuestList)
        {
            if (q.Qid == qid)
            {
                pData.QuestList.Remove(q);
                break;
            }
        }
    }
    public void CompleteGuildQst(int quid)
    {
        foreach (var q in pData.QuestList)
        {
            if (q.QUid == quid)
            {
                q.State = 2;
                break;
            }
        }
    }
    public void ClearGuildQst(int quid)
    {
        foreach (var q in pData.QuestList)
        {
            if (q.QUid == quid)
            {
                pData.QuestList.Remove(q);
                break;
            }
        }
    }
    public void NextQuestOrder(int qid)
    {
        int n = pData.QuestList.FindIndex(q => q.Qid == qid);
        pData.QuestList[n].Order++;
        pData.QuestList[n].Desc = LocalizationManager.GetValue($"{pData.QuestList[n].Name}_{pData.QuestList[n].Order}_Desc");
        Presenter.Send("WorldMainUI", "SetTraceQst");
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
    public int GetSkLv(int skId)
    {
        return pData.SkList.ContainsKey(skId) ? pData.SkList[skId].Lv : 1;
    }
    public int GetQstItemCnt(int itemId)
    {
        int cnt = 0;
        foreach (var q in pData.Inven)
        {
            if (q.ItemId == itemId) cnt++;
        }
        return cnt;
    }
    #region 튜토리얼
    private IEnumerator DelayedStartTutorial(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        StartTutorial();
    }
    public void StartTutorial()
    {
        QuestData qData = QuestManager.I.QuestData[101];
        pData.QuestList.Add(new QuestInstData(qData.QuestID, 0, qData.Type, qData.Name, qData.IsTrace));
        int n = pData.QuestList.Count - 1;
        pData.QuestList[n].SetQuestBase(LocalizationManager.GetValue("QstM_Tuto_1_Desc"), 1, 1000, 1000, 100);
        pData.QuestList[n].Order = 1;
        pData.TraceQId = 101;

        Presenter.Send("WorldMainUI", "SetTraceQst");

        WorldCore.I.SetWorldCoreForTutorial();
    }
    #endregion
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
        AddSkExp(1001, 20); //명상
        // AddSkExp(1002, 20); //대시
        // AddSkExp(1003, 20); //이중 공격
    }
    public void TestDropItem()
    {
        ItemManager.I.TestDropItem();
    }
    public void TestAddExp()
    {
        pData.Exp += 10000;
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
            myScript.ChangePlayerSkin();
        if (GUILayout.Button("플레이어 머리 색상 변경"))
            myScript.ChangePlayerHairColor();
        if (GUILayout.Button("스킬 추가"))
            myScript.TestAddSkExp();
        if (GUILayout.Button("아이템 드랍"))
            myScript.TestDropItem();
        if (GUILayout.Button("튜토리얼 시작"))
            myScript.StartTutorial();
        if (GUILayout.Button("플레이어 경험치 획득"))
            myScript.TestAddExp();
    }
}
