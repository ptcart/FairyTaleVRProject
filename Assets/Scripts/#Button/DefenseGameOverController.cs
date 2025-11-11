using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 💀 DefenseFade 전용 게임오버 컨트롤러 (버튼 없음)
/// - Inspector에서 문장(storyLines)과 다음 씬(nextSceneName) 지정
/// - 텍스트 페이드 인/아웃 후 자동으로 다음 씬 전환
/// - Flask, 입력, 버튼 모두 제거됨
/// </summary>
public class DefenseGameOverController : MonoBehaviour
{
    [Header("🎞️ UI 구성요소")]
    public CanvasGroup fadePanel;      // 검정 페이드 패널
    public TMP_Text storyText;         // 문장 표시용 텍스트

    [Header("⚙️ 연출 설정")]
    public float fadeDuration = 1.5f;  // 텍스트 페이드 시간
    public float textDelay = 1.0f;     // 문장 간 대기시간
    public float nextSceneDelay = 1.5f; // 마지막 문장 후 씬 이동까지 대기시간

    [Header("📜 문장 설정 (Inspector에서 지정)")]
    [TextArea(2, 5)]
    public List<string> storyLines = new List<string>();

    [Header("🎯 다음 씬 설정")]
    [Tooltip("자동으로 이동할 씬 이름 (예: DefenseIntro)")]
    public string nextSceneName;

    private int currentIndex = 0;

    void Start()
    {
        // 초기 세팅
        if (fadePanel != null) fadePanel.alpha = 1f;
        if (storyText != null) storyText.alpha = 0f;

        // 기본 문장 (없을 경우)
        if (storyLines == null || storyLines.Count == 0)
        {
            storyLines = new List<string>
            {
                "방어에 실패했습니다...",
                "마을은 불타올랐지만...",
                "당신의 용기는 사라지지 않았습니다."
            };
        }

        // 첫 문장부터 시작
        StartCoroutine(PlayEndingSequence());
    }

    private IEnumerator PlayEndingSequence()
    {
        // 문장 순서대로 출력
        while (currentIndex < storyLines.Count)
        {
            storyText.text = storyLines[currentIndex];

            // 텍스트 페이드 인
            yield return StartCoroutine(FadeText(0f, 1f));
            yield return new WaitForSeconds(textDelay);

            // 텍스트 페이드 아웃
            yield return StartCoroutine(FadeText(1f, 0f));

            currentIndex++;
        }

        // 마지막 문장 후 약간의 대기 후 다음씬 이동
        yield return new WaitForSeconds(nextSceneDelay);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"➡️ 다음 씬으로 자동 이동: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ 다음 씬 이름이 비어 있습니다! 자동 이동 생략됨.");
        }
    }

    private IEnumerator FadeText(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (storyText != null)
                storyText.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
    }
}
