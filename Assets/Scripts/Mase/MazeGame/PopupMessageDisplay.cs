using UnityEngine;
using TMPro;
using System.Collections;

public class PopupMessageDisplay : MonoBehaviour
{
    public GameObject messageCanvas;
    public TextMeshProUGUI messageText;

    private Coroutine current;

    public void ShowMessage(string msg, float duration = 2f)
    {
        if (current != null)
            StopCoroutine(current);
        current = StartCoroutine(Show(msg, duration));
    }

    IEnumerator Show(string msg, float duration)
    {
        messageCanvas.SetActive(true);
        messageText.text = msg;
        yield return new WaitForSeconds(duration);
        messageCanvas.SetActive(false);
        current = null;
    }
}