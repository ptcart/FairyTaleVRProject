using UnityEngine;

public class MainVRSceneManager : MonoBehaviour
{
    private static bool soundInitialized = false; // 🔹 앱 실행 중 1회만 초기화되도록 하는 플래그

    void Awake()
    {
        if (!soundInitialized)
        {
            // 🔹 앱 완전 새 실행 시에만 오디오 설정 초기화
            PlayerPrefs.DeleteKey("SoundInitialized");
            soundInitialized = true; // ✅ 이후엔 다시 초기화되지 않음

            Debug.Log("🧹 [MainVRSceneManager] 앱 첫 실행 감지 → 오디오 설정 초기화 완료");
        }
        else
        {
            Debug.Log("⚪ [MainVRSceneManager] 이미 초기화된 상태 → 오디오 설정 유지");
        }
    }

    void Start()
    {
        Debug.Log("🎬 [MainVRSceneManager] 메인화면 초기화 완료");
    }
}