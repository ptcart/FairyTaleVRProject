using UnityEngine;

public class EndingResetSave : MonoBehaviour
{
    void Start()
    {
        SaveManager.ClearSave();
        Debug.Log("🏁 엔딩 도달 → 저장 기록 초기화 완료");
    }
}