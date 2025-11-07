using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PersistentBGM : MonoBehaviour
{
    private static PersistentBGM instance;
    private AudioSource audioSource;

    [Tooltip("BGM을 정지시킬 씬들의 이름을 입력하세요.")]
    public List<string> scenesToStopBGM = new List<string>(); 

    private bool isBGMStoppedForever = false; // ✅ 한 번 멈추면 끝까지 유지하는 플래그

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isBGMStoppedForever)
        {
            // 이미 꺼진 적이 있으면 절대 켜지지 않음
            if (audioSource.isPlaying)
                audioSource.Stop();
            return;
        }

        if (scenesToStopBGM.Contains(scene.name))
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            // ✅ 한 번 꺼졌으면 영원히 꺼지도록 설정
            isBGMStoppedForever = true;
        }
        else
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }
}