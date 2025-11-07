using UnityEngine;
using UnityEngine.UI;

public class FairyTaleCardUI : MonoBehaviour
{
    public FairyTaleUIManager.FairyTale fairyTaleData; // 연결된 동화 정보
    public RectTransform scrollViewport;               // ScrollView 내 Viewport
    public Camera uiCamera;                      // UI용 카메라 (World Space Canvas에 할당)

    public GameObject outlineObject; // ⭐ 테두리용 오브젝트
    public Image thumbnailImage;   // ⭐ 추가: 썸네일용 Image
    // 현재는 VRCardHighlighter.cs가 직접 처리하므로 색상 관련은 필요 없음
    // 필요한 경우에 대비해 Image 캐싱만 유지
    [HideInInspector]
    public Image bg;

    void Awake()
    {
        // 카드 배경 이미지 자동 할당 (없을 경우 자식에서도 탐색)
        bg = GetComponent<Image>();
        if (bg == null)
        {
            bg = GetComponentInChildren<Image>();
        }
    }

    // Viewport 내부에 있는지 확인 (다른 로직에서 사용 중이라면 유지)
    public bool IsInViewport()
    {
        if (scrollViewport == null || uiCamera == null)
        {
            Debug.LogWarning("⚠️ Viewport 또는 UICamera가 설정되지 않음");
            return true; // 못 판단할 땐 일단 true로 처리
        }

        Vector3 screenPos = uiCamera.WorldToScreenPoint(transform.position);
        return RectTransformUtility.RectangleContainsScreenPoint(scrollViewport, screenPos, uiCamera);
    }
    
    public void SetSelected(bool selected)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(selected);
        }
        if (selected)
        {
            Debug.Log("✅ 카드 선택됨: ");
        }
    }

}