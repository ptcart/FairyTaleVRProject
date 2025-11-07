using UnityEngine;

public class RollingRockTrigger : MonoBehaviour
{
    [Tooltip("굴러올 바위 오브젝트 (비활성화 상태에서 시작됨)")]
    public GameObject rockObject;  // BigRock 오브젝트
    private RollingRock rollingRock;

    private bool hasTriggered = false;

    void Start()
    {
        // 처음에 비활성화되어 있으므로 컴포넌트 직접 참조 불가 → 나중에 가져옴
        if (rockObject != null && rockObject.activeSelf)
        {
            Debug.LogWarning("⚠️ rockObject는 시작할 때 비활성화되어 있어야 합니다!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        if (rockObject != null)
        {
            // 1️⃣ 오브젝트 켜기
            rockObject.SetActive(true);

            // 2️⃣ 컴포넌트 다시 가져오기
            rollingRock = rockObject.GetComponent<RollingRock>();

            // 3️⃣ Roll() 호출
            if (rollingRock != null)
            {
                rollingRock.Roll();
                Debug.Log("🪨 바위 활성화 + Roll 시작!");
            }
        }

        hasTriggered = true;
    }
}