// using UnityEngine;
// using UnityEngine.SceneManagement;
// using System.Collections.Generic;
//
// public class PersistentBGM : MonoBehaviour
// {
//     public string bgmName;  // BGM 고유 이름
//     public List<string> scenesToStopBGM = new List<string>();
//
//     private static Dictionary<string, PersistentBGM> instances = new Dictionary<string, PersistentBGM>();
//     private AudioSource audioSource;
//     private bool isStopped = false;
//     private string originScene;
//     public List<string> scenesToKeepBGM = new List<string>(); // ✅ 새로 추가
//
//     void Awake()
//     {
//         audioSource = GetComponent<AudioSource>();
//         originScene = SceneManager.GetActiveScene().name; // ✅ 추가
//         
//
//         if (instances.ContainsKey(bgmName))
//         {
//             Destroy(gameObject); // 같은 이름이 이미 존재하면 파괴
//             return;
//         }
//
//         instances.Add(bgmName, this);
//         DontDestroyOnLoad(gameObject);
//     }
//
//     void OnEnable()
//     {
//         SceneManager.sceneLoaded += OnSceneLoaded;
//     }
//
//     void OnDisable()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }
//
//     void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         string currentScene = scene.name;
//
//         // 1️⃣ Stop 리스트에 포함된 씬이면 정지
//         if (scenesToStopBGM.Contains(currentScene))
//         {
//             if (audioSource.isPlaying)
//             {
//                 audioSource.Stop();
//                 Debug.Log($"🛑 {bgmName} 정지됨 (Stop리스트: {currentScene})");
//             }
//             isStopped = true;
//             return;
//         }
//
//         // 2️⃣ 유지 리스트(Keep)에 포함된 씬이면 그대로 유지
//         if (scenesToKeepBGM.Contains(currentScene))
//         {
//             if (!audioSource.isPlaying)
//             {
//                 audioSource.Play();
//                 Debug.Log($"🎵 {bgmName} 유지 (Keep리스트: {currentScene})");
//             }
//             isStopped = false;
//             return;
//         }
//
//         // 3️⃣ 원래 씬으로 돌아왔을 때 재생
//         if (currentScene == originScene)
//         {
//             if (!audioSource.isPlaying)
//             {
//                 audioSource.Play();
//                 Debug.Log($"🎵 {bgmName} 재생 (복귀: {originScene})");
//             }
//             isStopped = false;
//             return;
//         }
//
//         // 4️⃣ 나머지 씬은 정지
//         if (audioSource.isPlaying)
//         {
//             audioSource.Stop();
//             Debug.Log($"⏹️ {bgmName} 정지 (현재: {currentScene}, 원래: {originScene})");
//         }
//
//         isStopped = true;
//     }
//
// }

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PersistentBGM : MonoBehaviour
{
    public string bgmName;  // BGM 고유 이름
    public List<string> scenesToKeepBGM = new List<string>(); // ✅ 유지할 씬만 관리

    private static Dictionary<string, PersistentBGM> instances = new Dictionary<string, PersistentBGM>();
    private AudioSource audioSource;
    private string originScene;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        originScene = SceneManager.GetActiveScene().name; // ✅ 초기 씬 저장

        if (instances.ContainsKey(bgmName))
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        instances.Add(bgmName, this);
        DontDestroyOnLoad(gameObject);
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
        string currentScene = scene.name;

        // ✅ 유지 리스트에 포함된 씬이면 재생 유지
        if (scenesToKeepBGM.Contains(currentScene) || currentScene == originScene)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log($"🎵 {bgmName} 재생 (유지 씬: {currentScene})");
            }
            return;
        }

        // ⏹️ 유지 리스트에 없는 씬에서는 정지
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log($"🛑 {bgmName} 정지 (씬: {currentScene})");
        }
    }
}

