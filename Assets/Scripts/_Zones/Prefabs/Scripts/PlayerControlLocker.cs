using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ✅ 플레이어의 조작(이동/그랩 등) 잠금을 처리하는 클래스
/// </summary>
public class PlayerControlLocker : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> controlScripts = new List<MonoBehaviour>();

    public void LockForSeconds(float seconds)
    {
        StartCoroutine(LockRoutine(seconds));
    }

    private IEnumerator LockRoutine(float seconds)
    {
        // 지정된 스크립트들을 잠시 꺼둠
        foreach (var script in controlScripts)
        {
            if (script != null) script.enabled = false;
        }

        yield return new WaitForSeconds(seconds);

        // 다시 활성화
        foreach (var script in controlScripts)
        {
            if (script != null) script.enabled = true;
        }
    }
}