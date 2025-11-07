using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 전환을 위한 네임스페이스

// 게임 시작 버튼 클릭 시 실행할 동작을 정의한 클래스
public class EndingButtonAction : MonoBehaviour, IButtonAction
{
    
    public float delay = 0.3f;           // ⏱ 딜레이 시간
    public AudioClip clickSound;         // 🎵 클릭 효과음
    public float volume = 1.0f;          // 🔊 사운드 볼륨
    public void OnButtonClick()
    {
        // 게임 시작 시 씬 전환
        Debug.Log("엔딩 모음!");
        PlayClickSound();
        StartCoroutine(LoadSceneWithDelay());
        //SceneManager.LoadScene("FairyTaleSelectionScene");
    }
    
    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            // 메인 카메라 위치에서 사운드 재생 (씬 바뀌어도 안 끊김!)
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, volume);
        }
    }
    
    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Endings");
    }
}