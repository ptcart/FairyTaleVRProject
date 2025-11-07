using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text storyNarrationText; // Inspector에서 직접 연결

    private void Awake()
    {
        Instance = this;
    }
}