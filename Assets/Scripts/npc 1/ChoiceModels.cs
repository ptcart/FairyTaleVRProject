using System;

/// <summary>
/// ✅ 선택지 데이터 모델
/// - Flask(choice_list) 응답 JSON과 1:1 매칭
/// - ChoiceManager, NPCInteraction에서 공용으로 사용
/// </summary>
[System.Serializable]
public class ChoiceData
{
    public int choice_id;        // PK (선택지 ID)
    public int story_id;         // 어떤 Story에 속하는지
    public int question_id;      // ✅ 질문 ID (추가)
    public string content;       // 버튼에 표시할 텍스트
    public int next_story_id;    // 선택 시 이어질 Story ID
    public int has_puzzle_game;  // 퍼즐 여부 (0: 없음, 1: 있음)
}

/// <summary>
/// ✅ 선택지 리스트 Wrapper
/// - JsonUtility는 배열([])을 바로 파싱 못하므로
///   { "choices": [ ... ] } 형태로 감싼 뒤 사용
/// </summary>
[System.Serializable]
public class ChoiceListWrapper
{
    public ChoiceData[] choices;
}