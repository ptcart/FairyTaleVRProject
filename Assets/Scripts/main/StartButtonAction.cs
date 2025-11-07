using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 게임 시작 버튼 클릭 시 실행할 동작을 정의한 클래스
public class StartButtonAction : MonoBehaviour, IButtonAction
{
    [Header("버튼 클릭 설정")]
    public float delay = 0.3f;           // ⏱ 씬 전환 딜레이 시간
    public AudioClip clickSound;         // 🎵 클릭 효과음
    [Range(0f, 1f)]
    public float volume = 1.0f;          // 🔊 사운드 볼륨 (0~1)

    public void OnButtonClick()
    {
        Debug.Log("게임 시작!");

        // ✅ SFXManager를 통해 Mixer(SFX 그룹) 경로로 재생
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play(clickSound, volume);
        }
        else
        {
            Debug.LogWarning("⚠️ SFXManager가 씬에 없습니다. 클릭 사운드가 재생되지 않습니다.");
        }

        // ✅ 일정 시간 후 다음 씬 로드
        StartCoroutine(LoadSceneWithDelay());
    }

    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("FairyTaleSelectionScene");
    }
}