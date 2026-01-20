using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using GB;
using Newtonsoft.Json;

// 통합 저장 데이터 클래스
[System.Serializable]
public class GameSaveData
{
    public int curDay;
    public float curTime;
    public Vector3 playerPos;
    public PlayerData playerData;
    public bool isGate1Open;
    public Dictionary<int, Dictionary<int, QuestInstData>> CityQuest;
    public Dictionary<int, WorldMonData> worldMonDataList;
    public List<List<SkSlot>> playerSkSlots;
}

public class SaveFileManager : AutoSingleton<SaveFileManager>
{
    private static bool isFirstLoad = true;
    public void LoadSaveFileManager()
    {
        if (isFirstLoad)
        {
            isFirstLoad = false;
            LoadGameFile();
        }
    }
    public void SaveGameFile()
    {
        GameSaveData saveData = new GameSaveData();

        #region 저장 데이터
        saveData.playerPos = WorldCore.I.GetPlayerPos();
        saveData.playerData = PlayerManager.I.pData;
        saveData.CityQuest = QuestManager.I.CityQuest;
        saveData.worldMonDataList = WorldObjManager.I.worldMonDataList;
        saveData.curDay = GsManager.I.tDay;
        saveData.curTime = GsManager.I.wTime;
        saveData.playerSkSlots = PlayerManager.I.skSlots;
        #endregion

        // ⭐ 여기가 핵심!
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };

        string json = JsonConvert.SerializeObject(saveData, settings);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "game_save.json"), json);
        Debug.Log("=== 게임 데이터 저장 완료 ===");
    }
    public void LoadGameFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "game_save.json");

        if (File.Exists(path))
        {
            try
            {
                // ✅ 파일 읽기와 역직렬화를 try 안에 포함
                string jsonContent = File.ReadAllText(path);
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                GameSaveData loadedData = JsonConvert.DeserializeObject<GameSaveData>(jsonContent, settings);

                PlayerManager.I.ApplyPlayerData(loadedData.playerData, loadedData.playerPos);
                QuestManager.I.CityQuest = loadedData.CityQuest;
                WorldObjManager.I.worldMonDataList = loadedData.worldMonDataList;
                GsManager.I.tDay = loadedData.curDay;
                GsManager.I.wTime = loadedData.curTime;
                PlayerManager.I.isObjCreated = true; //WorldObjManager.I.worldMonDataList 에 데이터가 있기떄문에 덮여씌어지지 않도록 isObjCreated 를 true 로 설정
                PlayerManager.I.isGate1Open = loadedData.isGate1Open; //관문 통행 여부
                PlayerManager.I.skSlots = loadedData.playerSkSlots; //스킬 슬롯 데이터 로드
                Debug.Log("=== 게임 데이터 로드 완료 ===");
            }
            catch (System.Exception e)
            {
                // ✅ 상세한 에러 정보 출력
                Debug.LogError($"게임 데이터 로드 중 오류 발생:\n{e.Message}\n{e.StackTrace}");
                PlayerManager.I.DummyPlayerData();
            }
        }
        else
        {
            // ✅ 파일이 없을 때 처리
            Debug.Log("저장 파일이 없습니다. 새 게임으로 시작합니다.");
            PlayerManager.I.DummyPlayerData();
        }
    }
    public void LoadDataTest()
    {
        string path = Path.Combine(Application.persistentDataPath, "game_save.json");
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            GameSaveData loadedData = JsonConvert.DeserializeObject<GameSaveData>(jsonContent);
            Debug.Log(loadedData.playerPos);
            Debug.Log(loadedData.playerData.EqSlot["Hand1"].Name);
            // Debug.Log(loadedData.playerData.EqSlot["Armor"].Name);

        }

    }
    // 저장 파일 삭제
    public void DelSaveFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "game_save.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("저장 파일이 삭제되었습니다: " + path);
        }
        else
        {
            Debug.Log("삭제할 저장 파일이 없습니다.");
        }
    }

    // 저장 파일 존재 여부 확인
    public bool HasSaveFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "game_save.json");
        return File.Exists(path);
    }
}

[CustomEditor(typeof(SaveFileManager))]
public class SaveFileManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveFileManager myScript = (SaveFileManager)target;

        GUILayout.Space(10);
        GUILayout.Label("게임 저장/로드 테스트", EditorStyles.boldLabel);

        if (GUILayout.Button("게임 데이터 저장"))
        {
            myScript.SaveGameFile();
        }

        if (GUILayout.Button("게임 데이터 로드"))
        {
            // myScript.LoadGameFile();
            myScript.LoadDataTest();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("저장 파일 삭제"))
        {
            if (EditorUtility.DisplayDialog("저장 파일 삭제",
                "정말로 저장 파일을 삭제하시겠습니까?", "삭제", "취소"))
            {
                myScript.DelSaveFile();
            }
        }

        GUILayout.Space(5);
        GUILayout.Label($"저장 파일 존재: {(myScript.HasSaveFile() ? "있음" : "없음")}", EditorStyles.miniLabel);
    }
}