using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string path = Application.persistentDataPath + "/save_scene.json";

    // 저장되지 않아야 할 씬 리스트
    private static readonly string[] nonSavableScenes = new string[]
    {
        "MainVRScene",  // 메인 화면 씬
        "FairyTaleSelectionScene",  // 동화 선택 씬
        "Endings",  // 엔딩 모음 씬
        "Setting",// 환경설정 씬
        "EndingCollection",
        "ObstacleMain",
        "ObstacleRule",
        "MaseMain",
        "MazeRulleSence",
        "DefenseMain",
        "DefenseRule",
        "NPCInteraction 1"
    };

    [System.Serializable]
    public class SceneSaveData
    {
        public string sceneName;
        public string savedTime;
    }

    public static void SaveCurrentScene()
    {
        // 현재 씬의 이름을 가져옴
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 저장되지 않아야 할 씬에 대해 저장하지 않음
        foreach (string nonSavableScene in nonSavableScenes)
        {
            if (currentScene == nonSavableScene)
            {
                Debug.Log($"⚠️ {currentScene} 씬은 저장되지 않습니다.");
                return;
            }
        }

        // 저장할 씬 데이터 생성
        SceneSaveData data = new SceneSaveData
        {
            sceneName = currentScene,
            savedTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"💾 자동 저장 완료: {data.sceneName}");
    }

    public static string LoadSavedScene()
    {
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        SceneSaveData data = JsonUtility.FromJson<SceneSaveData>(json);
        Debug.Log($"📂 저장된 씬 불러오기: {data.sceneName}");
        return data.sceneName;
    }

    public static bool HasSaveData() => File.Exists(path);

    public static void ClearSave()
    {
        if (File.Exists(path))
            File.Delete(path);
    }
    
    public static void ValidateSaveAtStartup()
    {
        string savedScene = LoadSavedScene();

        if (string.IsNullOrEmpty(savedScene))
        {
            Debug.Log("🧹 저장 데이터 없음 → 초기 상태 유지");
            return;
        }

        // 저장 제외 목록 불러오기
        var nonSavableScenes = typeof(SaveManager)
            .GetField("nonSavableScenes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .GetValue(null) as string[];

        // 저장된 씬이 제외 목록에 속하면 파일 삭제
        foreach (var scene in nonSavableScenes)
        {
            if (savedScene == scene)
            {
                Debug.Log($"🧹 '{savedScene}'은 저장 제외 씬 → 세이브 파일 삭제");
                ClearSave();
                return;
            }
        }

        Debug.Log($"💾 유효한 저장 씬 유지: {savedScene}");
    }



}