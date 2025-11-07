using UnityEngine;

// 📘 이 스크립트는 현재는 아무 대사 처리 없이,
// 단지 스토리 흐름을 다음으로 넘기기 위한 구조만 포함합니다.
// (즉, NPC 등장/퇴장만 확인하고, 바로 다음 스토리로 넘어가는 역할만 함)

public class DialogueManager : MonoBehaviour
{
    private Dialogue[] dialogueList;
    private int currentIndex = 0;
    
    public void StartDialogue(Dialogue[] dialogues)
    {
        Debug.Log("📜 대사 없이 바로 다음 스토리로 넘어감");

        // ✅ 대사 없이 바로 다음 스토리 진행
        VRStoryProgressor.Instance?.LoadNextStory();
    }
    
    public void ShowCurrentDialogue()
    {
        if (dialogueList == null || currentIndex >= dialogueList.Length) return;

        Dialogue current = dialogueList[currentIndex];
        Debug.Log($"🗨️ NPC {current.npcId}: {current.content}");

        // ✅ NPC 프리팹 찾기
        string npcName = $"npc_{current.npcId}";
        GameObject npcObj = GameObject.Find(npcName);
    
        if (npcObj != null)
        {
            // ✅ 카메라 NPC 정면 확대
            VRCameraFocus cameraFocus = FindObjectOfType<VRCameraFocus>();
            if (cameraFocus != null)
            {
                cameraFocus.FocusOnNPC(npcObj.transform);
            }
        }

        // 👉 이후 말풍선 출력, 음성 재생 등 여기에 추가 가능
    }
    
    public void ShowNextDialogue()
    {
        currentIndex++;
        if (currentIndex < dialogueList.Length)
        {
            ShowCurrentDialogue();
        }
        else
        {
            VRStoryProgressor.Instance?.LoadNextStory();
        }
    }
}