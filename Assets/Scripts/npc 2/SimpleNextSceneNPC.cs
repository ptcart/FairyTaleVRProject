using UnityEngine;
using TMPro;

public class VRNPCInteraction : MonoBehaviour
{
    public GameObject dialogueUI;
    public TextMeshProUGUI npcNameText;      // ✅ 추가 (NPC 이름용)
    public TextMeshProUGUI dialogueText;     // 대사 텍스트
    public Transform rightHandAnchor;
    public LayerMask npcLayerMask;

    private bool isRayHittingNPC = false;
    private bool isDialogueActive = false;

    private string[] dialogues = new string[]
    {
        "빨간 모자: 할머니!! 저 왔어요!!.",
        "빨간 모자: 어라..? 왜 대답이 없으시지..?",
        "???: 문 열려있다! 들어오거라!!",
        "빨간 모자: 앗! 넵!! 들어갈께요 할머니!!"
    };
    private int currentDialogueIndex = 0;

    void Start()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    void Update()
    {
        CheckRayHit();

        if (isRayHittingNPC && OVRInput.GetDown(OVRInput.Button.One))
        {
            if (!isDialogueActive)
                StartDialogue();
            else
                NextDialogue();
        }
    }

    void CheckRayHit()
    {
        isRayHittingNPC = false;

        if (rightHandAnchor == null)
            return;

        Ray ray = new Ray(rightHandAnchor.position, rightHandAnchor.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f, npcLayerMask))
        {
            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                isRayHittingNPC = true;
            }
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueUI?.SetActive(true);
        ShowCurrentDialogue();
    }

    void NextDialogue()
    {
        currentDialogueIndex++;
        if (currentDialogueIndex >= dialogues.Length)
        {
            isDialogueActive = false;
            currentDialogueIndex = 0;
            dialogueUI?.SetActive(false);
        }
        else
        {
            ShowCurrentDialogue();
        }
    }

    void ShowCurrentDialogue()
    {
        if (dialogues.Length == 0)
            return;

        string line = dialogues[currentDialogueIndex];

        // ✅ 이름 : 대사 분리
        if (line.Contains(":"))
        {
            string[] splitLine = line.Split(new char[] { ':' }, 2);
            if (npcNameText != null)
                npcNameText.text = splitLine[0].Trim();  // 이름 부분
            if (dialogueText != null)
                dialogueText.text = splitLine[1].Trim(); // 대사 부분
        }
        else
        {
            if (npcNameText != null)
                npcNameText.text = "";
            if (dialogueText != null)
                dialogueText.text = line;
        }
    }
}
