using UnityEngine;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;
    private AudioSource narrationSource;

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
        narrationSource = gameObject.AddComponent<AudioSource>();
        narrationSource.playOnAwake = false;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (isMuted) return;

        narrationSource.clip = clip;
        narrationSource.volume = volume;
        narrationSource.Play();
    }

    public void Stop()
    {
        narrationSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        narrationSource.volume = isMuted ? 0f : volume;
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        narrationSource.volume = isMuted ? 0f : volume;
    }
}