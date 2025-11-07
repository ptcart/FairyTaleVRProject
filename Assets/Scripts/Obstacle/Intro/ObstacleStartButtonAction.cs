using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 전환을 위한 네임스페이스

// 게임 시작 버튼 클릭 시 실행할 동작을 정의한 클래스
public class ObstacleStartButtonAction : MonoBehaviour, IButtonAction
{
    public void OnButtonClick()
    {
        // 게임 시작 시 씬 전환
        Debug.Log("장애물 치우기 게임 시작!");
        //SceneManager.LoadScene("MainVRScene");  // 씬 전환
        
        SceneManager.LoadScene("ObstacleMain");
    }
}