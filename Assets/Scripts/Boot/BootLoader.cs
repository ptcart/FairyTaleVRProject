using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [Tooltip("다음 씬 이름")]
    public string nextSceneName = "MainVRScene";

    private IEnumerator Start()
    {
        // 🔹 Mixer 초기화 완료될 때까지 약간의 대기
        yield return new WaitForSeconds(0.3f);

        Debug.Log("🎧 [BootLoader] 사운드 설정 반영 완료 → 다음 씬으로 이동");
        SceneManager.LoadScene(nextSceneName);
    }
}