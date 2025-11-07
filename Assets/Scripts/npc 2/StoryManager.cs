using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance; // 싱글톤 인스턴스

    public int currentStoryId; // 현재 진행 중인 스토리 ID

    void Awake()
    {
        // 만약 이미 인스턴스가 존재하면 파괴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ 씬이 바뀌어도 이 오브젝트는 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }
}