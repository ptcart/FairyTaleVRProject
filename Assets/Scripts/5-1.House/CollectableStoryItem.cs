using UnityEngine;
using UnityEngine.UI;

public class CollectableStoryItem : MonoBehaviour
{
    [Header("아이템 설정")]
    public string itemName;      
    public string itemKey;   // 시스템용 (Bucket, Butter, Pepper)
    public float holdDuration = 2f;  // 버튼 누르고 있어야 하는 시간

    [Header("UI")]
    public Image progressBar;       

    private bool collected = false;
    private float holdStartTime = -1f; 
    private bool isTargeted = false;    
    
    private QuickOutline outline; // 아이템 테두리용

    void Start()
    {
        // 시작할 땐 게이지 꺼두기
        if (progressBar != null)
            progressBar.gameObject.SetActive(false);
        
        // 🔹 테두리 초기화
        outline = GetComponent<QuickOutline>();
        if (outline != null)
            outline.SetOutline(false); // 시작 시 비활성화
    }

    void Update()
    {
        int currentStory = NPCInteraction.CurrentStoryId;

        // ✅ 현재 스토리ID 가져오기
        // ✅ 현재 NPC의 스토리 ID 확인
        // 🔹 스토리 301일 때만 테두리 표시
        if (outline != null)
        {
            if (currentStory == 301 && !collected)
                outline.SetOutline(true);
            else
                outline.SetOutline(false);
        }
        Debug.Log("지금 스토리 어디인가용? : " + currentStory);

        if (isTargeted && currentStory == 301) // 🔑 301일 때만 게이지 표시
        {
            if (progressBar != null && !progressBar.gameObject.activeSelf)
                progressBar.gameObject.SetActive(true);

            if (OVRInput.Get(OVRInput.Button.One))
            {
                if (holdStartTime < 0f)
                {
                    holdStartTime = Time.time;
                    Debug.Log($"{itemName} 수집 시작");
                }

                float holdTime = Time.time - holdStartTime;

                if (progressBar != null)
                    progressBar.fillAmount = Mathf.Clamp01(holdTime / holdDuration);

                if (holdTime >= holdDuration)
                    Collect();
            }
            else
            {
                holdStartTime = -1f;
                if (progressBar != null)
                    progressBar.fillAmount = 0f;
            }
        }
        else
        {
            // 🔒 301이 아니거나 Ray가 안 맞으면 게이지 숨김
            if (progressBar != null && progressBar.gameObject.activeSelf)
                progressBar.gameObject.SetActive(false);

            holdStartTime = -1f;
        }

        isTargeted = false; // 매 프레임 초기화
    }

    public void SetTargeted(bool state)
    {
        isTargeted = state;
    }

    private void Collect()
    {
        int currentStory = GameDataManager.nextStoryIdToLoad;

        // 301 구간에서만 수집 허용
        if (currentStory != 301)
        {
            Debug.Log($"❌ 현재는 재료를 수집할 수 없습니다. (storyId={currentStory})");
            return;
        }

        // 이미 수집된 경우 중복 방지
        if (collected)
        {
            Debug.Log($"⚠️ {itemName}은 이미 수집된 아이템입니다.");
            return;
        }

        collected = true;

        // ✅ StoryItemManager에 반영
        StoryItemManager.Instance?.CollectItem(itemName);

        Debug.Log($"✅ {itemName} 최종 수집 완료!");
        DialogueUI.Instance?.ShowTemporaryMessage($"{itemKey} 획득!", 1.5f);

        if (progressBar != null) 
            progressBar.fillAmount = 0f;

        // 오브젝트 비활성화 → 사라지게
        gameObject.SetActive(false);

        // ✅ 퀘스트 완료 체크
        if (StoryItemManager.Instance.AllItemsCollected())
        {
            Debug.Log("모든 재료를 모았습니다! 이제 NPC와 대화할 수 있습니다.");
            DialogueUI.Instance?.ShowTemporaryMessage("할머니에게 가자", 2f);
            StoryItemManager.Instance.questCompleted = true;
        }
        else
        {
            Debug.Log("📦 아직 남은 재료가 있습니다.");
        }
    }

}
