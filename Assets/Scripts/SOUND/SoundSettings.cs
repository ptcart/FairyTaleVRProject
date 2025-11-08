using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using System.Collections;

public class SoundSettings : MonoBehaviour
{
    [Header("🎛️ AudioMixer 연결")]
    public AudioMixer masterMixer;

    [Header("🎵 배경음 (BGM)")]
    public Slider bgmSlider;
    public Toggle bgmMuteToggle;

    [Header("🔊 효과음 (SFX)")]
    public Slider sfxSlider;
    public Toggle sfxMuteToggle;

    [Header("🗣️ NPC 음성 켜기")]
    public Toggle npcVoiceToggle;

    [Header("📖 나레이션 음성 켜기")]
    public Toggle narrationVoiceToggle;

    [Header("🪄 테스트 클릭 사운드")]
    public AudioClip testClickSound;
    public float testClickVolume = 1f;

    private float lastBgmVolume = 0f;
    private float lastSfxVolume = 0f;
    private bool uiReady = false;

    // ===============================================================
    // 🔹 Awake(): PlayerPrefs 먼저 준비
    // ===============================================================
    private void Awake()
    {
        InitializeDefaultSettings(); // PlayerPrefs 값 세팅
        Debug.Log("🟡 [SoundSettings] Awake() 실행됨");
    }

    // ===============================================================
    // 🔹 Start(): UI 준비 이후 1프레임 기다리고 반영
    // ===============================================================
    private IEnumerator Start()
    {
        yield return null; // 한 프레임 대기 → UI 생성 완료 후
        Debug.Log("🟡 [SoundSettings] Start() 실행됨 - 설정 적용 시도");
        ResetToDefaultIfFirstTime();  // ✅ 처음 실행 시만 기본값 적용
        ApplySavedSettings();
        RegisterListeners();
        uiReady = true;
        Debug.Log("✅ [SoundSettings] UI 초기화 및 설정 반영 완료");
    }

    // ===============================================================
    // 🎚 기본값 세팅 (최초 실행 시만)
    // ===============================================================
    private void InitializeDefaultSettings()
    {
        if (!PlayerPrefs.HasKey("SoundInitialized"))
        {
            PlayerPrefs.SetFloat("BGMVolume", 0f);
            PlayerPrefs.SetFloat("SFXVolume", 0f);
            PlayerPrefs.SetInt("BgmMuted", 0);
            PlayerPrefs.SetInt("SfxMuted", 0);
            PlayerPrefs.SetInt("NpcMuted", 0);
            PlayerPrefs.SetInt("NarrationMuted", 0);
            PlayerPrefs.SetInt("SoundInitialized", 1);
            PlayerPrefs.Save();
            Debug.Log("🔰 [SoundSettings] 첫 실행 기본 오디오 설정 초기화 완료");
        }
    }

    // ===============================================================
    // 🎛 PlayerPrefs 불러와서 Mixer + UI에 반영
    // ===============================================================
    private void ApplySavedSettings()
    {
        float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 0f);
        float savedSfx = PlayerPrefs.GetFloat("SFXVolume", 0f);
        bool isBgmMuted = PlayerPrefs.GetInt("BgmMuted", 0) == 1;
        bool isSfxMuted = PlayerPrefs.GetInt("SfxMuted", 0) == 1;
        bool isNpcMuted = PlayerPrefs.GetInt("NpcMuted", 0) == 1;
        bool isNarrationMuted = PlayerPrefs.GetInt("NarrationMuted", 0) == 1;

        // ✅ Mixer 반영
        masterMixer.SetFloat("BGMVolume", isBgmMuted ? -80f : savedBgm);
        masterMixer.SetFloat("SFXVolume", isSfxMuted ? -80f : savedSfx);
        masterMixer.SetFloat("NPCVolume", isNpcMuted ? -80f : 0f);
        masterMixer.SetFloat("NarrationVolume", isNarrationMuted ? -80f : 0f);

