using UnityEngine;

/// <summary>
/// ✅ 리스폰 가능한 오브젝트 (돌, 통나무 등)
/// - 초기 위치와 회전을 저장하고
/// - ResetToStart() 호출 시 제자리로 복귀
/// - 리지드바디의 속도도 초기화함
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RespawnableItem : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;
    private Rigidbody rb;
    private int respawnCount = 0;  // 🔁 리스폰 횟수 추적 (선택)

    private void Awake()
    {
        // 초기 위치와 회전 기억
        startPos = transform.position;
        startRot = transform.rotation;

        // Rigidbody 가져오기
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"❌ {name} 에 Rigidbody가 없습니다!");
        }
    }

    /// <summary>
    /// ✅ 제자리로 되돌리는 메서드
    /// - 위치/회전 복원
    /// - 속도 초기화
    /// - 콘솔 출력
    /// </summary>
    public void ResetToStart()
    {
        if (rb == null)
        {
            Debug.LogWarning($"⚠️ {name} 의 Rigidbody가 null입니다. 리셋 불가");
            return;
        }

        // 속도 정지
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 위치/회전 복원
        transform.position = startPos;
        transform.rotation = startRot;

        // 로그 출력
        respawnCount++;
        Debug.Log($"🔁 {name} 리셋됨 #{respawnCount} → 위치: {startPos}, 회전: {startRot.eulerAngles}");
    }
}