// using UnityEngine;
//
// /// <summary>
// /// BigRock을 양손으로만 Grab할 수 있도록 제어하는 전용 스크립트
// /// </summary>
// public class BigRockGrabber : MonoBehaviour
// {
//     [Header("손 위치 설정")]
//     public Transform leftRayOrigin;
//     public Transform rightRayOrigin;
//
//     [Header("감지 설정")]
//     public float rayDistance = 3f;
//     public LayerMask bigRockLayer;
//
//     [Header("입력 버튼")]
//     public OVRInput.Button leftGrabButton = OVRInput.Button.PrimaryHandTrigger;
//     public OVRInput.Button rightGrabButton = OVRInput.Button.SecondaryHandTrigger;
//
//     private Rigidbody grabbedRb;
//     private FixedJoint grabJoint;
//
//     void Update()
//     {
//         bool leftPressed = OVRInput.Get(leftGrabButton);
//         bool rightPressed = OVRInput.Get(rightGrabButton);
//
//         if (grabbedRb == null && leftPressed && rightPressed)
//         {
//             TryGrabBigRock();
//         }
//         else if (grabbedRb != null && (!leftPressed || !rightPressed))
//         {
//             Release();
//         }
//     }
//
//     void TryGrabBigRock()
//     {
//         // 두 손 모두 Ray 쏨
//         bool leftHitCheck = Physics.Raycast(leftRayOrigin.position, leftRayOrigin.forward, out RaycastHit leftHit, rayDistance, bigRockLayer);
//         bool rightHitCheck = Physics.Raycast(rightRayOrigin.position, rightRayOrigin.forward, out RaycastHit rightHit, rayDistance, bigRockLayer);
//
//         if (!leftHitCheck || !rightHitCheck) return;
//
//         // 양쪽이 같은 오브젝트를 보고 있는지 확인
//         if (leftHit.collider.gameObject != rightHit.collider.gameObject) return;
//
//         // BigRock인지 확인
//         var item = leftHit.collider.GetComponent<CollectableItem>();
//         if (item == null || item.itemType != ObstacleType.BigRock) return;
//
//         grabbedRb = item.GetComponent<Rigidbody>();
//         if (grabbedRb == null) return;
//
//         // 오른손 기준으로 FixedJoint 생성
//         grabJoint = rightRayOrigin.gameObject.AddComponent<FixedJoint>();
//         grabJoint.connectedBody = grabbedRb;
//
//         // 물리 안정화
//         grabbedRb.useGravity = false;
//         grabbedRb.velocity = Vector3.zero;
//         grabbedRb.angularVelocity = Vector3.zero;
//
//         Debug.Log("✅ BigRock 양손으로 Grab 성공");
//     }
//
//     void Release()
//     {
//         if (grabJoint != null)
//         {
//             Destroy(grabJoint);
//             grabJoint = null;
//         }
//
//         if (grabbedRb != null)
//         {
//             // 중력 재적용 + 던지기 효과
//             grabbedRb.useGravity = true;
//             grabbedRb.velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
//             grabbedRb.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);
//
//             Debug.Log("🔚 BigRock Release");
//             grabbedRb = null;
//         }
//     }
// }

using UnityEngine;

/// <summary>
/// 양손으로만 BigRock을 들 수 있도록 하며,
/// 두 손 사이의 중간 위치로 AddForce 방식으로 끌어당김
/// </summary>
using UnityEngine;

/// <summary>
/// BigRock을 양손으로 Grab할 수 있게 하며,
/// GrabCenter(두 손 사이의 중간 지점)에 FixedJoint를 생성해 BigRock을 제어
/// </summary>
public class BigRockGrabber : MonoBehaviour
{
    [Header("손 설정")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Ray 설정")]
    public float rayDistance = 3f;
    public LayerMask bigRockLayer;

    [Header("입력 버튼")]
    public OVRInput.Button leftGrabButton = OVRInput.Button.PrimaryHandTrigger;
    public OVRInput.Button rightGrabButton = OVRInput.Button.SecondaryHandTrigger;

    private GameObject grabCenter;       // 두 손 중간 지점에 생성되는 기준 오브젝트
    private Rigidbody grabbedRb;         // 현재 잡은 BigRock
    private FixedJoint grabJoint;

    private bool isGrabbing = false;

    void Update()
    {
        bool leftPressed = OVRInput.Get(leftGrabButton);
        bool rightPressed = OVRInput.Get(rightGrabButton);

        if (!isGrabbing && leftPressed && rightPressed)
        {
            TryGrab();
        }
        else if (isGrabbing && (!leftPressed || !rightPressed))
        {
            Release();
        }

        // Grab 중일 때 GrabCenter의 위치를 계속 갱신
        if (isGrabbing && grabCenter != null)
        {
            Vector3 midPoint = (leftHand.position + rightHand.position) * 0.5f;
            Quaternion midRotation = Quaternion.Slerp(leftHand.rotation, rightHand.rotation, 0.5f);

            grabCenter.transform.position = midPoint;
            grabCenter.transform.rotation = midRotation;
        }
    }

    void TryGrab()
    {
        // 두 손 각각 Ray 쏴서 같은 BigRock을 보고 있는지 확인
        bool leftHit = Physics.Raycast(leftHand.position, leftHand.forward, out RaycastHit lHit, rayDistance, bigRockLayer);
        bool rightHit = Physics.Raycast(rightHand.position, rightHand.forward, out RaycastHit rHit, rayDistance, bigRockLayer);

        if (!leftHit || !rightHit) return;
        if (lHit.collider.gameObject != rHit.collider.gameObject) return;

        var item = lHit.collider.GetComponent<CollectableItem>();
        if (item == null || item.itemType != ObstacleType.BigRock) return;

        grabbedRb = item.GetComponent<Rigidbody>();
        if (grabbedRb == null) return;

        // ✅ GrabCenter 생성
        grabCenter = new GameObject("GrabCenter");
        grabCenter.transform.position = (leftHand.position + rightHand.position) * 0.5f;
        grabCenter.transform.rotation = Quaternion.Slerp(leftHand.rotation, rightHand.rotation, 0.5f);

        Rigidbody centerRb = grabCenter.AddComponent<Rigidbody>();
        centerRb.isKinematic = true;

        // ✅ FixedJoint 연결
        grabJoint = grabCenter.AddComponent<FixedJoint>();
        grabJoint.connectedBody = grabbedRb;

        // 무게감 설정
        grabbedRb.useGravity = true;
        grabbedRb.velocity = Vector3.zero;
        grabbedRb.angularVelocity = Vector3.zero;

        isGrabbing = true;
        Debug.Log("✅ BigRock Grab 성공 (GrabCenter 기준)");
    }

    void Release()
    {
        if (grabJoint != null)
        {
            Destroy(grabJoint);
            grabJoint = null;
        }

        if (grabCenter != null)
        {
            Destroy(grabCenter);
            grabCenter = null;
        }

        if (grabbedRb != null)
        {
            grabbedRb.useGravity = true;

            // 던지기 효과
            grabbedRb.velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            grabbedRb.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);

            grabbedRb = null;
        }

        isGrabbing = false;
        Debug.Log("🔚 BigRock Release");
    }
}
