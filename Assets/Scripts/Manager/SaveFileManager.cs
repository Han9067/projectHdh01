using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using GB;

public class SaveFileManager : AutoSingleton<SaveFileManager>
{
    private static bool isFirstLoad = true;
    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);

        if (isFirstLoad)
        {
            LoadGameFile();
            isFirstLoad = false;
        }
    }
    public void SaveGameFile()
    {
        PlayerData pData = PlayerManager.I.pData;
        GameSaveData saveData = new GameSaveData();
        saveData.playerData = pData;
        Debug.Log(JsonUtility.ToJson(saveData, true));
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "game_save.json"), json);
    }
    public void LoadGameFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "game_save.json");
        if (File.Exists(path))
        {
            try
            {
                // JSON 파일 읽기
                string jsonContent = File.ReadAllText(path);
                GameSaveData loadedData = JsonUtility.FromJson<GameSaveData>(jsonContent);
                // PlayerManager에 데이터 로드
                if (loadedData.playerData != null)
                {
                    PlayerManager.I.ApplyPlayerData(loadedData.playerData);
                }
                Debug.Log("=== 게임 데이터 로드 완료 ===");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"게임 데이터 로드 중 오류 발생: {e.Message}");
                PlayerManager.I.DummyPlayerData();
            }
        }
        else
        {
            Debug.Log("저장 파일을 찾을 수 없습니다: " + path);
            PlayerManager.I.DummyPlayerData();
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
            myScript.LoadGameFile();
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