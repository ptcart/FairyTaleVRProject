using UnityEngine;
using TMPro;

public class ProgressDisplay : MonoBehaviour
{
    [Header("📋 연결할 요소들")]
    public GameObject progressUI;               // World Space Canvas 전체
    public TMP_Text progressText;               // TMP 텍스트
    public MissionController missionController; // MissionController 참조

    void Start()
    {
        if (progressUI != null)
        {
            progressUI.SetActive(false);
            Debug.Log("📦 ProgressUI 시작 시 비활성화됨");
        }
    }

    void Update()
    {
        //Debug.Log("✅ ProgressDisplay.Update() 호출 중");
        
        // 누르고 있는 중
        if (OVRInput.Get(OVRInput.Button.One))
        {
            if (!progressUI.activeSelf)
            {
                Debug.Log("🟢 A 버튼 눌림 → ProgressUI 활성화");
                progressUI.SetActive(true);
                UpdateText(); // 텍스트 갱신
            }
        }
        else
        {
            if (progressUI.activeSelf)
            {
                Debug.Log("🔴 A 버튼에서 손 뗌 → ProgressUI 비활성화");
                progressUI.SetActive(false);
            }
        }
    }

    void UpdateText()
    {
        string zoneId = missionController.currentZoneId;
        string detail = missionController.GetDetailedZoneProgressText(zoneId);
        progressText.text = detail;

        Debug.Log($"📋 진행도 텍스트 갱신:\n{detail}");
    }
}