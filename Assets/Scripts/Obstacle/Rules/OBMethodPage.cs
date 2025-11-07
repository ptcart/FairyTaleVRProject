using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ✅ DB 전용 방법 페이지 컨트롤러 (개선판)
/// - Flask /command (puzzlegame_slides)에서 슬라이드 목록 로드
/// - 페이지 오브젝트(methodPages[])를 토글
/// - Title/Body 텍스트 자동 주입 (줄바꿈/HTML 정규화 포함)
/// - VIDEO 타입이면 OBPageVideoController 통해 재생 (VideoDisplay 강제 활성화 + 타겟 연결)
/// - 버튼 상태/상호작용 상태를 안전하게 동기화
/// </summary>
public class OBMethodPage : MonoBehaviour
{
    // ------------------------------------------------------------
    // ▣ 인스펙터 필드
    // ------------------------------------------------------------

    [Header("페이지 오브젝트 (필수)")]
    [Tooltip("한 페이지당 GameObject 1개씩 (예: Method_1, Method_2 ...). 서버 slides 순서와 동일하지 않아도 되지만, 인덱스 접근을 위해 개수는 충분해야 합니다.")]
    public GameObject[] methodPages;      // Method_1, Method_2, Method_3 등

    [Header("버튼 (선택)")]
    [Tooltip("이전 페이지 버튼 GameObject (없으면 비워두세요)")]
    public GameObject previousButton;
    [Tooltip("다음 페이지 버튼 GameObject (없으면 비워두세요)")]
    public GameObject nextButton;
    [Tooltip("마지막 페이지에서 노출할 '시작' 버튼 GameObject (없으면 비워두세요)")]
    public GameObject startGameButton;

    [Header("비디오 (필수)")]
    [Tooltip("영상 재생 전용 컨트롤러. VIDEO 타입일 때 사용합니다.")]
    public OBPageVideoController videoController;

    [Header("서버 설정")]
    [Tooltip("Flask /command 엔드포인트 URL")]
    public string commandUrl = "http://localhost:5000/command";
    [Tooltip("요청 payload.game_event_id 값")]
    public int gameEventId = 1;

    [Header("UI 옵션")]
    [Tooltip("TextMeshPro/LegacyText에 WordWrapping 자동 적용")]
    public bool enableWordWrapping = true;
    [Tooltip("TextMeshPro/LegacyText에 RichText 자동 적용")]
    public bool enableRichText = true;

    // ------------------------------------------------------------
    // ▣ 내부 상태
    // ------------------------------------------------------------

    private int currentIndex = 0;     // 현재 표시 중인 슬라이드 인덱스 (0-based)
    private SlideDTO[] slides;        // 서버에서 받은 슬라이드 목록 (정렬 후 사용)

    // ------------------------------------------------------------
    // ▣ DTO 정의
    // ------------------------------------------------------------

    #region DTO
    [System.Serializable] 
    public class SlideDTO
    {
        public int slide_id;
        public int slide_order;
        public string title;
        public string content;
        public string media_type;  // VIDEO | IMAGE | NONE
        public string media_path;  // 예: "Videos/page1.mp4"
    }

    [System.Serializable] 
    public class SlideResponse
    {
        public int game_event_id;
        public SlideDTO[] slides;
    }
    #endregion

    // ------------------------------------------------------------
    // ▣ Unity 수명주기
    // ------------------------------------------------------------

    private void Awake()
    {
        // 기본 방어: 필수 의존성 체크
        if (methodPages == null || methodPages.Length == 0)
            Debug.LogWarning("[OBMethodPage] methodPages가 비어있습니다. 인스펙터에서 페이지 오브젝트를 할당하세요.");

        if (videoController == null)
            Debug.LogWarning("[OBMethodPage] videoController가 비어있습니다. VIDEO 타입 슬라이드는 재생되지 않습니다.");
    }

    private void Start()
    {
        StartCoroutine(LoadSlides());
    }

    private void OnDestroy()
    {
        // 씬 전환/오브젝트 파괴 시 안전하게 영상 중지
        if (videoController != null) videoController.StopVideo();
    }

    // ------------------------------------------------------------
    // ▣ 네트워크: 슬라이드 로드
    // ------------------------------------------------------------

