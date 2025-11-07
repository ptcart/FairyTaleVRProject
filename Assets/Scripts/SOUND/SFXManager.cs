using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;            // 전역 접근용 싱글톤
    public AudioMixerGroup sfxMixerGroup;         // SFX Mixer 그룹 연결

    private void Awake()
    {
        // ✅ 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);            // 씬이 바뀌어도 유지
    }

    /// <summary>
    /// 효과음을 재생하는 전역 메서드
    /// </summary>
    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // ✅ 임시 오브젝트 생성
        GameObject temp = new GameObject("SFX_Temp_" + clip.name);
        AudioSource source = temp.AddComponent<AudioSource>();

        // Mixer 그룹 연결
        source.outputAudioMixerGroup = sfxMixerGroup;

        source.clip = clip;
        source.volume = volume;
        source.Play();

        // ✅ 재생 후 삭제
        Destroy(temp, clip.length);
    }
}