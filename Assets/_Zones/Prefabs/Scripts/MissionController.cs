using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MissionController : MonoBehaviour
{
    [System.Serializable]
    public class ZoneGroup
    {
        public string zoneId;                        // 예: "A"
        public List<DumpPoint> dumpPoints;           // 해당 Zone에 포함된 DumpPoint들
        public GameObject gateObject;                // Zone을 막는 월드보더/게이트
        public Transform respawnPoint;              // ✅ 이 Zone의 리스폰 위치

        public bool IsComplete => dumpPoints.All(p => p.IsComplete);
    }

    [Header("🗌 Zone 설정")]
    public List<ZoneGroup> zones;

    [Header("▶️ 현재 진행 중인 Zone ID")]
    public string currentZoneId = "A";   // 시작 Zone

    [Header("🔊 Zone 클리어 효과음")]
    public AudioClip zoneClearSound;     // 효과음
    public float soundVolume = 3.0f;     // 0.0 ~ 1.0

    void Update()
    {
        foreach (var zone in zones)
        {
            if (zone.IsComplete && zone.gateObject != null && zone.gateObject.activeSelf)
            {
                Debug.Log($"🟢 Zone {zone.zoneId} 완료! 건물 열림");

                // 효과음 재사 (gate 위치)
                if (zoneClearSound != null)
                {
                    Vector3 soundPos = zone.gateObject.transform.position;
                    AudioSource.PlayClipAtPoint(zoneClearSound, soundPos, soundVolume);
                }

                // 건물 제거
                zone.gateObject.SetActive(false);

                // DumpPoint 및 장애물 제거
                foreach (var dump in zone.dumpPoints)
                {
                    foreach (var item in dump.GetInsideItems())
                    {
                        if (item != null)
                            Destroy(item);
                    }

                    if (dump != null)
                        Destroy(dump.gameObject);
                }

                // 다음 Zone으로 이동
                if (zone.zoneId == currentZoneId)
                {
                    int index = zones.FindIndex(z => z.zoneId == currentZoneId);
                    if (index >= 0 && index < zones.Count - 1)
                    {
                        currentZoneId = zones[index + 1].zoneId;
                        Debug.Log($"➡️ 다음 Zone: {currentZoneId}");
                    }
                    else
                    {
                        Debug.Log("🏋️️ 모든 Zone 완료!");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 현재 Zone에 해당하는 리스폰 위치 발을 반환
    /// </summary>
    public Transform GetRespawnPointForCurrentZone()
    {
        var zone = zones.Find(z => z.zoneId == currentZoneId);
        return zone?.respawnPoint;
    }

    /// <summary>
    /// 현재 Zone의 상세 수가 현황 발을 (\ec4u 출력용)
    /// </summary>
    public string GetDetailedZoneProgressText(string zoneId)
    {
        var zone = zones.Find(z => z.zoneId == zoneId);
        if (zone == null) return $"존재하지 않는 Zone: {zoneId}";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"현재 Zone: {zone.zoneId} 단계");

        foreach (var dump in zone.dumpPoints)
        {
            string name = GetKoreanName(dump.targetType);
            sb.AppendLine($"{name,-4} {dump.currentAmount} / {dump.requiredAmount}");
        }

        return sb.ToString();
    }

    private string GetKoreanName(ObstacleType type)
    {
        return type switch
        {
            ObstacleType.Rock => "돌",
            ObstacleType.Log => "통나무",
            ObstacleType.BigRock => "큰 바위",
            _ => "?"
        };
    }
}
