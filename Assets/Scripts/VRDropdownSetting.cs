using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class VRDropdownSetting : MonoBehaviour
{
    [Header("🎛 TMP 드롭다운 (UI Dropdown)")]
    public TMP_Dropdown dropdown;

    [Header("🎮 VR 클릭 버튼 (기본: A 버튼)")]
    public OVRInput.Button clickButton = OVRInput.Button.One; // 필요 시 트리거로 바꾸기

    private bool isPointerOver = false;

    void Start()
    {
        // 🔹 드롭다운 자동 참조
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        // 🔹 기존 옵션 초기화
        dropdown.ClearOptions();

        // 🔹 새 옵션 추가 (폰트 크기 선택)
        dropdown.AddOptions(new List<string> { "작게", "보통", "크게" });

        // 🔹 기본 선택값 (보통)
        dropdown.value = 1;
        dropdown.RefreshShownValue();
        dropdown.captionText.text = "보통";

        // 🔹 값 변경 이벤트 연결
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        Debug.Log("✅ VRDropdownSetting 초기화 완료");
    }

    void Update()
    {
        // 🔹 컨트롤러로 클릭할 때만 작동 (선택 메뉴 클릭)
        if (isPointerOver && OVRInput.GetDown(clickButton))
        {
            // 실제 TMP_Dropdown은 CustomOVRInputModule에 의해 처리되므로,
            // 여기는 “VR 위에서 클릭했음”만 표시용으로 사용 가능
            Debug.Log("🎮 VR 컨트롤러로 드롭다운 선택 시도");
        }
    }

    /// <summary>
    /// 드롭다운 값 변경 시 자동 호출
    /// </summary>
    private void OnDropdownValueChanged(int index)
    {
        string selected = dropdown.options[index].text;
        Debug.Log($"✅ 드롭다운 선택됨 → {selected}");

        // 🔹 라벨 갱신 (즉시 반영)
        dropdown.captionText.text = selected;

        // 🔹 이후 실제 반응 (폰트 크기 / UI 스케일 변경 등)
        switch (selected)
        {
            case "작게":
                SetUIFontScale(0.8f);
                break;
            case "보통":
                SetUIFontScale(1.0f);
                break;
            case "크게":
                SetUIFontScale(1.2f);
                break;
        }
    }

    /// <summary>
    /// 포인터가 드롭다운 위에 있을 때만 VR 입력 허용
    /// (EventTrigger에 연결하면 됨)
    /// </summary>
    public void OnPointerEnter() => isPointerOver = true;
    public void OnPointerExit() => isPointerOver = false;

    /// <summary>
    /// 실제 폰트 스케일 변경 예시 (원하면 지워도 됨)
    /// </summary>
    private void SetUIFontScale(float scale)
    {
        // 🔹 현재 Scene 내의 모든 TMP_Text 크기 변경 예시
        foreach (TMP_Text tmp in FindObjectsOfType<TMP_Text>())
        {
            tmp.fontSize = Mathf.RoundToInt(16 * scale); // 기본 16 기준
        }

        Debug.Log($"🔧 전체 UI 폰트 크기 조정됨 → 배율 {scale}");
    }
}
