using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 🧩 단일 설정 카드(UI)
/// - "기본 설정", "사운드 설정", "UI 설정" 등의 항목을 표시
/// - 클릭 시 SettingMenuManager에 선택된 key를 전달
/// </summary>
public class SettingCardUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("카드의 제목 텍스트 (예: 기본 설정, 사운드 설정 등)")]
    public TMP_Text titleText;

    [Tooltip("카드 왼쪽의 아이콘 이미지")]
    public Image iconImage;

    [Tooltip("카드 클릭 버튼")]
    public Button button;

    [Header("설정 구분 ID (Manager에서 지정)")]
    [Tooltip("각 설정 카테고리의 고유 키 (basic, sound, ui 등)")]
    public string settingKey;

    private SettingMenuManager menuManager;

    /// <summary>
    /// 초기화 메서드 (SettingMenuManager에서 생성 시 호출)
    /// </summary>
    public void Init(SettingMenuManager manager, string title, Sprite icon, string key)
    {
        menuManager = manager;
        settingKey = key;

        if (titleText != null)
            titleText.text = title;

        if (iconImage != null && icon != null)
            iconImage.sprite = icon;

        // 중복 리스너 방지 후 새로 등록
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    /// <summary>
    /// 카드 클릭 시 호출 (SettingMenuManager로 전달)
    /// </summary>
    private void OnClicked()
    {
        Debug.Log($"🟢 [SettingCardUI] 클릭됨 → {settingKey} ({titleText.text})");

        if (menuManager != null)
        {
            menuManager.OnCardSelected(settingKey);
        }
        else
        {
            Debug.LogWarning("⚠️ SettingMenuManager 참조가 설정되지 않았습니다!");
        }
    }
}