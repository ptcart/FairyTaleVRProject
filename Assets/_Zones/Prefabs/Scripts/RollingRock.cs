using UnityEngine;

/// <summary>
/// 시작 시 바위를 특정 방향으로 자연스럽게 굴리는 스크립트 + 반복 효과음
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RollingRock : MonoBehaviour
{
    [Header("🪨 굴림 설정")]
    [Tooltip("굴러갈 방향 (예: X+Z는 ↘ 방향)")]
    public Vector3 rollDirection = new Vector3(1, 0, 1);

    [Tooltip("굴러가는 힘의 크기")]
    public float rollForce = 1000f;

    [Tooltip("시작 시 자동으로 굴릴지 여부")]
    public bool autoRollOnStart = true;

    [Tooltip("Y축으로 들썩임 방지 (위로 튀는 거 방지)")]
    public bool freezeYVelocity = false;

    [Header("🎵 사운드 설정")]
    [Tooltip("바위 굴러갈 때 재생할 효과음 (3초짜리)")]
    public AudioClip rollSound;

    [Tooltip("효과음을 반복 재생할 총 시간 (초)")]
    public float totalSoundDuration = 10f;

    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // AudioSource 자동 준비
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false; // 기본은 반복 꺼둠

        if (autoRollOnStart)
        {
            Roll(); // ✅ Start 내부에서 호출 시에는 안전함
        }
    }

    /// <summary>
    /// 외부에서 수동으로 호출할 수도 있음
    /// </summary>
    public void Roll()
    {
        // ✅ Rigidbody 안전 확인
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("❌ RollingRock: Rigidbody가 없습니다. Roll() 실패.");
                return;
            }
        }

        // 바위 굴리기
        Vector3 force = rollDirection.normalized * rollForce;
        rb.AddForce(force);
        Debug.Log($"🧱 RollingRock: Force {force} applied.");

        // 효과음 재생 (반복)
        if (rollSound != null && audioSource != null)
        {
            audioSource.clip = rollSound;
            audioSource.loop = true; // 반복 재생
            audioSource.Play();

            // 일정 시간 뒤 자동으로 멈춤
            StartCoroutine(StopSoundAfterDelay(totalSoundDuration));
        }
    }

    void FixedUpdate()
    {
        if (freezeYVelocity && rb != null)
        {
            // 위아래 튐 방지: Y축 속도를 0으로 강제
            Vector3 v = rb.velocity;
            v.y = 0;
            rb.velocity = v;
        }
    }

    private System.Collections.IEnumerator StopSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false; // 반복 해제
        }
    }
}
