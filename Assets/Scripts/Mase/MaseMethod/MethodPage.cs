using System.Collections;
using UnityEngine;

public class MethodPage : MonoBehaviour
{
    public GameObject[] methodPages;      // 설명 페이지들
    public GameObject previousButton;     // "이전" 버튼
    public GameObject nextButton;         // "다음" 버튼
    public GameObject startGameButton;    // "게임 시작" 버튼

    private int currentIndex = 0;

    public PageVideoController videoController; // Inspector에 드래그
    
    void Start()
    {
        ShowPage(0);
    }

    public void ShowNext()
    {
        if (currentIndex < methodPages.Length - 1)
        {
            StartCoroutine(NextWithDelay());
        }
    }

    public void ShowPrevious()
    {
        if (currentIndex > 0)
        {
            StartCoroutine(PreviousWithDelay());
        }
    }
    private IEnumerator NextWithDelay()
    {
        yield return new WaitForSeconds(0.3f); // ✅ 딜레이는 여기에!
        currentIndex++;
        ShowPage(currentIndex);
    }

    private IEnumerator PreviousWithDelay()
    {
        yield return new WaitForSeconds(0.3f); // ✅ 여기도 여기에만 딜레이 넣으면 끝!
        currentIndex--;
        ShowPage(currentIndex);
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < methodPages.Length; i++)
        {
            methodPages[i].SetActive(i == index);
        }

        // 버튼 상태 조절
        if (previousButton != null)
            previousButton.SetActive(index > 0);

        if (nextButton != null)
            nextButton.SetActive(index < methodPages.Length - 1);

        if (startGameButton != null)
            startGameButton.SetActive(index == methodPages.Length - 1);
        
        if (videoController != null)
            videoController.PlayVideoForPage(index);

        Debug.Log($"📄 현재 페이지: {index + 1} / {methodPages.Length}");
    }
}
