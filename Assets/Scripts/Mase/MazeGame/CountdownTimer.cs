using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float startTime = 180f; // 3분
    private float currentTime;
    private bool isRunning = true;

    void Start()
    {
        currentTime = startTime;
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0); // 음수 방지

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";

        if (currentTime <= 0)
        {
            isRunning = false;
            Debug.Log("⏰ 제한 시간 종료!");
            
            FindObjectOfType<ScreenFader>()?.StartFadeOut();
            // TODO: 실패 처리 or 씬 전환
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = startTime;
        isRunning = true;
    }
}