// ObstacleTypeExtensions.cs
using UnityEngine;

/// <summary>
/// ObstacleType enum → 한글 이름 변환용 확장 메서드
/// </summary>
public static class ObstacleTypeExtensions
{
    public static string ToKorean(this ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Rock: return "돌";
            case ObstacleType.Log: return "통나무";
            case ObstacleType.BigRock: return "큰 바위";
            default: return type.ToString(); // 혹시 매핑이 없으면 기본 영어 표시
        }
    }
}