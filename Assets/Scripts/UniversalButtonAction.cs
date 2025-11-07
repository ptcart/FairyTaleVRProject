using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 🎮 EndingButtonActionPlus
/// - UI 클릭 또는 (버튼 위에 포인터/선택된 상태에서) VR A버튼 입력 지원
/// - 클릭 사운드 → 딜레이 → 씬 전환
/// - 네가 쓰던 EndingButtonAction과 사용법 동일 + 옵션만 추가
/// </summary>
[RequireComponent(typeof(Button))]
public class UniversalButtonAction : MonoBehaviour, IButtonAction,
    IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("🎯 전환할 씬 이름")]
    [SerializeField] private string sceneToLoad = "FairyTaleSelectionScene";

    [Header("⏱ 전환 전 연출")]
    [SerializeField] private float delay = 0.3f;      // 사운드 후 대기
    [SerializeField] private AudioClip clickSound;    // 클릭 효과음
    [SerializeField] private float volume = 1.0f;

    [Header("🎮 VR 입력 옵션")]
    [SerializeField] private bool enableVRInput = true;
    [SerializeField] private bool requireHoverOrSelect = true; // 버튼 위 or 선택 상태일 때만 A 허용

    private Button uiButton;
    private bool isHovered = false;
    private bool isSelected = false;
    private bool isLoading = false;        // 중복 입력 방지

    void Awake()
    {
        uiButton = GetComponent<Button>();
    }

    void OnEnable()
    {
        if (uiButton != null)
        {
            uiButton.onClick.RemoveAllListeners();
            uiButton.onClick.AddListener(OnButtonClick);
        }
        ResetStates();
    }

    void Update()
    {
        if (!enableVRInput || isLoading) return;
        if (!OVRInput.GetDown(OVRInput.Button.One)) return;
        if (uiButton != null && !uiButton.interactable) return;

        if (requireHoverOrSelect && !(isHovered || isSelected)) return;

        // A 버튼으로도 클릭 실행
        OnButtonClick();
    }

    public void OnButtonClick()
    {
        if (isLoading) return;

        Debug.Log("🎬 엔딩 모음 버튼 실행!");
        PlayClickSound();
        StartCoroutine(LoadSceneWithDelay());
    }

    private void PlayClickSound()
    {
        if (clickSound == null) return;

        // 메인 카메라가 없을 수 있으므로 안전 처리
        var cam = Camera.main != null ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clickSound, cam, volume);
    }

    private IEnumerator LoadSceneWithDelay()
    {
        isLoading = true;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"✅ 씬 전환: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("⚠ 전환할 씬 이름이 비어 있습니다. 인스펙터에서 설정하세요.");
            isLoading = false; // 전환 실패 시 다시 입력 허용
        }
    }

    // ---- 포인터/선택 상태 콜백 ----
    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData)  => isHovered = false;
    public void OnSelect(BaseEventData eventData)          => isSelected = true;
    public void OnDeselect(BaseEventData eventData)        => isSelected = false;

    void OnDisable() => ResetStates();

    private void ResetStates()
    {
        isHovered = false;
        isSelected = false;
        isLoading = false;
    }

    // (선택) 코드로 씬 이름 바꾸고 싶을 때 사용
    public void SetScene(string sceneName) => sceneToLoad = sceneName;
}
