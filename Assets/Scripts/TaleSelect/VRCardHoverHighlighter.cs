using UnityEngine;
using UnityEngine.UI;

public class VRCardHoverHighlighter : MonoBehaviour
{
    public Transform rayOrigin;               // 오른손 컨트롤러 (Ray 쏘는 위치)
    public float rayDistance = 10f;           // 레이 길이
    public LayerMask cardLayer;               // FairyTaleCard에만 닿도록 Layer 설정

    private GameObject currentCard = null;
    private Image currentImage = null;
    private Color originalColor;

    private void Update()
    {
        // 레이캐스트 쏘기
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, rayDistance, cardLayer))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj != currentCard)
            {
                // 이전 카드 원상복구
                ResetPreviousCard();

                // 새로운 카드 저장
                currentCard = hitObj;
                currentImage = currentCard.GetComponent<Image>();

                if (currentImage != null)
                {
                    originalColor = currentImage.color;
                    currentImage.color = new Color(0.7f, 0.8f, 1f); // 연한 파란색
                }
            }
        }
        else
        {
            // 카드에서 벗어난 경우 원상복구
            ResetPreviousCard();
        }
    }

    void ResetPreviousCard()
    {
        if (currentImage != null)
        {
            currentImage.color = originalColor;
        }
        currentCard = null;
        currentImage = null;
    }
}