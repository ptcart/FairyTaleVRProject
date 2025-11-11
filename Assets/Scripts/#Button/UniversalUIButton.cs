using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalUIButton : MonoBehaviour, IButtonAction
{
    [Header("🎯 버튼 기본 설정")]
    [Tooltip("버튼 클릭 시 전환할 씬 이름 (비워두면 씬 전환 없음)")]
    public string sceneToLoad;

    [Tooltip("씬 전환 전 딜레이 시간 (초)")]
    public float sceneLoadDelay = 0.3f;

    [Header("🔊 클릭 사운드 설정")]
    public AudioClip clickSound;
    [Range(0f, 1f)] public float volume = 1f;

    public void OnButtonClick()
    {
        Debug.Log($"🖱️ 버튼 클릭됨: {gameObject.name}");

        // ✅ 클릭 사운드 재생 (SFXMixer 경로)
        if (SFXManager.Instance != null && clickSound != null)
        {
            SFXManager.Instance.Play(clickSound, volume);
        }
        else
        {
            Debug.LogWarning("⚠️ SFXManager 또는 클릭 사운드가 설정되지 않았습니다.");
        }

        // ✅ 씬 이름이 설정되어 있으면 전환
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"🎬 {sceneToLoad} 씬 전환 예약됨 ({sceneLoadDelay}s 후)");
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(sceneToLoad);
    }
}