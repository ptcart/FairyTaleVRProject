// 🔍 수거 존에 누가 들어왔는지 로그 확인용
using UnityEngine;

public class DumpZoneDebugger : MonoBehaviour
{
    public string zoneName = "Zone A";

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{zoneName}] ENTER: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[{zoneName}] EXIT: {other.name}");
    }
}