using UnityEngine;

public class ClearSaveOnStartup : MonoBehaviour
{
    private static bool hasCleared = false; // 🔒 한 번만 실행하도록 플래그

    void Start()
    {
        // 이미 한 번 실행됐다면, 더 이상 실행하지 않음
        if (hasCleared)
            return;

        // 🔥 최초 실행 시에만 저장 초기화
        SaveManager.ClearSave();
        hasCleared = true;

        Debug.Log("🧹 새 실행 시작 → 이전 세이브 데이터 초기화 완료 (한 번만 실행됨)");
    }
}