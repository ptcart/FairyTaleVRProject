using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PersistentEventSystemManager : MonoBehaviour
{
    private static PersistentEventSystemManager instance;

    void Awake()
    {
        // 싱글톤 유지 (한 번만 존재)
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // 🔹 씬 바뀌어도 유지

        SceneManager.activeSceneChanged += OnSceneChanged; // 🔹 씬 바뀔 때마다 실행
        CleanDuplicateEventSystems(); // 🔹 첫 씬에서도 바로 검사
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        CleanDuplicateEventSystems(); // 🔹 씬이 바뀌면 다시 검사
    }

    private void CleanDuplicateEventSystems()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>(true);
        if (systems.Length > 1)
        {
            Debug.LogWarning($"⚠️ EventSystem {systems.Length}개 감지됨 → 중복 제거 중...");
            for (int i = 1; i < systems.Length; i++)
                Destroy(systems[i].gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged; // 🔹 이벤트 해제 (메모리 누수 방지)
    }
}