using UnityEngine;

public class NPCVoiceManager : MonoBehaviour
{
    public static NPCVoiceManager Instance;
    private AudioSource npcSource;

    [Range(0f, 1f)] public float volume = 1f;
    public bool isMuted = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        npcSource = gameObject.AddComponent<AudioSource>();
        npcSource.playOnAwake = false;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (isMuted) return;

        npcSource.clip = clip;
        npcSource.volume = volume;
        npcSource.Play();
    }

    public void Stop()
    {
        npcSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        npcSource.volume = isMuted ? 0f : volume;
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        npcSource.volume = isMuted ? 0f : volume;
    }
}