        // ✅ 슬라이더 기본값
        // bgmSlider.value = 1f;
        // sfxSlider.value = 1f;
        // ✅ 슬라이더 값을 PlayerPrefs에서 불러온 볼륨으로 반영 (dB → 0~1)
        float bgmValueNormalized = Mathf.Pow(10, savedBgm / 20);
        float sfxValueNormalized = Mathf.Pow(10, savedSfx / 20);
        bgmSlider.SetValueWithoutNotify(bgmValueNormalized);
        sfxSlider.SetValueWithoutNotify(sfxValueNormalized);

        // ✅ 슬라이더 잠금 상태 유지 (음소거된 경우)
        bgmSlider.interactable = !isBgmMuted;
        sfxSlider.interactable = !isSfxMuted;


        // ✅ 토글 UI 상태 (이벤트 없이 반영)
        bgmMuteToggle.SetIsOnWithoutNotify(isBgmMuted);
        sfxMuteToggle.SetIsOnWithoutNotify(isSfxMuted);
        npcVoiceToggle.SetIsOnWithoutNotify(!isNpcMuted);
        narrationVoiceToggle.SetIsOnWithoutNotify(!isNarrationMuted);

        Debug.Log($"🟣 [SoundSettings] ApplySavedSettings → " +
            $"BGM:{isBgmMuted} / SFX:{isSfxMuted} / NPC:{!isNpcMuted} / Narration:{!isNarrationMuted}");
    }

    // ===============================================================
    // 🔄 리스너 등록 + 로그
    // ===============================================================
    private void RegisterListeners()
    {
        bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);

        bgmMuteToggle.onValueChanged.AddListener((v) =>
        {
            Debug.Log($"🟢 [Toggle Changed] bgmMuteToggle -> {v}");
            SetBgmMute(v);
            PlayClickIfSfxEnabled(); // ✅ 추가
        });
        sfxMuteToggle.onValueChanged.AddListener((v) =>
        {
            Debug.Log($"🟢 [Toggle Changed] sfxMuteToggle -> {v}");
            SetSfxMute(v);
            PlayClickIfSfxEnabled(); // ✅ 추가
        });
        npcVoiceToggle.onValueChanged.AddListener((v) =>
        {
            Debug.Log($"🟢 [Toggle Changed] npcVoiceToggle -> {v}");
            OnNpcVoiceToggle(v);
            PlayClickIfSfxEnabled(); // ✅ 추가
        });
        narrationVoiceToggle.onValueChanged.AddListener((v) =>
        {
            Debug.Log($"🟢 [Toggle Changed] narrationVoiceToggle -> {v}");
            OnNarrationVoiceToggle(v);
            PlayClickIfSfxEnabled(); // ✅ 추가
        });
        AddPointerUpEventForBgm(bgmSlider);
        AddPointerUpEvent(sfxSlider);
         // ✅ 배경음 슬라이더도 버튼 뗄 때 클릭 사운드

        Debug.Log("🟢 [SoundSettings] 모든 리스너 등록 완료");
    }

    // ===============================================================
// 🪄 효과음이 켜져 있을 때만 클릭 소리 재생
// ===============================================================
    private void PlayClickIfSfxEnabled()
    {
        if (testClickSound == null) return;

        // 🔹 PlayerPrefs에서 SFX Mute 상태 확인
        bool isSfxMuted = PlayerPrefs.GetInt("SfxMuted", 0) == 1;

        if (!isSfxMuted && SFXManager.Instance != null)
        {
            SFXManager.Instance.Play(testClickSound, testClickVolume);
            Debug.Log("🟣 [SoundSettings] SFX 켜짐 상태 → 클릭 사운드 재생");
        }
        else
        {
            Debug.Log("⚪ [SoundSettings] SFX 꺼짐 상태 → 클릭 사운드 미재생");
        }
    }
    
    // ===============================================================
