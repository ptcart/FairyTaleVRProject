using UnityEngine;

public class FollowCameraText : MonoBehaviour
{
    public Transform cameraTransform;
    public float distance = 2.0f;  // 카메라 앞 거리
    public float heightOffset = 0.0f;  // 눈높이 기준 높이 조정

    void Update()
    {
        if (cameraTransform == null) return;

        // 카메라 정면 방향으로 일정 거리만큼 떨어진 위치 계산
        Vector3 forwardPosition = cameraTransform.position + cameraTransform.forward * distance;

        // 높이 보정
        forwardPosition.y = cameraTransform.position.y + heightOffset;

        transform.position = forwardPosition;

        // 카메라 쪽 바라보도록 회전
        transform.LookAt(cameraTransform);
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}