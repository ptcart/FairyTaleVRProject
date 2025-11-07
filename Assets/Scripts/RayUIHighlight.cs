using UnityEngine;
using UnityEngine.UI;

public class RayUIHighlight : MonoBehaviour
{
    private Image buttonImage;
    //private Color originalColor;

    //public Color highlightColor = Color.yellow;
    
    public Sprite normalSprite;    // ✅ 기본 스프라이트
    public Sprite highlightSprite; // ✅ 하이라이트 스프라이트

    void Start()
    {
        // buttonImage = GetComponent<Image>();
        // if (buttonImage != null)
        // {
        //     originalColor = buttonImage.color;
        // }
        buttonImage = GetComponent<Image>();
    }

    public void Highlight(bool isHovering)
    {
        // if (buttonImage == null) return;
        //
        // buttonImage.color = isHovering ? highlightColor : originalColor;
        if (buttonImage == null) return;

        buttonImage.sprite = isHovering ? highlightSprite : normalSprite;
    }
}