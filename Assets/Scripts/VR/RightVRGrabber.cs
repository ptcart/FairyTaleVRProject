using UnityEngine;
using System.Collections;

/// <summary>
/// VR 컨트롤러에서 Ray를 쏴서 장애물(CollectableItem)을 그랩/릴리즈
/// - FixedJoint로 연결해서 부드럽게 끌기
/// - 릴리즈 시 속도 적용 (던지기 효과)
/// </summary>
public class RightVRGrabber : MonoBehaviour
{
    [Header("레이 설정")]
    public Transform rayOrigin;                // 레이 시작점 (예: RightRay)
    public float rayDistance = 3f;             // 감지 최대 거리
    public LayerMask interactableLayer;        // 잡을 수 있는 레이어

    [Header("입력 버튼")]
    public OVRInput.Button grabButton = OVRInput.Button.SecondaryHandTrigger;

    private Rigidbody grabbedRb;               // 현재 잡은 오브젝트의 Rigidbody
    private FixedJoint grabJoint;              // 손과 오브젝트를 연결하는 Joint

    void Start()
    {
        // rayOrigin에 Rigidbody가 없으면 추가하고 설정
        Rigidbody rb = rayOrigin.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = rayOrigin.gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;     // 물리 충돌은 되지만 위치는 트래킹으로 이동
        rb.useGravity = false;
    }

    void Update()
    {
        if (OVRInput.GetDown(grabButton)) TryGrab();
        if (OVRInput.GetUp(grabButton)) Release();
    }

    /// <summary>
    /// Ray를 쏴서 잡을 수 있는 오브젝트가 있으면 FixedJoint로 연결
    /// </summary>
    void TryGrab()
    {
        if (grabJoint != null || grabbedRb != null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayer))
        {
            var item = hit.collider.GetComponent<CollectableItem>();
            if (item == null) return;

            // ❗❗ BigRock은 한 손으로 못 잡도록 필터링
            if (item.itemType == ObstacleType.BigRock)
            {
                Debug.Log("❌ BigRock은 양손으로만 들 수 있습니다!");
                return;
            }

            grabbedRb = item.GetComponent<Rigidbody>();
            if (grabbedRb == null) return;

            grabJoint = rayOrigin.gameObject.AddComponent<FixedJoint>();
            grabJoint.connectedBody = grabbedRb;

            grabbedRb.useGravity = false;
            grabbedRb.velocity = Vector3.zero;
            grabbedRb.angularVelocity = Vector3.zero;

            Debug.Log($"✅ Grabbed: {grabbedRb.name}");
        }
    }


    /// <summary>
    /// 연결된 Joint 해제 + 물리 적용
    /// </summary>
    void Release()
    {
        if (grabJoint != null)
        {
            Destroy(grabJoint);
            grabJoint = null;
        }

        if (grabbedRb != null)
        {
            // 중력 다시 활성화
            grabbedRb.useGravity = true;

            // 손의 속도를 오브젝트에 전달 (던지기 효과)
            grabbedRb.velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            grabbedRb.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);

            // 한 프레임 지연 후 null 처리 (FixedJoint 해제 타이밍 문제 방지)
            StartCoroutine(DelayedClearRigidbody());
        }
    }

    /// <summary>
    /// grabbedRb를 한 프레임 뒤에 null 처리
    /// </summary>
    IEnumerator DelayedClearRigidbody()
    {
        yield return null;
        grabbedRb = null;
    }
}