// 🪄 배경음이 켜져 있을 때만 클릭 소리 재생
// ===============================================================
    private void PlayClickIfBgmEnabled()
    {
        if (testClickSound == null) return;

        // 🔹 PlayerPrefs에서 BGM Mute 상태 확인
        bool isBgmMuted = PlayerPrefs.GetInt("BgmMuted", 0) == 1;

        if (!isBgmMuted && SFXManager.Instance != null)
        {
            SFXManager.Instance.Play(testClickSound, testClickVolume);
            Debug.Log("🟣 [SoundSettings] BGM 켜짐 상태 → 클릭 사운드 재생");
        }
        else
        {
            Debug.Log("⚪ [SoundSettings] BGM 꺼짐 상태 → 클릭 사운드 미재생");
        }
    }


    

    // ===============================================================
    // 🎵 배경음 (BGM)
    // ===============================================================
    private void SetBgmVolume(float value)
    {
        if (!uiReady) return;
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        masterMixer.SetFloat("BGMVolume", dB);
        PlayerPrefs.SetFloat("BGMVolume", dB);
        
    }

    private void SetBgmMute(bool isMuted)
    {
        if (!uiReady) return;
        if (isMuted)
        {
            masterMixer.GetFloat("BGMVolume", out lastBgmVolume);
            masterMixer.SetFloat("BGMVolume", -80f);
            PlayerPrefs.SetInt("BgmMuted", 1);
            Debug.Log("🔕 BGM 음소거 적용됨");
        }
        else
        {
            masterMixer.SetFloat("BGMVolume", lastBgmVolume);
            PlayerPrefs.SetInt("BgmMuted", 0);
            Debug.Log("🔊 BGM 음소거 해제됨");
        }
        bgmSlider.interactable = !isMuted;

// 🔹 시각적으로 슬라이더 손잡이(동그라미) 위치도 그대로 유지
        bgmSlider.SetValueWithoutNotify(bgmSlider.value);

// 🔹 음소거 시 슬라이더를 아예 비활성화, 해제 시 다시 활성화
        if (isMuted)
        {
            bgmSlider.interactable = false;
        }
        else
        {
            bgmSlider.interactable = true;
        }

    }

    // ===============================================================
    // 🔊 효과음 (SFX)
    // ===============================================================
    private void SetSfxVolume(float value)
    {
        if (!uiReady) return;
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        masterMixer.SetFloat("SFXVolume", dB);
        PlayerPrefs.SetFloat("SFXVolume", dB);
    }

    private void SetSfxMute(bool isMuted)
    {
        if (!uiReady) return;
        if (isMuted)
        {
            masterMixer.GetFloat("SFXVolume", out lastSfxVolume);
            masterMixer.SetFloat("SFXVolume", -80f);
            PlayerPrefs.SetInt("SfxMuted", 1);
            Debug.Log("🔕 SFX 음소거 적용됨");
        }
        else
        {
            masterMixer.SetFloat("SFXVolume", lastSfxVolume);
            PlayerPrefs.SetInt("SfxMuted", 0);
            Debug.Log("🔊 SFX 음소거 해제됨");
        }
        sfxSlider.interactable = !isMuted;
        sfxSlider.SetValueWithoutNotify(sfxSlider.value);

// 🔹 음소거 시 비활성화 / 해제 시 활성화
        if (isMuted)
        {
            sfxSlider.interactable = false;
        }
        else
        {
            sfxSlider.interactable = true;
        }

        PlayClickSound();
    }

    // ===============================================================
    // 🗣️ NPC 음성 켜기
    // ===============================================================
    private void OnNpcVoiceToggle(bool isOn)
    {
        if (!uiReady) return;
        masterMixer.SetFloat("NPCVolume", isOn ? 0f : -80f);
        PlayerPrefs.SetInt("NpcMuted", isOn ? 0 : 1);
        Debug.Log($"🟡 NPC Voice {(isOn ? "ON" : "OFF")}");
    }

    // ===============================================================
    // 📖 나레이션 음성 켜기
    // ===============================================================
    private void OnNarrationVoiceToggle(bool isOn)
    {
        if (!uiReady) return;
        masterMixer.SetFloat("NarrationVolume", isOn ? 0f : -80f);
        PlayerPrefs.SetInt("NarrationMuted", isOn ? 0 : 1);
        Debug.Log($"🟡 Narration Voice {(isOn ? "ON" : "OFF")}");
    }

    // ===============================================================
    // 🪄 클릭 사운드
    // ===============================================================
    private void PlayClickSound()
    {
        if (testClickSound == null) return;
        if (SFXManager.Instance != null)
            SFXManager.Instance.Play(testClickSound, testClickVolume);
    }

    private void AddPointerUpEvent(Slider slider)
    {
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = slider.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        // ✅ 효과음이 켜져 있을 때만 클릭 사운드 재생
        entry.callback.AddListener((eventData) => PlayClickIfSfxEnabled());
        trigger.triggers.Add(entry);
    }
    
    private void AddPointerUpEventForBgm(Slider slider)
    {
        EventTrigger trigger = slider.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = slider.gameObject.AddComponent<EventTrigger>();

        // 중복 방지
        trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.PointerUp);

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entry.callback.AddListener((eventData) => PlayClickIfBgmEnabled());
        trigger.triggers.Add(entry);
    }



    
    // ===============================================================
