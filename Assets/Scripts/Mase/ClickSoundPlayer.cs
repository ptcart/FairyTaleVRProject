using UnityEngine;

public class ClickSoundPlayer : MonoBehaviour
{
    public static ClickSoundPlayer Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        // 싱글턴 + 씬 넘어가도 살아있게
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClick(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}