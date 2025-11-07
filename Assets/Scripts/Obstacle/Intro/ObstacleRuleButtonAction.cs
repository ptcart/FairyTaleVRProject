using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObstacleRuleButtonAction : MonoBehaviour, IButtonAction
{
    public float delay = 0.3f;           // ⏱ 딜레이 시간
    public AudioClip clickSound;         // 🎵 클릭 효과음
    public float volume = 1.0f;          // 🔊 사운드 볼륨

    public void OnButtonClick()
    {
        //Debug.Log("미로 게임 룰 진입!");
        PlayClickSound();                      // 🔊 사운드 먼저 재생
        StartCoroutine(LoadSceneWithDelay());  // ⏱ 딜레이 후 씬 전환
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
        SceneManager.LoadScene("ObstacleRule");
    }
}