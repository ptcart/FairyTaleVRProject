using UnityEngine;

public class StartupManager : MonoBehaviour
{
    void Start()
    {
        // 💾 게임 시작 시 세이브 파일 유효성 검사 및 정리
        SaveManager.ValidateSaveAtStartup();
        Debug.Log("🧹 StartupManager: 세이브 데이터 검사 완료");
    }
}