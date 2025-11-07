using UnityEngine;
using UnityEngine.UI;

public class VRSettingUIManager : MonoBehaviour
{
    [Header("패널 내부의 모든 슬라이더 & 토글 자동 감지")]
    public Slider[] sliders;
    public Toggle[] toggles;

    void Start()
    {
        // 자식들 중에서 Slider와 Toggle 자동 검색
        sliders = GetComponentsInChildren<Slider>(true);
        toggles = GetComponentsInChildren<Toggle>(true);

        // 각각 이벤트 연결
        foreach (Slider slider in sliders)
        {
            slider.onValueChanged.AddListener(value => OnSliderChanged(slider, value));
        }

        foreach (Toggle toggle in toggles)
        {
            toggle.onValueChanged.AddListener(isOn => OnToggleChanged(toggle, isOn));
        }

        Debug.Log($"🔍 감지된 슬라이더: {sliders.Length}, 토글: {toggles.Length}");
    }

    private void OnSliderChanged(Slider slider, float value)
    {
        Debug.Log($"🎚️ [슬라이더 감지됨] {slider.name} → {value}");
        
        string sliderName = slider.gameObject.name;
        Debug.Log($"🎚️ [{sliderName}] 값 변경 → {value}");

        // 필요시 이름 기준으로 기능 분리
        if (sliderName.Contains("BGM"))
        {
            // 배경음 볼륨 조절 코드 추가
        }
        else if (sliderName.Contains("SFX"))
        {
            // 효과음 볼륨 조절 코드 추가
        }
        else if (sliderName.Contains("NPC"))
        {
            // NPC 음성 볼륨 조절 코드 추가
        }
    }

    private void OnToggleChanged(Toggle toggle, bool isOn)
    {
        string toggleName = toggle.gameObject.name;
        Debug.Log($"✅ [{toggleName}] 상태 변경 → {(isOn ? "ON" : "OFF")}");

        // 필요시 이름 기준으로 기능 분리
        if (toggleName.Contains("BGM"))
        {
            // 배경음 음소거 코드 추가
        }
        else if (toggleName.Contains("SFX"))
        {
            // 효과음 음소거 코드 추가
        }
        else if (toggleName.Contains("NPC"))
        {
            // NPC 음성 음소거 코드 추가
        }
    }
}