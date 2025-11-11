using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // ✅ 추가

public class ScreenFader : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeDuration = 2f;
    public string nextSceneName = "ObstacleEnding"; // 원하는 씬 이름으로 수정

    public void StartFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private System.Collections.IEnumerator FadeOutCoroutine()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 1f;
        Debug.Log("엔딩으로 이도옹");
        // ⚙️ 1️⃣ 중복 EventSystem 정리
        EventSystem[] systems = FindObjectsOfType<EventSystem>();
        if (systems.Length > 1)
        {
            Debug.LogWarning($"⚠️ EventSystem이 {systems.Length}개 감지됨 → 중복 제거 중...");
            // 첫 번째만 남기고 나머지 삭제
            for (int i = 1; i < systems.Length; i++)
            {
                Destroy(systems[i].gameObject);
            }
        }

// ⚙️ 2️⃣ GlobalScreenFader 제거 (DontDestroyOnLoad로 남아있는 경우)
        var globalFader = FindObjectOfType<GlobalScreenFader>();
        if (globalFader != null)
        {
            Debug.Log("🧹 GlobalScreenFader 제거 (엔딩씬 중복 방지)");
            Destroy(globalFader.gameObject);
        }

        SceneManager.LoadSceneAsync(nextSceneName);
    }
}