// 🧹 PlayerPrefs 오디오 설정 초기화 (한 번만 실행용)
// ===============================================================
    [ContextMenu("🔄 Reset Sound Settings")]
    public void ResetSoundSettings()
    {
        PlayerPrefs.DeleteKey("BGMVolume");
        PlayerPrefs.DeleteKey("SFXVolume");
        PlayerPrefs.DeleteKey("BgmMuted");
        PlayerPrefs.DeleteKey("SfxMuted");
        PlayerPrefs.DeleteKey("NpcMuted");
        PlayerPrefs.DeleteKey("NarrationMuted");
        PlayerPrefs.DeleteKey("SoundInitialized");
        PlayerPrefs.Save();
        Debug.Log("🧹 PlayerPrefs 오디오 설정 초기화 완료! 다음 실행 시 기본값으로 복원됩니다.");
    }
    
    // ===============================================================
// 🧩 처음 환경설정 진입 시에만 기본값 적용
// ===============================================================
    private void ResetToDefaultIfFirstTime()
    {
        if (!PlayerPrefs.HasKey("SoundInitialized"))
        {
            // 🎵 첫 실행 시 기본 볼륨 = 0dB (즉, 100%)
            PlayerPrefs.SetFloat("BGMVolume", 0f);
            PlayerPrefs.SetFloat("SFXVolume", 0f);

            // 🔹 기본 토글 상태
            PlayerPrefs.SetInt("BgmMuted", 0);        // 배경음 켜짐 ✅
            PlayerPrefs.SetInt("SfxMuted", 0);        // 효과음 켜짐 ✅
            PlayerPrefs.SetInt("NpcMuted", 0);        // NPC 켜짐 ✅
            PlayerPrefs.SetInt("NarrationMuted", 0);  // 나레이션 켜짐 ✅

            PlayerPrefs.SetInt("SoundInitialized", 1);
            PlayerPrefs.Save();

            Debug.Log("🟣 [SoundSettings] 첫 실행 - 기본 오디오 설정 저장 완료");


            Debug.Log("🟣 [SoundSettings] 첫 실행 - 기본 오디오 설정 저장 완료");
        }
        else
        {
            Debug.Log("⚪ [SoundSettings] 이미 사용자 설정값이 존재함, 기본값 적용 생략");
        }
    }
    
    // ===============================================================
// 🧹 실행 종료 시 모든 PlayerPrefs 초기화
// ===============================================================
    // private void OnApplicationQuit()
    // {
    //     PlayerPrefs.DeleteAll(); // ✅ 모든 저장값 삭제
    //     PlayerPrefs.Save();
    //     Debug.Log("🧹 [SoundSettings] 실행 종료 - PlayerPrefs 전부 초기화됨");
    // }


    
    

}
