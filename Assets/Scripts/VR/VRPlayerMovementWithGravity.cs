using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class VRPlayerMovementWithGravity : MonoBehaviour
{
    public Transform vrCamera;                  // 카메라 방향 기준으로 이동
    public float moveSpeed = 3f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.1f;

    private CharacterController controller;
    private float verticalVelocity = 0f;
    private bool isGrounded;
    
    public float turnSpeed = 90f; // 회전 속도

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleSmoothTurn(); // ← 반드시 호출 필요
    }

    void HandleMovement()
    {
        // 👉 1. 바닥 감지
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f);

        // 👉 2. 중력 처리
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 👉 3. 컨트롤러 입력 받기 (오른쪽 조이스틱 기준)
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // 👉 4. 카메라 방향 기준으로 이동 벡터 계산
        Vector3 forward = vrCamera.forward;
        Vector3 right = vrCamera.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (right * input.x + forward * input.y) * moveSpeed;
        move.y = verticalVelocity;

        // 👉 5. 이동 적용
        controller.Move(move * Time.deltaTime);
    }
    
    void HandleSmoothTurn()
    {
        // 오른쪽 조이스틱 좌우 입력값 감지
        float turnInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        if (Mathf.Abs(turnInput) > 0.2f)
        {
            float turnAmount = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0f, turnAmount, 0f);
        }
    }
}