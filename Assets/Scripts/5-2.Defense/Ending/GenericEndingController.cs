using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

/// <summary>
/// 🎬 모든 엔딩씬에서 공용으로 사용하는 엔딩 연출 컨트롤러
/// - Inspector에서 엔딩 스크립트(대사) 설정 가능
/// - 자동 페이드 인/아웃 및 버튼 표시 지원
/// - Flask 서버로 엔딩 클리어 상태(is_cleared) 자동 업데이트
/// </summary>
public class GenericEndingController : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup fadePanel;          // 검은 화면 (페이드용)
    public TMP_Text storyText;             // 대사 텍스트

    [Header("버튼 설정")]
    public GameObject mainButton;          // "메인으로" 버튼
    public CanvasGroup mainButtonGroup;    // "메인으로" 버튼 페이드 제어용
    public GameObject retryButton;         // "다시하기" 버튼
    public CanvasGroup retryButtonGroup;   // "다시하기" 버튼 페이드 제어용

    [Header("설정값")]
    public float fadeDuration = 1.5f;      // 글씨 페이드 인/아웃 속도
    public float textDelay = 1.0f;         // 문장 사이 대기시간

    [Header("엔딩 텍스트 설정")]
    [TextArea(2, 5)]
    public List<string> storyLines = new List<string>();  // 엔딩 대사 리스트 (씬별 지정)

    [Header("엔딩 제목")]
    public string endingTitle = "# 엔딩";   // 마지막에 표시될 제목 (예: “#1. 해피엔딩”)

    private int currentIndex = 0;
    private bool isTransitioning = false;
    private bool isEndingFinished = false;

    // ✅ Flask 서버 주소 (필요 시 IP 변경)
    private string serverUrl = "http://127.0.0.1:5000/command";

    void Start()
    {
        // ✅ 현재 씬 이름에서 ending_id 추출 후 Flask 업데이트
        string sceneName = SceneManager.GetActiveScene().name;
        int endingId = ExtractEndingId(sceneName);
        Debug.Log($"🎬 현재 엔딩 씬: {sceneName}, ID: {endingId}");
        StartCoroutine(UpdateEndingClearStatus(endingId));

        // ✅ 초기 UI 상태 설정
        if (fadePanel != null) fadePanel.alpha = 1f;
        if (storyText != null) storyText.alpha = 0f;

        if (mainButtonGroup != null) mainButtonGroup.alpha = 0f;
        if (retryButtonGroup != null) retryButtonGroup.alpha = 0f;

        if (mainButton != null) mainButton.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false);

        // ✅ 기본 문장 제공 (엔딩별 대사가 비어있을 경우)
        if (storyLines == null || storyLines.Count == 0)
        {
            storyLines = new List<string>
            {
                "이야기가 끝나자 세상은 다시 평화를 되찾았다.",
                "모든 것은 운명처럼 흘러가고...",
                "끝."
            };
        }

        storyText.text = storyLines[currentIndex];
        StartCoroutine(FadeText(0f, 1f));
    }

    void Update()
    {
        // ✅ 엔딩 완료 이후 입력 차단
        if (isEndingFinished || isTransitioning)
            return;

        // Oculus A버튼 / PC Space 키 입력
        if (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(NextSequence());
        }
    }

    private IEnumerator NextSequence()
    {
        isTransitioning = true;

        // ✅ 현재 문장 페이드 아웃
        yield return StartCoroutine(FadeText(1f, 0f));
        yield return new WaitForSeconds(textDelay);

        // ✅ 다음 문장으로 이동
        currentIndex++;
        if (currentIndex < storyLines.Count)
        {
            storyText.text = storyLines[currentIndex];
            yield return StartCoroutine(FadeText(0f, 1f));
        }
        else
        {
            // ✅ 마지막 문장 이후 엔딩 타이틀 출력
            isEndingFinished = true;

            storyText.text = endingTitle;
            yield return StartCoroutine(FadeText(0f, 1f));

            // ✅ 버튼 두 개 모두 표시
            yield return new WaitForSeconds(1f);
            if (mainButton != null) mainButton.SetActive(true);
            if (retryButton != null) retryButton.SetActive(true);

            // ✅ 버튼 페이드 인 (동시에 실행)
            if (mainButtonGroup != null) StartCoroutine(FadeButtonIn(mainButtonGroup));
            if (retryButtonGroup != null) StartCoroutine(FadeButtonIn(retryButtonGroup));
        }

        isTransitioning = false;
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

    private IEnumerator FadeButtonIn(CanvasGroup targetGroup)
    {
        if (targetGroup == null) yield break;

        targetGroup.alpha = 0f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            targetGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
    }

    // ✅ 엔딩 ID 자동 추출 (예: "Ending 2" → 2)
    private int ExtractEndingId(string sceneName)
    {
        string digits = Regex.Match(sceneName, @"\d+").Value;
        if (int.TryParse(digits, out int id))
            return id;
        return -1;
    }

    // ✅ Flask 서버로 엔딩 클리어 상태 업데이트
    private IEnumerator UpdateEndingClearStatus(int endingId)
    {
        if (endingId <= 0)
        {
            Debug.LogWarning("⚠️ 유효하지 않은 엔딩 ID입니다. 업데이트 생략.");
            yield break;
        }

        string jsonData = "{\"command\":\"ending_update\",\"payload\":{\"ending_id\":" + endingId + ",\"is_cleared\":true}}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(serverUrl, jsonData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"📤 Flask로 엔딩 업데이트 전송: {jsonData}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ 엔딩 업데이트 성공: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"❌ 엔딩 업데이트 실패: {request.error}");
        }
    }

    // ✅ 버튼 클릭 이벤트
    public void OnClickMainButton()
    {
        Debug.Log("🏠 메인 화면으로 이동");
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickRetryButton()
    {
        Debug.Log("🔁 다시하기 버튼 클릭");
        SceneManager.LoadScene("YourGameScene"); // 🔧 다시 시작할 씬 이름 지정
    }
}
