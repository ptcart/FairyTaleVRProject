using UnityEngine;
using System.Linq; // OrderBy 사용하려면 필요

[RequireComponent(typeof(LineRenderer))]
public class RayItemInteractor : MonoBehaviour
{
    public float rayDistance = 10f;
    public LayerMask itemLayer; // 아이템 전용 레이어 (Butter, Pepper, Bucket)

    private LineRenderer lineRenderer;
    private CollectableStoryItem lastTarget; // 마지막으로 맞은 아이템

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // 🎯 Ray 그리기 (시각적으로 보이도록)
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + transform.forward * rayDistance);

        Ray ray = new Ray(transform.position, transform.forward);

        // ✅ RaycastAll 사용 → 화면에 여러 아이템이 있어도 가장 가까운 것만 선택
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, itemLayer);

        if (hits.Length > 0)
        {
            // 거리순으로 정렬 → 제일 가까운 아이템만 타겟
            RaycastHit nearest = hits.OrderBy(h => h.distance).First();

            var item = nearest.collider.GetComponent<CollectableStoryItem>();

            if (item != null)
            {
                // 이전 타겟이 다른 아이템이었다면 false 처리
                if (lastTarget != null && lastTarget != item)
                {
                    lastTarget.SetTargeted(false);
                }

                // 새 아이템을 타겟으로 지정
                item.SetTargeted(true);
                lastTarget = item;
            }
        }
        else
        {
            // 아무 것도 맞지 않으면 마지막 타겟 해제
            if (lastTarget != null)
            {
                lastTarget.SetTargeted(false);
                lastTarget = null;
            }
        }
    }
}