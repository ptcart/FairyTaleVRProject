using UnityEngine;

public class HouseTrigger : MonoBehaviour
{
    public CountdownTimer timer; // 드래그로 연결
    public int totalKeys = 3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.breadCount >= totalKeys)
            {
                Debug.Log("🏠 집 도착! 열쇠(빵) 다 모음!");
                timer.StopTimer();  // ⏹ 타이머 멈춤

                // TODO: 성공 처리 (씬 전환, 승리 UI 등)
            }
        }
    }
}