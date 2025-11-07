using UnityEngine;

/// <summary>
/// 조이스틱으로 플레이어를 이동시키고, 중력을 적용해 땅에 착지하도록 처리하는 스크립트
/// 반드시 CharacterController 컴포넌트와 함께 사용해야 함.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class VRJoystickMovement : MonoBehaviour
{
    public float movementSpeed = 3f;       // 플레이어 이동 속도
    public float rotationSpeed = 5f;       // 회전 속도 (현재 미사용)
    public float joystickDeadzone = 0.2f;  // 조이스틱 민감도 (입력이 이 값 이하이면 무시)
    public float gravity = -9.81f;         // 중력 가속도 (음수로 설정)

    private Vector3 moveDirection;         // 이동 방향 벡터
    private Vector3 velocity;              // 중력 방향 포함한 최종 속도
    private Transform cameraTransform;     // 시야 기준 방향 (카메라)
    private CharacterController controller; // Unity 내장 충돌+중력 처리 컴포넌트

    void Start()
    {
        // Main Camera를 기준으로 플레이어가 바라보는 방향을 저장
        cameraTransform = Camera.main.transform;

        // 이 오브젝트에 붙은 CharacterController 컴포넌트 가져오기
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement(); // 조이스틱 이동 처리
        HandleGravity();  // 중력 적용
    }

    // 🟢 조이스틱 이동 처리
    void HandleMovement()
    {
        // 왼쪽 조이스틱 입력값 받아오기 (X: 좌우, Y: 앞뒤)
        float horizontal = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        float vertical = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

        // 입력값이 Deadzone 이상이면 이동 시작
        if (Mathf.Abs(horizontal) > joystickDeadzone || Mathf.Abs(vertical) > joystickDeadzone)
        {
            // 카메라 기준 방향 계산 (수평 방향만 사용)
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f; // 수직방향 제거
            camRight.y = 0f;

            // 최종 이동 방향 계산
            moveDirection = (camForward.normalized * vertical + camRight.normalized * horizontal);

            // CharacterController를 통해 이동
            controller.Move(moveDirection * movementSpeed * Time.deltaTime);
        }
    }

    // 🔵 중력 적용 처리
    void HandleGravity()
    {
        // 땅에 닿아 있지 않으면 중력 계속 적용
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            // 땅에 있을 땐 Y속도를 작게 유지 (무한 점프 방지)
            velocity.y = -1f;
        }

        // 중력 방향 이동 적용
        controller.Move(velocity * Time.deltaTime);
    }
}
