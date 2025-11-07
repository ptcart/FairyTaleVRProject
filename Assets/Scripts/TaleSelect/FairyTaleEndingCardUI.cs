using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FairyTaleEndingCardUI : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text titleText;      // 제목
    public Image statusIcon;        // 체크/느낌표 아이콘

    [Header("Icons")]
    public Sprite checkSprite;      // 완료 시 아이콘
    public Sprite exclamationSprite; // 미완료 시 아이콘

    /// <summary>
    /// 카드 내용 세팅 (제목 + 완료 여부)
    /// </summary>
   
    
    
    public void Setup(string title, bool isCompleted)
    {
        if (titleText != null)
            titleText.text = title;

        SetStatusIcon(isCompleted);
    }

    /// <summary>
    /// 완료 상태에 따라 아이콘 교체
    /// </summary>
    public void SetStatusIcon(bool completed)
    {
        if (statusIcon == null) return;

        if (completed)
        {
            statusIcon.sprite = checkSprite;
            statusIcon.color = new Color(0.43f, 0.91f, 0.65f); // 연두색
        }
        else
        {
            statusIcon.sprite = exclamationSprite;
            statusIcon.color = new Color(0.85f, 0.85f, 0.85f); // 회색
        }
    }
}