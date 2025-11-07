using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ObstacleType
{
    Rock,      // 돌
    Log,       // 통나무
    BigRock    // 큰 바위
}

/// <summary>
/// 특정 장애물을 수거하는 Dump Point 영역
/// 장애물이 들어오고 나갈 때 개수 조절 및 텍스트 업데이트
/// </summary>
public class DumpPoint : MonoBehaviour
{
    [Header("🧱 기본 설정")]
    public string pointId = "A1";                        // 고유 포인트 ID
    public string zoneId = "A";                          // 어떤 Zone에 속하는지
    public ObstacleType targetType = ObstacleType.Rock;  // 받을 장애물 종류

    [Header("📦 수거 목표")]
    public int requiredAmount = 1;                       // 수거 목표 수

    [Header("📝 텍스트 표시")]
    public TMP_Text progressText;                        // 진행 상황 표시용 TMP 텍스트

    // ✅ 실제 내부 상태
    private HashSet<GameObject> insideItems = new();     // 현재 안에 들어온 아이템 목록

    /// <summary>
    /// 현재 수거된 개수
    /// </summary>
    public int currentAmount => insideItems.Count;

    /// <summary>
    /// 이 DumpPoint가 완료되었는지 여부
    /// </summary>
    public bool IsComplete => currentAmount >= requiredAmount;

    void Start()
    {
        UpdateText();
    }

    /// <summary>
    /// 장애물이 트리거에 들어올 때
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponent<CollectableItem>();
        if (item == null) return;
        if (item.itemType != targetType) return;

        if (!insideItems.Contains(other.gameObject))
        {
            insideItems.Add(other.gameObject);
            Debug.Log($"{item.itemType.ToKorean()} : {currentAmount}/{requiredAmount}");

            UpdateText();
        }
    }
    
    
    /// <summary>
    /// 장애물이 트리거에서 나갈 때
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        var item = other.GetComponent<CollectableItem>();
        if (item == null) return;
        if (item.itemType != targetType) return;

        if (insideItems.Contains(other.gameObject))
        {
            insideItems.Remove(other.gameObject);
            Debug.Log($"❌ {pointId}: {item.itemType} 나감 ({currentAmount}/{requiredAmount})");
            UpdateText();
        }
    }

    /// <summary>
    /// 수거 현황 텍스트 갱신
    /// </summary>
    private void UpdateText()
    {
        if (progressText != null)
        {
           
            progressText.text = $"{targetType} : {currentAmount}/{requiredAmount}";
        }
    }
    
    /// <summary>
    /// 현재 이 DumpPoint 안에 들어온 아이템들 반환 (외부에서 제거 가능)
    /// </summary>
    public IEnumerable<GameObject> GetInsideItems()
    {
        return insideItems;
    }
}
