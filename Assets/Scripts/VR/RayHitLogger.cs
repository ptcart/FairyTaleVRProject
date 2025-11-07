using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayHitLogger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"✅ Ray가 버튼 [{gameObject.name}] 에 닿았어요! (충돌체: {other.name})");
    }
}
