using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // UI를 위한 네임스페이스

[RequireComponent(typeof(LineRenderer))]
public class RayUIInteractor : MonoBehaviour
{
    public float rayDistance = 10f;
    public LayerMask uiLayer;
    private LineRenderer lineRenderer;
    private GameObject lastHitObject;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // Ray의 시작과 끝 위치를 설정
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + transform.forward * rayDistance);

        // Ray 발사
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // UI Layer에서만 Raycast
        if (Physics.Raycast(ray, out hit, rayDistance, uiLayer))
        {
            GameObject currentHit = hit.collider.gameObject;

            // 로그 출력
            //Debug.Log("Ray hit: " + currentHit.name);

            // ✅ 새로운 버튼을 가리키면 이전 버튼 하이라이트 해제
            if (lastHitObject != currentHit)
            {
                if (lastHitObject != null)
                {
                    var oldHighlight = lastHitObject.GetComponent<RayUIHighlight>();
                    if (oldHighlight != null)
                        oldHighlight.Highlight(false);  // 원래 색으로 복원
                }

                lastHitObject = currentHit;

                var newHighlight = currentHit.GetComponent<RayUIHighlight>();
                if (newHighlight != null)
                    newHighlight.Highlight(true);  // 노란색 하이라이트
            }

            // ✅ 버튼 누르기
            if (OVRInput.GetDown(OVRInput.Button.One))  // 'One' 버튼을 눌렀을 때
            {
                Debug.Log("One 버튼 눌림");
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Camera.main.WorldToScreenPoint(hit.point);

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                // `foreach` 바깥에서 버튼 클릭 처리
                foreach (var result in results)
                {
                    // 클릭 이벤트 처리
                    ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                }

                // 버튼 클릭 후 동작 처리
                // 여기서 각 버튼의 클래스로 넘어가도록 처리
                OnButtonClicked(currentHit);  // 버튼 클릭 시 동작 실행
            }
        }
        else
        {
            // ✅ 버튼을 가리키지 않으면 하이라이트 해제
            if (lastHitObject != null)
            {
                var highlight = lastHitObject.GetComponent<RayUIHighlight>();
                if (highlight != null)
                    highlight.Highlight(false);  // 원래 색으로 복원
                lastHitObject = null;
            }
        }
    }

    // 버튼 클릭 시 텍스트 변경 또는 씬 전환
    void OnButtonClicked(GameObject clickedObject)
    {
        if (clickedObject.CompareTag("UIButton")) // 버튼에 Tag가 "UIButton"이어야 함
        {
            IButtonAction buttonAction = clickedObject.GetComponent<IButtonAction>();
            if (buttonAction != null)
            {
                buttonAction.OnButtonClick();  // 버튼 클릭 시 동작 실행
            }
        }
    }
}
