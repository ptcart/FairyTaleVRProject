using UnityEngine;

/// <summary>
/// 수거 가능한 아이템(돌, 나무, 큰바위)에 부착되는 스크립트
/// - 타입 식별
/// - 중복 카운트 방지
/// </summary>
public class CollectableItem : MonoBehaviour
{
    [Tooltip("아이템 종류 (돌, 통나무, 큰바위 등)")]
    public ObstacleType itemType;

    [Tooltip("이미 카운트 되었는지 여부 (한 번만 수거 인정)")]
    public bool hasBeenCounted = false;
}