    // 기존 using 구문 + 추가
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;  // 씬 전환용

public class FairyTaleUIManager : MonoBehaviour
{
    [System.Serializable]
    public class FairyTale
    {
        public int fairy_tale_id;
        public string title;
        public string summary;
        public string preview_image_path;
        
    }

    public Button nextButton; // 👈 인스펙터에서 연결
    public Transform contentParent; // ScrollView → Content
    public GameObject cardPrefab;   // 새로 만든 카드 프리팹
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI summaryText;
    public Image previewImage;  // ⭐ 상세창에 표시할 썸네일 이미지
    public List<GameObject> detailPanelObjects;  // ⭐ DetailPanel 안 요소들 리스트
    
    public RectTransform viewport;  // Viewport 참조 (외부에서 드래그해줘야 함)

    private List<FairyTale> tales = new List<FairyTale>();
    

    void Start()
    {
        StartCoroutine(LoadFairyTales());
        if (nextButton != null)
            nextButton.interactable = false; // 또는 nextButton.gameObject.SetActive(false);
        
        // ⭐ 처음에 DetailPanel 안 모든 오브젝트 꺼주기
        if (detailPanelObjects != null)
        {
            foreach (var obj in detailPanelObjects)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }

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
            string result = request.downloadHandler.text;
            FairyTale[] fairyTales = JsonHelper.FromJson<FairyTale>(result);
            tales.AddRange(fairyTales);
            DisplayTaleButtons();
        }
        else
        {
            Debug.LogError("❌ Server error: " + request.error);
        }
    }

    private FairyTaleCardUI selectedCard;
    void DisplayTaleButtons()
    {
        Debug.Log("📦 동화 총 개수: " + tales.Count);

        foreach (FairyTale tale in tales)
        {
            GameObject card = Instantiate(cardPrefab, contentParent);
            var cardUI = card.GetComponent<FairyTaleCardUI>();
            Debug.Log($"🧩 카드 생성됨. {card.name}");
            
            if (cardUI != null)
            {
                cardUI.fairyTaleData = tale;
                cardUI.scrollViewport = viewport;
                cardUI.uiCamera = Camera.main;

                // Button 클릭 이벤트 연결
                Button btn = card.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() =>
                    {
                        OnCardSelected(cardUI);
                    });
                }
            }
            

            var titleObj = card.transform.Find("TitleText");
            var summaryObj = card.transform.Find("SummaryText");

            if (titleObj == null || summaryObj == null)
            {
                Debug.LogError($"❌ 텍스트 오브젝트 누락! 카드 이름: {card.name}");
                foreach (Transform child in card.GetComponentsInChildren<Transform>())
                {
                    Debug.Log("  └ " + child.name);
                }
                continue;
            }

            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            var summaryText = summaryObj.GetComponent<TextMeshProUGUI>();

            if (titleText == null || summaryText == null)
            {
                Debug.LogError("❌ TextMeshProUGUI 컴포넌트가 빠졌습니다.");
                continue;
            }

            // 카드 텍스트 설정
            titleText.text = tale.title;
            summaryText.text = tale.summary;

            // 카드 정보 연결
            FairyTaleCardUI cardUIScript = card.GetComponent<FairyTaleCardUI>();
            if (cardUIScript != null)
            {
                cardUIScript.fairyTaleData = tale;
                cardUIScript.scrollViewport = viewport;  // Viewport 정보 전달
                cardUIScript.uiCamera = Camera.main; // UI 카메라 전달
            }
            // ✅ 썸네일 이미지 세팅 추가
            if (!string.IsNullOrEmpty(tale.preview_image_path))
            {
                string path = tale.preview_image_path;

                // ⭐ 확장자 제거 (.jpg, .png 등)
                if (path.EndsWith(".jpg"))
                {
                    path = path.Replace(".jpg", "");
                }
                else if (path.EndsWith(".png"))
                {
                    path = path.Replace(".png", "");
                }

                Sprite thumbnailSprite = Resources.Load<Sprite>(path); // 확장자 없는 상태로 로드
                if (thumbnailSprite != null && cardUIScript.thumbnailImage != null)
                {
                    cardUIScript.thumbnailImage.sprite = thumbnailSprite;
                }
                else
                {
                    Debug.LogWarning($"❗ 썸네일 로딩 실패 또는 thumbnailImage 연결 누락: {path}");

                    // ✅ 여기 추가: 기본 Default 이미지 적용
                    Sprite defaultSprite = Resources.Load<Sprite>("images/default_thumbnail");
                    if (defaultSprite != null)
                    {
                        cardUIScript.thumbnailImage.sprite = defaultSprite;
                    }
                    else
                    {
                        Debug.LogError("❌ 기본(Default) 썸네일 이미지도 로딩 실패했어요!");
                    }
                }
            }


            Debug.Log("✅ 카드 생성 완료: " + tale.title);
        }
    }

    // 카드 클릭 시 상세정보 표시
    public void ShowDetails(FairyTale tale)
    {
        if (titleText != null) titleText.text = tale.title;
        if (summaryText != null) summaryText.text = tale.summary;
    }
    
    public void OnCardSelected(FairyTaleCardUI cardUI)
    {
        if (!cardUI.IsInViewport()) return;

        // 기존 선택 해제
        if (selectedCard != null && selectedCard != cardUI)
            selectedCard.SetSelected(false);

        // 새로 선택
        selectedCard = cardUI;
        selectedCard.SetSelected(true);
        
        if (nextButton != null)
            nextButton.interactable = true; // 또는 nextButton.gameObject.SetActive(true);
        
        // ⭐ 카드 클릭 시 DetailPanel 오브젝트 켜주기
        if (detailPanelObjects != null)
        {
            foreach (var obj in detailPanelObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
        
        // ✅ 썸네일 이미지 복사 추가
        if (previewImage != null && cardUI.thumbnailImage != null)
        {
            previewImage.sprite = cardUI.thumbnailImage.sprite;
        }

        // ✅ 로그 출력
        Debug.Log("✅ 카드 선택됨: " + cardUI.fairyTaleData.title);

        ShowDetails(cardUI.fairyTaleData);
    }
    
    public void GoToNextScene()
    {
        SceneManager.LoadScene("NPCInteraction"); // ← 너가 만든 다음 씬 이름으로 변경!
    }

}
