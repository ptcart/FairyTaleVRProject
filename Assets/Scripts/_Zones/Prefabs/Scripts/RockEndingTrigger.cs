using UnityEngine;

public class RockEndingTrigger : MonoBehaviour
{
    [SerializeField] private string endingSceneName = "ObstacleEnding"; // 이동할 엔딩 씬 이름
    [SerializeField] private float lockSeconds = 1.2f; // 충돌 후 잠금 시간

    private bool _triggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (_triggered) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        _triggered = true;
        Debug.Log("💥 바위 충돌 → 페이드아웃 시작");

        // 1) 플레이어 조작 잠금
        var locker = collision.gameObject.GetComponentInParent<PlayerControlLocker>();
        if (locker != null) locker.LockForSeconds(lockSeconds);

        // 2) ScreenFader 찾아서 페이드 아웃 실행
        ScreenFader fader = FindObjectOfType<ScreenFader>();
        if (fader != null)
        {
            fader.nextSceneName = endingSceneName; // 엔딩 씬 이름 지정
            fader.StartFadeOut();                  // 페이드 아웃 + 씬 전환
        }
        else
        {
            Debug.LogWarning("⚠️ ScreenFader가 씬에 없습니다. 바로 전환합니다.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(endingSceneName);
        }
    }
}