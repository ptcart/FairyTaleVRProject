using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VRUIButtonClicker : MonoBehaviour
{
    public OVRInput.Button selectButton = OVRInput.Button.One; // One 버튼
    public OVRInput.Controller controller = OVRInput.Controller.RTouch; // 오른손
    public LayerMask buttonLayer; // "UIButton" 같은 레이어로 버튼만 감지하게

    private GameObject currentHoverButton = null; // 현재 레이 맞은 버튼 저장
    private Color originalColor;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, 10f, buttonLayer);

        if (hitSomething)
        {
            GameObject hitButton = hit.collider.gameObject;

            if (currentHoverButton != hitButton)
            {
                ClearHover(); // 기존 Hover 초기화

                // 새로 Hover 시작
                currentHoverButton = hitButton;
                Image img = hitButton.GetComponent<Image>();
                if (img != null)
                {
                    originalColor = img.color;
                    img.color = Color.yellow; // ⭐ Highlight 색으로 변경
                }
            }

            if (OVRInput.GetDown(selectButton, controller))
            {
                Debug.Log("🟢 버튼 히트: " + hit.collider.gameObject.name);

                if (hit.collider.CompareTag("NextButton")) // ← Tag로 구분
                {
                    Button btn = hit.collider.GetComponent<Button>();
                    if (btn != null && !btn.interactable)
                    {
                        Debug.Log("🚫 버튼이 비활성화 상태입니다. 클릭 무시");
                        return;
                    }

                    Debug.Log("🚪 Intro 씬으로 이동 (페이드 아웃 시작)");
                    GlobalScreenFader.Instance.FadeAndLoadScene("NPCInteraction 1");
                }
            }
        }
        else
        {
            // Ray가 아무것도 안 맞으면 Hover 해제
            ClearHover();
        }
    }

    void ClearHover()
    {
        if (currentHoverButton != null)
        {
            Image img = currentHoverButton.GetComponent<Image>();
            if (img != null)
            {
                img.color = originalColor; // 원래 색 복구
            }
            currentHoverButton = null;
        }
    }
}
