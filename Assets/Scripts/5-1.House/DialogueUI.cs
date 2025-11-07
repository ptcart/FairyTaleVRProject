using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    void Awake()
    {
        Instance = this;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // ✅ 잠깐 메시지 띄우고 자동으로 사라지기
    public void ShowTemporaryMessage(string message, float duration)
    {
        StopAllCoroutines(); // 혹시 기존 코루틴이 실행 중이면 정리
        StartCoroutine(ShowMessageRoutine(message, duration));
    }

    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = message;

            yield return new WaitForSeconds(duration);

            dialoguePanel.SetActive(false);
            dialogueText.text = "";
        }
    }
}