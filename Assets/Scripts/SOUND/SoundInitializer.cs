using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 🎧 게임 시작 시, PlayerPrefs에 저장된 오디오 설정(BGM/SFX/Narration/NPC)을
/// AudioMixer에 전역 적용하는 초기화 클래스.
/// 씬 전환 후에도 유지(DontDestroyOnLoad)되며, 항상 1개만 존재.
/// </summary>
public class SoundInitializer : MonoBehaviour
{
    [Header("🎛️ 연결할 AudioMixer (SoundSettings와 동일해야 함)")]
    public AudioMixer masterMixer;

    private static bool initialized = false; // 중복 초기화 방지용

    void Awake()
    {
        // 🔹 중복 생성 방지
        if (FindObjectsOfType<SoundInitializer>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);  // ✅ 씬이 바뀌어도 유지

        // ✅ PlayerPrefs 기본값 세팅 (첫 실행 시만)
        InitializeDefaultPrefs();

        // 🔹 Mixer 즉시 반영
        ApplySavedAudioSettings();
    }

    void Start()
    {
        // 🔹 혹시 Awake() 타이밍이 너무 빨랐을 경우 한 번 더 반영
        if (!initialized)
        {
            ApplySavedAudioSettings();
        }
    }

    /// <summary>
    /// 🧩 PlayerPrefs 기본값 세팅 (최초 실행 시 한 번만)
    /// </summary>
    private void InitializeDefaultPrefs()
    {
        if (!PlayerPrefs.HasKey("Initialized"))
        {
            // 🔹 볼륨 기본값 = 0dB (정상 볼륨)
            PlayerPrefs.SetFloat("BGMVolume", 0f);
            PlayerPrefs.SetFloat("SFXVolume", 0f);

            // 🔹 음소거 OFF (즉, 소리 켜짐)
            PlayerPrefs.SetInt("BgmMuted", 0);
            PlayerPrefs.SetInt("SfxMuted", 0);

            // 🔹 NPC, 나레이션 켜짐 상태 (Mute=false)
            PlayerPrefs.SetInt("NpcMuted", 0);
            PlayerPrefs.SetInt("NarrationMuted", 0);

            PlayerPrefs.SetInt("Initialized", 1);
            PlayerPrefs.Save();

            Debug.Log("🔰 [SoundInitializer] PlayerPrefs 기본 오디오 설정 저장 완료");
        }
    }

    /// <summary>
    /// PlayerPrefs 값 기반으로 Mixer 세팅
    /// </summary>
    private void ApplySavedAudioSettings()
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("⚠️ [SoundInitializer] MasterMixer가 연결되어 있지 않습니다!");
            return;
        }

        // ✅ PlayerPrefs에서 값 읽기
        float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 0f);
        float savedSfx = PlayerPrefs.GetFloat("SFXVolume", 0f);
        bool isBgmMuted = PlayerPrefs.GetInt("BgmMuted", 0) == 1;
        bool isSfxMuted = PlayerPrefs.GetInt("SfxMuted", 0) == 1;
        bool isNarrationMuted = PlayerPrefs.GetInt("NarrationMuted", 0) == 1;
        bool isNpcMuted = PlayerPrefs.GetInt("NpcMuted", 0) == 1;

        // ✅ Mixer에 적용 (파라미터 이름 반드시 일치해야 함)
        bool success = true;
        success &= TrySetMixerVolume("BGMVolume", isBgmMuted ? -80f : savedBgm);
        success &= TrySetMixerVolume("SFXVolume", isSfxMuted ? -80f : savedSfx);
        success &= TrySetMixerVolume("NarrationVolume", isNarrationMuted ? -80f : 0f);
        success &= TrySetMixerVolume("NPCVolume", isNpcMuted ? -80f : 0f);

        initialized = true;

        if (success)
        {
            Debug.Log(
                $"✅ [SoundInitializer] 오디오 설정 전역 반영 완료\n" +
                $"BGM: {(isBgmMuted ? "Muted" : "On")} ({savedBgm}dB)\n" +
                $"SFX: {(isSfxMuted ? "Muted" : "On")} ({savedSfx}dB)\n" +
                $"Narration: {(isNarrationMuted ? "Muted" : "On")}\n" +
                $"NPC: {(isNpcMuted ? "Muted" : "On")}"
            );
        }
    }

    /// <summary>
    /// Mixer 파라미터 이름 검증 후 SetFloat 시도
    /// </summary>
    private bool TrySetMixerVolume(string parameter, float value)
    {
        if (!masterMixer.HasParameter(parameter))
        {
            Debug.LogWarning($"⚠️ Mixer에 '{parameter}' 파라미터가 없습니다. 이름을 확인하세요.");
            return false;
        }

        masterMixer.SetFloat(parameter, value);
        return true;
    }
}

/// <summary>
/// AudioMixer의 파라미터 존재 여부를 확인하는 확장 메서드
/// </summary>
public static class AudioMixerExtensions
{
    public static bool HasParameter(this AudioMixer mixer, string parameterName)
    {
        float temp;
        return mixer.GetFloat(parameterName, out temp); // 존재하지 않으면 false 반환
    }
}
