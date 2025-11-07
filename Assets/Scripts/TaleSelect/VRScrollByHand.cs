using UnityEngine;
using UnityEngine.UI;

public class VRScrollByHand : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Transform rightHandTransform; // 오른손 컨트롤러 트랜스폼
    public float scrollSensitivity = 0.5f; // 스크롤 감도

    private float lastY;
    private bool isGrabbing = false;

    void Update()
    {
        // 트리거를 누르기 시작했을 때: 기준점 저장
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            lastY = rightHandTransform.position.y;
            isGrabbing = true;
        }

        // 트리거에서 손을 뗐을 때: 중단
        if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            isGrabbing = false;
        }

        // 누르고 있는 중 → 컨트롤러 위치 변화 감지
        if (isGrabbing)
        {
            float currentY = rightHandTransform.position.y;
            float deltaY = currentY - lastY;

            // 위로 올리면 scrollRect 위로, 아래로 내리면 아래로
            scrollRect.verticalNormalizedPosition += deltaY * scrollSensitivity;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);

            lastY = currentY;
        }
    }
}