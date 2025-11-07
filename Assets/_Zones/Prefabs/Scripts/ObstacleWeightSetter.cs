using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CollectableItem))]
public class ObstacleWeightSetter : MonoBehaviour
{
    void Start()
    {
        var rb = GetComponent<Rigidbody>();
        var item = GetComponent<CollectableItem>();

        switch (item.itemType)
        {
            case ObstacleType.Rock:
                rb.mass = 10f;
                rb.drag = 5f;
                rb.angularDrag = 5f;
                break;

            case ObstacleType.Log:
                rb.mass = 12f;
                rb.drag = 6f;
                rb.angularDrag = 6f;
                break;

            case ObstacleType.BigRock:
                rb.mass = 20f; // 실제 적용되진 않지만 일단 무겁게
                rb.drag = 8f;
                rb.angularDrag = 8f;
                break;
        }

        Debug.Log($"⚖️ {gameObject.name} 무게 설정됨: {rb.mass} (타입: {item.itemType})");
    }
}