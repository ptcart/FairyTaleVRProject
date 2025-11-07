using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene(nextSceneName);
    }
}