using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 전환을 위한 네임스페이스

// 게임 시작 버튼 클릭 시 실행할 동작을 정의한 클래스
public class MazeStartButtonAction : MonoBehaviour, IButtonAction
{
    [Header("전환할 씬 이름 (인스펙터에서 지정)")]
    public string sceneToLoad;

    public void OnButtonClick()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"씬 전환 시도: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("⚠ 전환할 씬 이름이 비어있습니다! 인스펙터에서 설정해주세요.");
        }
    }
}