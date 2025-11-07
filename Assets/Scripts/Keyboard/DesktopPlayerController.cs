using UnityEngine;

/// <summary>
/// CharacterController 기반의 키보드+마우스 이동 + 중력 처리 컨트롤러
/// 경사, 낙하, 미끄러짐 현상 없이 매우 부드럽고 안정적인 구조
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class DesktopPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 100f;
    public float gravity = -20f;
    public Transform cameraTransform;

    private CharacterController controller;
    private float cameraPitch = 0f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(inputX, 0, inputZ).normalized;

        // 카메라 기준으로 이동 방향 계산
        Vector3 move = (transform.right * inputX + transform.forward * inputZ).normalized;

        controller.Move(move * moveSpeed * Time.deltaTime);

        // 중력 적용
        if (controller.isGrounded)
        {
            velocity.y = -2f;  // 착지 시 작은 음수로 유지
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}