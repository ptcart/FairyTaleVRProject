using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalScreenFader : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("검정 이미지를 가진 CanvasGroup (필수). 씬마다 새로 만들지 말고 Overlay Canvas 밑에 1개 두세요.")]
    public CanvasGroup fadeGroup;

    [Tooltip("페이드 시간(초)")]
    public float fadeDuration = 1.5f;

    [Tooltip("씬이 시작될 때 자동으로 검정→투명 페이드 인을 수행")]
    public bool autoFadeInOnStart = true;

    [Header("Safety")]
    [Tooltip("씬이 바뀌어 fadeGroup이 파괴되면 새 씬에서 자동으로 다시 찾아 바인딩합니다 (Tag/Name 권장).")]
    public bool autoRebindFadeGroup = true;                 // ★
    [Tooltip("autoRebind가 켜져있을 때 우선 찾을 태그(없으면 비워둠).")]
    public string fadeGroupTag = "";                         // ★
    [Tooltip("autoRebind가 켜져있을 때 우선 찾을 이름(없으면 비워둠).")]
    public string fadeGroupName = "ScreenFadeOverlay";       // ★

    private static GlobalScreenFader _instance;
    public static GlobalScreenFader Instance => _instance;

    private Coroutine _currentFade;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // ★ 씬 변경 시 코루틴 중단 + 필요 시 재바인딩
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        if (!EnsureFadeGroupBound())
        {
            Debug.LogWarning("[GlobalScreenFader] fadeGroup이 연결되지 않았습니다. Fade가 보이지 않습니다.");
            return;
        }

        // 시작은 검정
        fadeGroup.gameObject.SetActive(true);
        SafeSetAlpha(1f);
        fadeGroup.interactable = false;
        fadeGroup.blocksRaycasts = true;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged; // ★
    }

    private void Start()
    {
        if (autoFadeInOnStart && fadeGroup != null)
        {
            StartFade(FadeInCoroutine());
        }
    }

    // ★ 씬이 바뀔 때: 코루틴 정지 + fadeGroup 유효성 확인/재바인딩
    private void OnActiveSceneChanged(Scene prev, Scene next)
    {
        if (_currentFade != null) { StopCoroutine(_currentFade); _currentFade = null; }
        EnsureFadeGroupBound();
    }

    public void FadeAndLoadScene(string sceneName)
    {
        if (!EnsureFadeGroupBound())
        {
            Debug.LogWarning("[GlobalScreenFader] fadeGroup 없음 → 즉시 LoadScene");
            SceneManager.LoadScene(sceneName);
            return;
        }
        StartFade(FadeOutAndLoadCoroutine(sceneName));
    }

    public void FadeIn()
    {
        if (!EnsureFadeGroupBound()) return;
        StartFade(FadeInCoroutine());
    }

    // ───────── 내부 구현 ─────────

    private void StartFade(IEnumerator routine)
    {
        if (!EnsureFadeGroupBound()) return;

        if (!fadeGroup.gameObject.activeSelf)
            fadeGroup.gameObject.SetActive(true);

        if (_currentFade != null)
        {
            StopCoroutine(_currentFade);
            _currentFade = null;
        }
        _currentFade = StartCoroutine(routine);
    }

    private IEnumerator FadeOutAndLoadCoroutine(string sceneName)
    {
        if (fadeGroup == null) yield break; // ★

        float t = 0f;
        float start = fadeGroup.alpha;
        const float end = 1f;

        fadeGroup.blocksRaycasts = true;

        while (t < fadeDuration)
        {
            if (fadeGroup == null) yield break; // ★
            t += Time.unscaledDeltaTime;
            SafeSetAlpha(Mathf.Lerp(start, end, t / fadeDuration)); // ★
            yield return null;
        }
        SafeSetAlpha(1f); // ★

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // 새 씬으로 넘어오면 OnActiveSceneChanged에서 코루틴이 정리/리바인딩됨
        if (EnsureFadeGroupBound())
            yield return FadeInCoroutine();

        _currentFade = null;
    }

    private IEnumerator FadeInCoroutine()
    {
        if (fadeGroup == null) yield break; // ★

        float t = 0f;
        float start = fadeGroup.alpha;
        const float end = 0f;

        fadeGroup.blocksRaycasts = true;

        while (t < fadeDuration)
        {
            if (fadeGroup == null) yield break; // ★
            t += Time.unscaledDeltaTime;
            SafeSetAlpha(Mathf.Lerp(start, end, t / fadeDuration)); // ★
            yield return null;
        }
        SafeSetAlpha(0f);  // ★
        fadeGroup.blocksRaycasts = false;

        _currentFade = null;
    }

    // ───────── 유틸 ─────────

    // ★ 안전한 알파 세터 (파괴 체크)
    private void SafeSetAlpha(float a)
    {
        if (fadeGroup != null) fadeGroup.alpha = a;
    }

    // ★ fadeGroup이 비었거나 파괴되었으면 자동으로 찾아서 바인딩
    private bool EnsureFadeGroupBound()
    {
        if (fadeGroup) return true;

        if (!autoRebindFadeGroup) return false;

        CanvasGroup found = null;

        if (!string.IsNullOrEmpty(fadeGroupTag))
        {
            var go = GameObject.FindWithTag(fadeGroupTag);
            if (go) found = go.GetComponent<CanvasGroup>();
        }

        if (found == null && !string.IsNullOrEmpty(fadeGroupName))
        {
            var go = GameObject.Find(fadeGroupName);
            if (go) found = go.GetComponent<CanvasGroup>();
        }

        if (found == null)
        {
            // 최후의 수단: 활성 오브젝트 중 첫 CanvasGroup
            found = FindObjectOfType<CanvasGroup>(includeInactive: true);
        }

        if (found != null)
        {
            fadeGroup = found;
            // 씬에서 따로 존재하는 오브젝트라면 비활성 상태일 수 있으니 기본값 정리
            fadeGroup.gameObject.SetActive(true);
            return true;
        }
        return false;
    }
}
