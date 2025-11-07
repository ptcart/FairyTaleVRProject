using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 🎬 FairyTaleEndingUIManager (최종 수정 버전)
/// - 왼쪽 동화 카드 클릭 → 오른쪽에 엔딩 버튼 3×2 표시
/// - 위: 엔딩 번호 / 아래: 엔딩 이름
/// - Flask DB의 is_cleared 로 잠금/해금 표현
/// </summary>
public class FairyTaleEndingUIManager : MonoBehaviour
{
    // ---------- 데이터 모델 ----------
    [System.Serializable]
    public class FairyTale
    {
        public int fairy_tale_id;
        public string title;
        public string summary;
        public int total_endings;
    }

    [System.Serializable]
    public class EndingData
    {
        public int ending_id;
        public int fairy_tale_id;
        public string ending_name;
        public bool is_cleared;
    }

    // ---------- 에디터 연결 ----------
    [Header("UI 연결")]
    public Transform contentParent;
    public GameObject taleCardPrefab;
    public Transform endingContentParent;
    public GameObject endingButtonPrefab;
    public RectTransform viewport;

    // ---------- 내부 상태 ----------
    private readonly List<FairyTale> tales = new List<FairyTale>();
    private FairyTale selectedTale;
    private readonly List<GameObject> spawnedEndingButtons = new List<GameObject>();
    private FairyTaleEndingCardUI selectedCard;

    void Start()
    {
        StartCoroutine(LoadFairyTales());
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
            Debug.Log("✅ A 버튼 눌림 (OVRInput 감지됨)");
    }

    // 🧩 [1] 동화 목록 로드
    IEnumerator LoadFairyTales()
    {
        string json = "{\"command\":\"fairytale_list\"}";

        UnityWebRequest request = new UnityWebRequest("http://localhost:5000/command", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            FairyTale[] result = JsonHelper.FromJson<FairyTale>(request.downloadHandler.text);
            if (result != null) tales.AddRange(result);
            DisplayTaleCards();
        }
        else
        {
            Debug.LogError("❌ 서버 통신 실패: " + request.error);
        }
    }

    // 🧩 [2] 동화 카드 표시
    void DisplayTaleCards()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        foreach (FairyTale tale in tales)
        {
            GameObject card = Instantiate(taleCardPrefab, contentParent);
            var cardUI = card.GetComponent<FairyTaleEndingCardUI>();

            if (cardUI == null)
            {
                Debug.LogError($"❌ FairyTaleEndingCardUI 누락: {card.name}");
                continue;
            }

            cardUI.Setup(tale.title, false);

            Button btn = card.GetComponent<Button>();
            if (btn != null)
            {
                FairyTale capturedTale = tale;
                FairyTaleEndingCardUI capturedCard = cardUI;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"🎯 카드 클릭: {capturedTale.title}");
                    OnTaleSelected(capturedTale, capturedCard);
                });
            }
        }
    }

    // 🧩 [3] 동화 선택 시 → 엔딩 상태 로드
    void OnTaleSelected(FairyTale tale, FairyTaleEndingCardUI clickedCard)
    {
        Debug.Log($"✅ '{tale.title}' 선택됨 (ID: {tale.fairy_tale_id})");

        selectedTale = tale;

        if (selectedCard != null && selectedCard != clickedCard)
            selectedCard.SetStatusIcon(false);

        selectedCard = clickedCard;
        selectedCard.SetStatusIcon(true);

        ClearEndings();
        StartCoroutine(LoadEndingStatus(tale));
    }

    // 🧩 [4] 해당 동화의 엔딩들 불러와서 버튼 생성
    IEnumerator LoadEndingStatus(FairyTale tale)
    {
        string json = $"{{\"command\":\"ending_list\", \"payload\":{{\"fairy_tale_id\":{tale.fairy_tale_id}}}}}";

        UnityWebRequest req = new UnityWebRequest("http://localhost:5000/command", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ 엔딩 목록 요청 실패: {req.error}");
            yield break;
        }

        string response = req.downloadHandler.text;
        Debug.Log($"📥 엔딩 데이터 수신: {response}");

        EndingData[] endings = JsonHelper.FromJson<EndingData>(response);

        if (endings == null || endings.Length == 0)
        {
            Debug.LogWarning($"⚠️ {tale.title}에 대한 엔딩 데이터가 없습니다.");
            yield break;
        }

        System.Array.Sort(endings, (a, b) => a.ending_id.CompareTo(b.ending_id));

        int localNo = 1;

        foreach (var ending in endings)
        {
            GameObject btnGO = Instantiate(endingButtonPrefab, endingContentParent);

            var endingName  = btnGO.transform.Find("EndingName")?.GetComponent<TextMeshProUGUI>();
            var endingTitle = btnGO.transform.Find("EndingTitle")?.GetComponent<TextMeshProUGUI>();

            string endingNumText = $"엔딩 {localNo}";
            string titleText;
            Color titleColor;

            if (ending.is_cleared)
            {
                titleText  = string.IsNullOrEmpty(ending.ending_name) ? "엔딩 이름 없음" : ending.ending_name;
                titleColor = new Color(0.43f, 0.91f, 0.65f); // 연두색
            }
            else
            {
                titleText  = "잠김";
                titleColor = new Color(0.6f, 0.6f, 0.6f); // 회색
            }

            // ✅ 위쪽: 엔딩 번호 (EndingTitle)
            if (endingTitle != null)
            {
                endingTitle.text = endingNumText;
                endingTitle.color = titleColor;
                endingTitle.alignment = TextAlignmentOptions.Center;
                endingTitle.gameObject.SetActive(true);
            }

            // ✅ 아래쪽: 엔딩 이름 (EndingName)
            if (endingName != null)
            {
                endingName.text = titleText;
                endingName.color = titleColor;
                endingName.alignment = TextAlignmentOptions.Center;
                endingName.gameObject.SetActive(true);
            }

            // 🔗 클릭 이벤트
            int globalId   = ending.ending_id;
            int displayNo  = localNo;
            bool cleared   = ending.is_cleared;

            Button button = btnGO.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    if (cleared)
                        OnEndingSelected(tale.title, displayNo, globalId);
                    else
                        Debug.Log($"🔒 {tale.title} - 엔딩 {displayNo}은 아직 잠겨 있습니다!");
                });
            }

            spawnedEndingButtons.Add(btnGO);
            localNo++;
        }

        Debug.Log($"✅ {tale.title}의 엔딩 {endings.Length}개 생성 완료 (표시 1~{endings.Length})");
    }

    // 🧩 [5] 엔딩 선택 시 (전역 ID 전달)
    void OnEndingSelected(string fairyTitle, int displayNo, int endingIdGlobal)
    {
        Debug.Log($"🌙 {fairyTitle} - 엔딩 {displayNo} 선택 (globalId={endingIdGlobal})");
        // SceneManager.LoadScene("EndingScene_" + endingIdGlobal);
    }

    // 🧩 [6] 엔딩 버튼 정리
    void ClearEndings()
    {
        foreach (var obj in spawnedEndingButtons)
            if (obj) Destroy(obj);
        spawnedEndingButtons.Clear();
    }
}
