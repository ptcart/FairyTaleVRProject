[System.Serializable]
public class StoryBundle
{
    public string storyContent;     // 스토리 설명 (나래이션)
    public int nextStoryId;         // 다음 스토리 번호
    public NpcData[] npcs;          // 등장할 NPC 배열
    public Dialogue[] dialogues;    // 대사 목록 (지금은 사용 안 함)
}

[System.Serializable]
public class NpcData
{
    public int npcId;               // NPC 고유 ID
    public string npcName;          // NPC 이름
    public string prefab;           // Resources 폴더에서 불러올 프리팹 이름
    public float positionX;
    public float positionY;
    public float positionZ;
    public int appearStoryId;       // 등장 시작 스토리 ID
    public int disappearStoryId;    // 퇴장 스토리 ID
}

[System.Serializable]
public class Dialogue
{
    public int dialogueId;
    public int order;
    public string content;
    public int npcId;
    public string npcName;
}