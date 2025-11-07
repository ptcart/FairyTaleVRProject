using UnityEngine;

/// <summary>
/// 머티리얼의 텍스처를 Y축으로 계속 움직이게 해서 줄이 위로 흐르도록 만듦
/// </summary>
public class BorderLineScroll : MonoBehaviour
{
    public float scrollSpeed = 0.5f;

    private Material mat;
    private Vector2 offset;

    void Start()
    {
        // 현재 오브젝트의 머티리얼 가져오기
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Y축 방향으로 텍스처 Offset 값을 증가
        offset.y += scrollSpeed * Time.deltaTime;
        mat.mainTextureOffset = offset;
    }
}