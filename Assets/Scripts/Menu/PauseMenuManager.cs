using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("🎛️ 일시정지 메뉴 오브젝트 (Canvas_PauseMenu)")]
    public GameObject pauseMenuUI;

    [Header("🌫 블러 배경 (blurBackground, 선택 사항)")]
    public GameObject blurBackground;

    [Header("🎮 일시정지 토글용 버튼 (예: Start, One, Two 등)")]
    public OVRInput.Button pauseButton = OVRInput.Button.Two; // ▶ 기본값: 오른손 B버튼

    [Header("📷 VR 카메라 기준 (CenterEyeAnchor)")]
    public Transform centerEyeAnchor;

    [Header("📏 메뉴 표시 거리 (기본 1m)")]
    public float menuDistance = 1.0f;

    [Header("📐 메뉴 높이 오프셋 (위/아래 미세조정)")]
    public float heightOffset = -0.05f;

    private bool isPaused = false;
    private Transform originalParent;

    void Start()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogWarning("⚠️ [PauseMenuManager] pauseMenuUI가 비어있습니다. Canvas_PauseMenu를 연결하세요.");
            return;
        }

        if (centerEyeAnchor == null)
        {
            Debug.LogWarning("⚠️ [PauseMenuManager] centerEyeAnchor가 비어있습니다. OVRCameraRig의 CenterEyeAnchor를 연결하세요.");
        }

        // 처음엔 비활성화
        pauseMenuUI.SetActive(false);
        if (blurBackground != null)
            blurBackground.SetActive(false);

        Debug.Log("✅ [PauseMenuManager] 초기화 완료");
        Debug.Log($"🎮 현재 Pause 버튼 설정: {pauseButton}");
    }

    void Update()
    {
        // 🔹 테스트용 입력 (키보드)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("⌨️ [테스트] P 키 눌림 → 메뉴 토글 실행");
            TogglePauseMenu();
        }

        // 🔹 OVR 입력 감지
        if (OVRInput.GetDown(pauseButton))
        {
            Debug.Log($"🕹️ Pause 버튼 '{pauseButton}' 감지됨 → 메뉴 토글 실행");
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuUI == null || centerEyeAnchor == null)
        {
            Debug.LogError("❌ PauseMenuManager: UI 또는 카메라 앵커가 연결되지 않았습니다.");
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
            ShowPauseMenu();
        else
            HidePauseMenu();
    }

    private void ShowPauseMenu()
    {
        Debug.Log("⏸️ 일시정지 메뉴 표시 중...");

        Vector3 headPos = centerEyeAnchor.position;
        Quaternion headRot = centerEyeAnchor.rotation;

        // 📍 시선 정면 방향 + 거리 + 높이 오프셋 적용
        Vector3 targetPos = headPos + headRot * Vector3.forward * menuDistance;
        targetPos.y += heightOffset;

        Quaternion targetRot = Quaternion.Euler(0, headRot.eulerAngles.y, 0);

        // 🔹 부모 분리 전에 정확히 위치 지정
        originalParent = pauseMenuUI.transform.parent;
        pauseMenuUI.transform.SetParent(null, true);
        pauseMenuUI.transform.position = targetPos;
        pauseMenuUI.transform.rotation = targetRot;

        // 🔹 UI 활성화
        pauseMenuUI.SetActive(true);
        if (blurBackground != null)
            blurBackground.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log($"📍 메뉴 위치 고정 완료 → Pos: {pauseMenuUI.transform.position}, Rot: {pauseMenuUI.transform.rotation.eulerAngles}");
    }

    private void HidePauseMenu()
    {
        Debug.Log("▶️ 일시정지 해제, 게임 재개");

        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);

        if (blurBackground != null)
            blurBackground.SetActive(false);

        // 다시 원래 부모로 복구
        if (originalParent != null)
            pauseMenuUI.transform.SetParent(originalParent);
    }

    // ▶ 계속하기 버튼
    public void OnContinueClicked()
    {
        Debug.Log("🔵 [PauseMenu] '계속하기' 버튼 클릭됨");
        TogglePauseMenu();
    }

    // 🚪 저장하고 나가기 버튼
    public void OnSaveAndExitClicked()
    {
        Debug.Log("🔴 [PauseMenu] '저장하고 나가기' 버튼 클릭됨");

        Time.timeScale = 1f;

        try
        {
            SaveManager.SaveCurrentScene(); // ✅ 자동 저장 시도
            Debug.Log("💾 저장 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"⚠️ 저장 중 오류 발생: {ex.Message}");
        }

        SceneManager.LoadScene("MainVRScene");
        Debug.Log("🌙 FairyTaleSelectScene 로드 중...");
    }
}
