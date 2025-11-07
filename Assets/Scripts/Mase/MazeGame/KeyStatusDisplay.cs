using UnityEngine;
using TMPro;

public class KeyStatusDisplay : MonoBehaviour
{
    public GameObject breadCanvas; // ← BreadStatusCanvas 오브젝트 지정
    public TextMeshProUGUI breadText;

    private void Update()
    {
        // A 버튼 누르고 있는 동안만 보임
        if (OVRInput.Get(OVRInput.Button.One))
        {
            breadCanvas.SetActive(true);
        }
        else
        {
            breadCanvas.SetActive(false);
        }
    }

    public void UpdateBreadCount(int count, int max)
    {
        breadText.text = count + "/" + max;
    }
}