    /// <summary>
    /// 📡 DB에서 슬라이드 로드
    /// - 서버 응답 파싱
    /// - slide_order 기준 정렬
    /// - 첫 페이지 표시
    /// </summary>
    private IEnumerator LoadSlides()
    {
        // 요청 본문(JSON)
        // {"command":"puzzlegame_slides","payload":{"game_event_id":<id>}}
        string body = $"{{\"command\":\"puzzlegame_slides\",\"payload\":{{\"game_event_id\":{gameEventId}}}}}";
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(body);

        using (var req = new UnityWebRequest(commandUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[OBMethodPage] 요청 실패: {req.error}");
                yield break;
            }

            string json = req.downloadHandler.text;
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[OBMethodPage] 빈 응답을 수신했습니다.");
                yield break;
            }

            SlideResponse res = null;
            try
            {
                res = JsonUtility.FromJson<SlideResponse>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[OBMethodPage] JSON 파싱 실패: {ex.Message}\n원본: {json}");
                yield break;
            }

            slides = res != null ? res.slides : null;

            if (slides == null || slides.Length == 0)
            {
                Debug.LogWarning("[OBMethodPage] 슬라이드 없음");
                yield break;
            }

            // ✅ slide_order 기준 정렬 (slide_order가 0인 경우도 있으니 안전 정렬)
            slides = slides
                .OrderBy(s => s != null ? s.slide_order : int.MaxValue)
                .ThenBy(s => s != null ? s.slide_id : int.MaxValue)
                .ToArray();

            // ✅ 첫 페이지 표시
            currentIndex = 0;
            ShowPage(currentIndex);
        }
    }

    // ------------------------------------------------------------
    // ▣ 버튼 이벤트
    // ------------------------------------------------------------

    /// <summary>
    /// ▶ 다음 페이지
    /// - 0.25초 딜레이 후 전환(더블클릭/연타에 대한 UI 안정성)
    /// </summary>
    public void ShowNext()
    {
        if (slides == null) return;
        if (currentIndex < slides.Length - 1)
            StartCoroutine(_Delay(() => { currentIndex++; ShowPage(currentIndex); }));
    }

    /// <summary>
    /// ◀ 이전 페이지
    /// </summary>
    public void ShowPrevious()
    {
        if (slides == null) return;
        if (currentIndex > 0)
            StartCoroutine(_Delay(() => { currentIndex--; ShowPage(currentIndex); }));
    }

    /// <summary>
    /// 작은 UI 안정화를 위한 짧은 지연
    /// </summary>
    private IEnumerator _Delay(System.Action act)
    {
        yield return new WaitForSeconds(0.25f);
        act?.Invoke();
    }

    // ------------------------------------------------------------
    // ▣ 페이지 표시
    // ------------------------------------------------------------

    /// <summary>
    /// ✅ 페이지 표시
    /// - 페이지 on/off
    /// - 버튼 상태/상호작용 동기화
    /// - 텍스트 주입(줄바꿈/HTML 정규화)
    /// - 영상 처리(타겟 연결 포함)
    /// </summary>
    private void ShowPage(int index)
    {
        // 기본 방어
        if (slides == null || slides.Length == 0) { Debug.LogWarning("[OBMethodPage] slides 비어있음"); return; }
        if (methodPages == null || methodPages.Length == 0) { Debug.LogWarning("[OBMethodPage] methodPages 비어있음"); return; }
        if (index < 0 || index >= slides.Length) { Debug.LogWarning($"[OBMethodPage] 인덱스 범위 초과: {index}"); return; }

        // ⚠️ 페이지 슬롯 부족 방어
        if (index >= methodPages.Length)
        {
            Debug.LogError($"[OBMethodPage] methodPages 개수({methodPages.Length})가 slides 개수({slides.Length})보다 적습니다. 인덱스 {index} 페이지 표시 불가.");
            return;
        }

        // 1) 페이지 on/off
        for (int i = 0; i < methodPages.Length; i++)
        {
            if (methodPages[i] != null)
                methodPages[i].SetActive(i == index);  // 현재 페이지만 활성화
        }

        // 2) 버튼 상태/상호작용 동기화
        bool hasPrev = index > 0;
        bool hasNext = index < slides.Length - 1;
        bool isLast = !hasNext;

        // GameObject 활성화
        if (previousButton) previousButton.SetActive(hasPrev);
        if (nextButton)     nextButton.SetActive(hasNext);
        if (startGameButton) startGameButton.SetActive(isLast);

        // Button.interactable도 가능하면 동기화
        SetButtonInteractable(previousButton, hasPrev);
        SetButtonInteractable(nextButton, hasNext);
        SetButtonInteractable(startGameButton, isLast);

        // 3) 텍스트 주입 (줄바꿈/HTML 정규화 포함)
        var s = slides[index] ?? new SlideDTO();
        string normTitle = NormalizeMultiline(s.title ?? "");
        string normBody  = NormalizeMultiline(s.content ?? "");

        InjectTexts(methodPages[index], normTitle, normBody);

        // 4) 영상 처리
        bool isVideo = !string.IsNullOrEmpty(s.media_type) && s.media_type.ToUpperInvariant() == "VIDEO";
        if (isVideo)
        {
            // 🎯 VideoDisplay 강제 활성화
            Transform currentPage = methodPages[index].transform;
            var rawImages = currentPage.GetComponentsInChildren<RawImage>(true);
            foreach (var img in rawImages)
            {
                if (img != null && img.name.ToLower().StartsWith("videodisplay"))
                {
                    img.gameObject.SetActive(true);
                    Debug.Log($"🔧 강제 VideoDisplay 활성화: {img.name}");
                }
            }

            // 🎯 VideoDisplay 안의 RawImage의 RenderTexture를 VideoPlayer에 다시 연결
            var rawImage = methodPages[index].GetComponentsInChildren<RawImage>(true)
                .FirstOrDefault(r => r != null && r.name.ToLower().StartsWith("videodisplay"));

            if (rawImage != null)
                videoController?.SetTargetTextureFromRawImage(rawImage);

            // ▶ 재생
            videoController?.PlayVideoFromPath(s.media_path);
        }
        else
        {
            // ⏹ 정지
            videoController?.StopVideo();
        }

        Debug.Log($"📄 {index + 1}/{slides.Length} | order={s.slide_order} | {s.title} | {s.media_type} | {s.media_path}");
    }

    // ------------------------------------------------------------
    // ▣ 텍스트 주입
    // ------------------------------------------------------------

    /// <summary>
    /// ✅ 텍스트 자동 주입
    /// - 이름 키워드(title/body/제목/내용/desc/description) 우선 매칭
    /// - 실패 시 첫 번째/두 번째 컴포넌트 폴백
    /// - WordWrapping, RichText 자동 옵션 세팅
    /// </summary>
    private void InjectTexts(GameObject page, string title, string body)
    {
        if (page == null) { Debug.LogWarning("[OBMethodPage] page is null"); return; }

        // 1) 모든 텍스트 컴포넌트 수집 (비활성 포함)
        var tmpAll = page.GetComponentsInChildren<TMP_Text>(true);
        var uiiAll = page.GetComponentsInChildren<Text>(true); // legacy Text

        // 2) 이름 키워드로 우선 매칭 (한/영 포함)
        System.Func<string, bool> isTitleName = n =>
        {
            var s = n.ToLower();
            return s.Contains("title") || s.Contains("\uC81C\uBAA9"); // "제목"
        };
        System.Func<string, bool> isBodyName = n =>
        {
            var s = n.ToLower();
            return s.Contains("body") || s.Contains("\uB0B4\uC6A9") || s.Contains("desc") || s.Contains("description"); // "내용"
        };

        TMP_Text tmpTitle = null, tmpBody = null;
        Text uiiTitle = null, uiiBody = null;

        foreach (var t in tmpAll)
        {
            if (t == null) continue;
            if (tmpTitle == null && isTitleName(t.name)) tmpTitle = t;
            if (tmpBody == null && isBodyName(t.name)) tmpBody = t;
        }
        foreach (var t in uiiAll)
        {
            if (t == null) continue;
            if (uiiTitle == null && isTitleName(t.name)) uiiTitle = t;
            if (uiiBody == null && isBodyName(t.name)) uiiBody = t;
        }

        // 3) 그래도 못 찾았으면 "첫 번째/두 번째 컴포넌트" 폴백
        if (tmpTitle == null && uiiTitle == null)
        {
            if (tmpAll.Length > 0) tmpTitle = tmpAll[0];
            else if (uiiAll.Length > 0) uiiTitle = uiiAll[0];
        }
        if (tmpBody == null && uiiBody == null)
        {
            if (tmpAll.Length > 1) tmpBody = tmpAll[1];
            else if (uiiAll.Length > 1) uiiBody = uiiAll[1];
        }

        // 4) 실제 텍스트 주입 + 상세 로그 (+ 옵션 세팅)
        if (tmpTitle != null)
        {
            tmpTitle.enableWordWrapping = enableWordWrapping;
            tmpTitle.richText = enableRichText;
            tmpTitle.text = title ?? "";
            Debug.Log($"[OBMethodPage] TMP Title <- \"{tmpTitle.name}\"");
        }
        else if (uiiTitle != null)
        {
            uiiTitle.supportRichText = enableRichText;
            uiiTitle.text = title ?? "";
            Debug.Log($"[OBMethodPage] UI Text Title <- \"{uiiTitle.name}\"");
        }
        else Debug.LogWarning("[OBMethodPage] 제목을 놓을 텍스트 컴포넌트를 못 찾음");

        if (tmpBody != null)
        {
            tmpBody.enableWordWrapping = enableWordWrapping;
            tmpBody.richText = enableRichText;
            tmpBody.text = body ?? "";
            Debug.Log($"[OBMethodPage] TMP Body <- \"{tmpBody.name}\"");
        }
        else if (uiiBody != null)
        {
            uiiBody.supportRichText = enableRichText;
            uiiBody.horizontalOverflow = HorizontalWrapMode.Wrap;   // 가독성 향상
            uiiBody.verticalOverflow   = VerticalWrapMode.Overflow; // 잘림 방지
            uiiBody.text = body ?? "";
            Debug.Log($"[OBMethodPage] UI Text Body <- \"{uiiBody.name}\"");
        }
        else Debug.LogWarning("[OBMethodPage] 본문을 놓을 텍스트 컴포넌트를 못 찾음");

        Debug.Log($"[OBMethodPage] Injected Title(len)={(title ?? "").Length} | Body(len)={(body ?? "").Length}");
    }

    // ------------------------------------------------------------
    // ▣ 유틸리티
    // ------------------------------------------------------------

    /// <summary>
    /// ✅ DB/JSON에서 넘어온 문자열의 줄바꿈/HTML 태그 등을
    ///    Unity가 이해하는 실제 줄바꿈으로 치환해주는 정규화 함수
    /// </summary>
    private static string NormalizeMultiline(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;

        string s = raw;

        // 1) JSON/DB에서 이스케이프된 "\\n" → 실제 개행 문자 '\n'
        //    (예: "첫째 줄\\n둘째 줄" 처럼 백슬래시가 살아있는 경우)
        s = s.Replace("\\n", "\n");

        // 2) 캐리지리턴 정리: 윈도우 스타일 "\r\n" → "\n"
        s = s.Replace("\r\n", "\n").Replace("\r", "\n");

        // 3) HTML 줄바꿈 태그 지원: <br>, <br/>, <br /> → '\n'
        //    (TMPro는 <br>도 이해하지만, 데이터에 섞여 있으면 통일해주는 편이 안전)
        s = s.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");

        // 4) HTML의 단락 태그 → 이중 개행(문단 구분)
        s = s.Replace("</p>", "\n\n").Replace("<p>", string.Empty);

        // 5) 탭/리스트 가독성 개선(필요 시 유지)
        s = s.Replace("\\t", "\t");     // 이스케이프된 탭을 실제 탭으로
        s = s.Replace("\n- ", "\n• ");  // 하이픈 리스트 → 불릿
        s = s.Replace("\n* ", "\n• ");  // 별표 리스트 → 불릿

        return s;
    }

    /// <summary>
    /// 버튼 GameObject에 Button 컴포넌트가 있으면 interactable 동기화
    /// </summary>
    private static void SetButtonInteractable(GameObject go, bool interactable)
    {
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn != null) btn.interactable = interactable;
    }
}
