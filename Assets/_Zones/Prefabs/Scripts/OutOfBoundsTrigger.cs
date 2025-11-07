using UnityEngine;

/// <summary>
/// ✅ 맵 경계(boundary)를 벗어났을 때 처리하는 트리거
/// - 플레이어: 현재 Zone 기준 리스폰 위치로 되돌림
/// - 장애물(돌, 통나무 등): 원래 자리로 리셋
/// </summary>
[RequireComponent(typeof(Collider))]
public class OutOfBoundsTrigger : MonoBehaviour
{
    [Header("🎯 태그 설정")]
    public string playerTag = "Player";
    public string obstacleTag = "Obstacle";

    private void Reset()
    {
        // 에디터에서 붙였을 때 자동 설정
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        // ✅ 플레이어가 나갔을 경우
        if (other.CompareTag(playerTag))
        {
            var mission = FindObjectOfType<MissionController>();
            var respawn = mission?.GetRespawnPointForCurrentZone();

            if (respawn == null)
            {
                Debug.LogError("❌ 리스폰 위치가 설정되지 않았습니다.");
                return;
            }

            Debug.Log($"🚷 플레이어가 경계 밖으로 나감 → Zone {mission.currentZoneId} 기준 리스폰");

            var cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                other.transform.position = respawn.position;
                other.transform.rotation = respawn.rotation;
                cc.enabled = true;
            }
            else
            {
                other.transform.position = respawn.position;
                other.transform.rotation = respawn.rotation;
            }

            // ✅ 여기서 진동, 사운드, 페이드 등 연출 추가 가능
        }

        // ✅ 물체(돌, 통나무 등)일 경우
        else if (other.CompareTag(obstacleTag))
        {
            var item = other.GetComponent<RespawnableItem>();
            if (item != null)
            {
                item.ResetToStart();
                Debug.Log($"🪨 {other.name} → 원위치로 리셋됨");
            }
        }
    }
}