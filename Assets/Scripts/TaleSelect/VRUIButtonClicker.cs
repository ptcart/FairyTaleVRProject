using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 🎮 VRUIButtonClicker
/// - 컨트롤러로 UI 버튼을 가리키고 클릭 가능
/// - NextButton: 새 게임 시작
/// - ContinueButton: 저장된 씬으로 이어하기
/// - BackButton: 이전 씬(메인 메뉴 등)으로 돌아가기
/// </summary>
public class VRUIButtonClicker : MonoBehaviour
{
    [Header("🎯 VR 입력 설정")]
    public OVRInput.Button selectButton = OVRInput.Button.One; // 오른손 A 버튼
    public OVRInput.Controller controller = OVRInput.Controller.RTouch; // 오른손
    public LayerMask buttonLayer; // UI 버튼 전용 레이어

    private GameObject currentHoverButton = null;
    private Color originalColor;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, 10f, buttonLayer);

        if (hitSomething)
        {
            GameObject hitButton = hit.collider.gameObject;

            // 🔸 Hover 색상 처리
            if (currentHoverButton != hitButton)
            {
                ClearHover(); // 이전 버튼 복구
                currentHoverButton = hitButton;

                Image img = hitButton.GetComponent<Image>();
                if (img != null)
                {
                    originalColor = img.color;
                    img.color = new Color(0.9f, 0.9f, 0.85f);
                }
            }

            // 🔹 클릭 감지
            if (OVRInput.GetDown(selectButton, controller))
            {
                Button btn = hit.collider.GetComponent<Button>();
                if (btn == null)
                {
                    Debug.LogWarning($"⚠️ 클릭된 오브젝트 {hit.collider.name}에 Button 컴포넌트가 없습니다.");
                    return;
                }

                if (!btn.interactable)
                {
                    Debug.Log("🚫 버튼이 비활성화 상태입니다. 클릭 무시");
                    return;
                }

                Debug.Log($"🟢 VR 버튼 클릭됨: {hit.collider.name} (Tag: {hit.collider.tag})");

                // =====================================
                // 1️⃣ NextButton → 새 게임 시작
                // =====================================
                if (hit.collider.CompareTag("NextButton"))
                {
                    GameDataManager.nextStoryIdToLoad = 0;
                    Debug.Log("🧭 새 게임 시작 → StoryID 초기화 (0으로 설정)");
                    GlobalScreenFader.Instance.FadeAndLoadScene("NPCInteraction 1");
                }

                // =====================================
                // 2️⃣ ContinueButton → 이어하기
                // =====================================
                else if (hit.collider.CompareTag("ContinueButton"))
                {
                    if (!SaveManager.HasSaveData())
                    {
                        Debug.Log("⚫ 저장된 데이터가 없습니다 → 이어하기 불가");
                        return;
                    }

                    string savedScene = SaveManager.LoadSavedScene();
                    if (!string.IsNullOrEmpty(savedScene))
                    {
                        Debug.Log($"🔄 이어하기 → '{savedScene}' 씬 로드 중...");
                        GlobalScreenFader.Instance.FadeAndLoadScene(savedScene);
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ 저장된 씬 정보를 불러올 수 없습니다.");
                    }
                }

                // =====================================
                // 3️⃣ BackButton → 이전 씬으로 돌아가기
                // =====================================
                else if (hit.collider.CompareTag("BackButton"))
                {
                    string previousScene = "MainVRScene"; // 🧭 기본적으로 메인 메뉴 씬 이름
                    Debug.Log($"↩️ [VRUIButtonClicker] 뒤로가기 → '{previousScene}' 로 전환");
                    SceneManager.LoadScene(previousScene);
                    //GlobalScreenFader.Instance.FadeAndLoadScene(previousScene);
                }
            }
        }
        else
        {
            ClearHover();
        }
    }

    void ClearHover()
    {
        if (currentHoverButton != null)
        {
            Image img = currentHoverButton.GetComponent<Image>();
            if (img != null)
                img.color = originalColor;

            currentHoverButton = null;
        }
    }
}
