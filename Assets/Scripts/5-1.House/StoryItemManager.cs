using UnityEngine;

public class StoryItemManager : MonoBehaviour
{
    public static StoryItemManager Instance;

    private bool butterFound = false;
    private bool pepperFound = false;
    private bool bucketFound = false;

    public bool questCompleted = false; // ✅ 수집 퀘스트 완료 여부 저장

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectItem(string itemName)
    {
        switch(itemName)
        {
            case "Butter": butterFound = true; break;
            case "Pepper": pepperFound = true; break;
            case "Bucket": bucketFound = true; break;
        }

        Debug.Log($"{itemName} 획득 완료!");

        // ✅ 모든 재료를 모았다면 퀘스트 완료 처리
        if (AllItemsCollected() && !questCompleted)
        {
            questCompleted = true;
            Debug.Log("🟢 모든 재료를 모았습니다! NPC에게 돌아가세요.");
            DialogueUI.Instance?.ShowTemporaryMessage("모든 재료를 모았다!\nNPC에게 돌아가자", 2f);
        }
    }

    public bool AllItemsCollected()
    {
        return butterFound && pepperFound && bucketFound;
    }
}