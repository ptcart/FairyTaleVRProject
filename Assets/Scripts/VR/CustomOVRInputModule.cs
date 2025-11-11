using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomOVRInputModule : PointerInputModule
{
    [Header("컨트롤러에서 Ray를 쏠 위치 (예: RightRay)")]
    public Transform rayTransform;

    [Header("클릭 버튼 설정 (Trigger = SecondaryIndexTrigger, A버튼 = One)")]
    public OVRInput.Button clickButton = OVRInput.Button.SecondaryIndexTrigger;

    private OVRPointerEventData pointerData;
    private GameObject lastOpenedDropdown; // 현재 열린 드롭다운 저장용

    public override void Process()
    {
        // ✅ RightRay 자동 재바인딩 (씬 전환 후 Missing 방지)
        if (rayTransform == null)
        {
            var found = GameObject.Find("RightRay");
            if (found != null)
            {
                rayTransform = found.transform;
                Debug.Log($"🔄 [CustomOVRInputModule] RightRay 재연결 완료: {found.name}");
            }
            else
            {
                // 아직 생성 안 된 경우 다음 프레임에서 다시 시도
                Debug.LogWarning("⚠️ [CustomOVRInputModule] RightRay를 찾지 못했습니다. (씬 생성 지연 가능성)");
                return;
            }
        }

        if (pointerData == null)
            pointerData = new OVRPointerEventData(eventSystem);
        else
            pointerData.Reset();
        
        if (rayTransform == null) return;

        if (pointerData == null)
            pointerData = new OVRPointerEventData(eventSystem);
        else
            pointerData.Reset();

        pointerData.button = PointerEventData.InputButton.Left;

        // ✅ Raycast 생성
        pointerData.worldSpaceRay = new Ray(rayTransform.position, rayTransform.forward);

        // 🔹 Raycast
        eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
        var raycast = FindFirstRaycast(m_RaycastResultCache);
        pointerData.pointerCurrentRaycast = raycast;
        m_RaycastResultCache.Clear();

        GameObject currentOverGo = raycast.gameObject;

        // ✅ 여기서 핵심: Ray 충돌 지점을 Canvas 기준 2D 위치로 변환
        if (raycast.module != null && raycast.module.eventCamera != null)
        {
            Vector3 worldPos = raycast.worldPosition;
            Vector2 screenPos = raycast.module.eventCamera.WorldToScreenPoint(worldPos);
            pointerData.position = screenPos;
        }

        HandlePointerExitAndEnter(pointerData, currentOverGo);

        bool pressed = OVRInput.GetDown(clickButton);
        bool held = OVRInput.Get(clickButton);
        bool released = OVRInput.GetUp(clickButton);
        bool onePressed = OVRInput.GetDown(OVRInput.Button.One);
        bool oneHeld = OVRInput.Get(OVRInput.Button.One);
        bool oneReleased = OVRInput.GetUp(OVRInput.Button.One);

        // 🔹 기존 트리거 (그대로 유지)
        if (pressed)
            ProcessPress(pointerData);

        if (held && pointerData.pointerDrag != null)
        {
            if (pointerData.pointerDrag.GetComponent<UnityEngine.UI.Slider>() != null)
            {
                if (!pointerData.dragging)
                {
                    ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.beginDragHandler);
                    pointerData.dragging = true;
                }
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
            }
        }

        if (released)
            ProcessRelease(pointerData);


        // ✅ A버튼 (버튼/토글은 즉시 확정, 슬라이더는 드래그 유지)
        if (onePressed)
        {
            var target = pointerData.pointerCurrentRaycast.gameObject;

            // 만약 Toggle 또는 Button이면 즉시 클릭 확정
            if (target != null && 
                (target.GetComponent<UnityEngine.UI.Toggle>() != null ||
                 target.GetComponent<UnityEngine.UI.Button>() != null))
            {
                ProcessPress(pointerData);
                ProcessRelease(pointerData);  // 즉시 클릭 확정 ✅
            }
            else
            {
                // 슬라이더 등은 기존 방식 유지
                ProcessPress(pointerData);
            }
        }

        // A버튼 누르고 있는 동안 드래그 유지 (슬라이더 전용)
        if (oneHeld && pointerData.pointerDrag != null)
        {
            if (pointerData.pointerDrag.GetComponent<UnityEngine.UI.Slider>() != null)
            {
                if (!pointerData.dragging)
                {
                    ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.beginDragHandler);
                    pointerData.dragging = true;
                }
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
            }
        }

        // A버튼 뗄 때 (단, 버튼/토글은 이미 클릭됐으므로 Release 생략)
        if (oneReleased && pointerData.pointerDrag != null)
        {
            ProcessRelease(pointerData);
        }


    }
    
    /// <summary>
    /// ✅ VR Ray를 이용해 TMP_Dropdown 항목 클릭 인식 개선
    /// </summary>
    private void HandleDropdownItemSelect(PointerEventData data)
    {
        if (data.pointerCurrentRaycast.gameObject == null)
        {
            Debug.Log("❌ Ray가 드롭다운 항목에 닿지 않음");
            return;
        }

        GameObject clicked = data.pointerCurrentRaycast.gameObject;
        Debug.Log($"🎯 A버튼 클릭 감지됨 → Ray Hit 대상: {clicked.name}");

        // 🔹 1️⃣ Toggle 찾기 (현재 오브젝트 + 상위 부모 탐색)
        UnityEngine.UI.Toggle toggle = clicked.GetComponent<UnityEngine.UI.Toggle>();
        if (toggle == null)
        {
            toggle = clicked.GetComponentInParent<UnityEngine.UI.Toggle>();
        }

        // 🔹 2️⃣ Toggle 클릭 이벤트 실행
        if (toggle != null)
        {
            Debug.Log($"✅ Toggle 항목 감지됨: {toggle.name}");

            // TMP_Dropdown 내부 토글 강제 선택 (IsOn 토글 + 이벤트 발생)
            toggle.isOn = true;

            ExecuteEvents.Execute(toggle.gameObject, data, ExecuteEvents.submitHandler);
            ExecuteEvents.Execute(toggle.gameObject, data, ExecuteEvents.pointerClickHandler);

            // 🔹 3️⃣ 드롭다운 닫기
            var tmpDropdown = toggle.GetComponentInParent<TMPro.TMP_Dropdown>();
            if (tmpDropdown != null)
            {
                Debug.Log($"📘 TMP_Dropdown 닫기 시도: {tmpDropdown.captionText.text}");
                tmpDropdown.Hide();
            }

            var dropdown = toggle.GetComponentInParent<UnityEngine.UI.Dropdown>();
            if (dropdown != null)
            {
                Debug.Log($"📗 Dropdown 닫기 시도: {dropdown.captionText.text}");
                dropdown.Hide();
            }

            return;
        }

        // 🔹 4️⃣ 일반 버튼일 경우
        var button = clicked.GetComponentInParent<UnityEngine.UI.Button>();
        if (button != null)
        {
            Debug.Log($"✅ Button 감지됨: {button.name}");
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
            return;
        }

        Debug.Log("⚠️ Toggle/Button 감지 실패 → Raycast 대상 확인 필요");
    }




    private void ProcessPress(PointerEventData data)
    {
        data.eligibleForClick = true;
        data.delta = Vector2.zero;
        data.dragging = false;
        data.useDragThreshold = true;
        data.pressPosition = data.position;
        data.pointerPressRaycast = data.pointerCurrentRaycast;

        GameObject currentOverGo = data.pointerCurrentRaycast.gameObject;

        // 🔹 클릭 가능한 오브젝트
        GameObject newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, data, ExecuteEvents.pointerDownHandler);
        if (newPressed == null)
            newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

        data.pointerPress = newPressed;
        data.rawPointerPress = currentOverGo;

        // 🔹 드래그 가능한 오브젝트 (슬라이더 등)
        data.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
        if (data.pointerDrag != null)
            ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.initializePotentialDrag);
    }

    private void ProcessRelease(PointerEventData data)
    {
        ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(
            data.pointerCurrentRaycast.gameObject);

        GameObject target = data.pointerPress ?? pointerUpHandler;

        if (target != null)
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);

        // ✅ [여기에 추가] 드롭다운 항목 클릭 처리
        ExecuteEvents.Execute(data.pointerCurrentRaycast.gameObject, data, ExecuteEvents.submitHandler);
        
        // ✅ 드롭다운 자동 닫기
        HandleDropdownClose(data, target);

        // 🔹 드래그 종료 처리
        if (data.pointerDrag != null)
        {
            ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.dropHandler);
            if (data.dragging)
                ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.endDragHandler);
        }

        data.eligibleForClick = false;
        data.pointerPress = null;
        data.rawPointerPress = null;
        data.pointerDrag = null;
        data.dragging = false;

        HandlePointerExitAndEnter(data, null);
    }

    /// <summary>
    /// ✅ 드롭다운 자동 닫힘 (TMP_Dropdown / Dropdown 모두 지원)
    /// </summary>
    private void HandleDropdownClose(PointerEventData data, GameObject clickedObject)
    {
        if (clickedObject != null)
        {
            var dropdown = clickedObject.GetComponentInParent<UnityEngine.UI.Dropdown>();
            var tmpDropdown = clickedObject.GetComponentInParent<TMPro.TMP_Dropdown>();

            if (dropdown != null)
            {
                if (lastOpenedDropdown == dropdown.gameObject)
                {
                    dropdown.Hide();
                    lastOpenedDropdown = null;
                    return;
                }

                lastOpenedDropdown = dropdown.gameObject;
                return;
            }
            else if (tmpDropdown != null)
            {
                if (lastOpenedDropdown == tmpDropdown.gameObject)
                {
                    tmpDropdown.Hide();
                    lastOpenedDropdown = null;
                    return;
                }

                lastOpenedDropdown = tmpDropdown.gameObject;
                return;
            }
        }

        if (lastOpenedDropdown != null)
        {
            var dropdown = lastOpenedDropdown.GetComponent<UnityEngine.UI.Dropdown>();
            var tmpDropdown = lastOpenedDropdown.GetComponent<TMPro.TMP_Dropdown>();

            if (dropdown != null)
                dropdown.Hide();
            else if (tmpDropdown != null)
                tmpDropdown.Hide();

            lastOpenedDropdown = null;
        }
    }
}
