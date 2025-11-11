using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ⚙️ SettingMenuManager
/// - 왼쪽 설정 카드를 자동 생성
/// - 카드 클릭 시 오른쪽 상세 설정 패널을 제어
/// </summary>
public class SettingMenuManager : MonoBehaviour
{
    [Header("프리팹 및 부모")]
    [Tooltip("왼쪽 설정 카드를 생성할 프리팹 (SettingCard)")]
    public GameObject settingCardPrefab; // SettingCard 프리팹
    [Tooltip("ScrollView > Viewport > Content 오브젝트")]
    public Transform contentParent;      // 카드들이 생성될 부모 (Content)

    [Header("아이콘 설정")]
    public Sprite basicIcon;  // ⚙️ 기본 설정 아이콘
    public Sprite soundIcon;  // 🔊 사운드 설정 아이콘
    public Sprite uiIcon;     // 🖥️ UI 설정 아이콘 (추후 업데이트 예정)

    [Header("오른쪽 상세 패널들")]
    public GameObject panelBasicSetting; // 기본 설정 패널
    public GameObject panelSoundSetting; // 사운드 설정 패널
    public GameObject panelUISetting;    // UI 설정 패널 (“준비 중” 문구용)

    // 내부용 구조체 (타이틀 + 아이콘 + 키)
    private struct SettingInfo
    {
        public string title;
        public Sprite icon;
        public string key;

        public SettingInfo(string title, Sprite icon, string key)
        {
            this.title = title;
            this.icon = icon;
            this.key = key;
        }
    }

    void Start()
    {
        // 왼쪽 카드 생성
        CreateSettingCards();

        // 초기 패널 표시 (기본 설정)
        ShowPanel("basic");
    }

    /// <summary>
    /// 왼쪽 설정 카드 목록을 동적으로 생성합니다.
    /// </summary>
    void CreateSettingCards()
    {
        SettingInfo[] settingData =
        {
            new SettingInfo("기본 설정", basicIcon, "basic"),
            //new SettingInfo("사운드 설정", soundIcon, "sound"),
            //new SettingInfo("UI 설정", uiIcon, "ui")
        };

        foreach (var data in settingData)
        {
            GameObject card = Instantiate(settingCardPrefab, contentParent);

            // SettingCardUI 컴포넌트 가져오기
            SettingCardUI cardUI = card.GetComponent<SettingCardUI>();
            if (cardUI != null)
            {
                cardUI.Init(this, data.title, data.icon, data.key);
            }
            else
            {
                Debug.LogWarning($"⚠️ {data.title} 카드에 SettingCardUI 컴포넌트가 없습니다!");
            }
        }
    }

    /// <summary>
    /// 카드 클릭 시 오른쪽 패널 전환 처리
    /// </summary>
    public void OnCardSelected(string key)
    {
        Debug.Log($"🟢 [SettingMenuManager] 선택된 설정: {key}");
        ShowPanel(key);
    }

    /// <summary>
    /// 오른쪽 상세 패널 표시 제어
    /// </summary>
    private void ShowPanel(string key)
    {
        // 모든 패널 비활성화
        if (panelBasicSetting != null) panelBasicSetting.SetActive(false);
        if (panelSoundSetting != null) panelSoundSetting.SetActive(false);
        if (panelUISetting != null) panelUISetting.SetActive(false);

        // 해당 패널만 활성화
        switch (key)
        {
            case "basic":
                panelBasicSetting?.SetActive(true);
                break;

            case "sound":
                panelSoundSetting?.SetActive(true);
                break;

            case "ui":
                panelUISetting?.SetActive(true);
                Debug.Log("⚙️ UI 설정은 추후 업데이트 예정입니다. (준비 중)");
                break;
        }
    }
}
