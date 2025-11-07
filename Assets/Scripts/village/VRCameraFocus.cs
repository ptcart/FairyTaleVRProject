using UnityEngine;

public class VRCameraFocus : MonoBehaviour
{
    public Transform vrCamera;  // 📌 OVRCameraRig 안의 CenterEyeAnchor 또는 MainCamera
    public float distance = 1.2f;  // 📏 NPC 앞에서 카메라가 멈출 거리

    public void FocusOnNPC(Transform npc)
    {
        // 🧭 NPC 정면 방향 기준 앞으로 distance만큼 떨어진 위치 계산
        Vector3 focusPos = npc.position + npc.forward * -distance + Vector3.up * 1.6f;  // 살짝 위로 보정
        vrCamera.position = focusPos;

        // 📸 NPC를 바라보도록 카메라 회전
        vrCamera.LookAt(npc.position + Vector3.up * 1.5f);
    }
}