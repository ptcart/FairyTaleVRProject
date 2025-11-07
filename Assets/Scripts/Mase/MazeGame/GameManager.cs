using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int breadCount = 0;
    public int totalBread = 3;

    public TextMeshProUGUI breadUIText; // "1/3" 표시용 UI Text
    public GameObject houseDoor; // 집 문 Collider 오브젝트

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CollectBread()
    {
        Debug.Log("🍞 열쇠 수집됨: CollectBread() 호출");
        breadCount++;
        breadUIText.text = $"{breadCount}/{totalBread}";
        
        // UI 텍스트 업데이트
        FindObjectOfType<KeyStatusDisplay>()?.UpdateBreadCount(breadCount, totalBread);

        if (breadCount >= totalBread)
        {
            houseDoor.GetComponent<Collider>().isTrigger = true; // 입장 가능하게 만듦
            Debug.Log("✅ 문이 열렸습니다! (isTrigger = true)");
            
            // ✅ 문 열림 메시지 띄우기
            FindObjectOfType<PopupMessageDisplay>()?.ShowMessage("문이 열렸어요!", 2f);
        }
    }
}