using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class VRCardClickHandler : MonoBehaviour
{
    public AudioClip clickSound;
    [Range(0f, 1f)] public float volume = 1.0f;
    
    public OVRInput.Button selectButton = OVRInput.Button.One; // One 버튼
    public OVRInput.Controller controller = OVRInput.Controller.RTouch; // 오른손

    public Camera uiCamera; // UI용 카메라
    [FormerlySerializedAs("fairyTaleSelector")] public FairyTaleUIManager fairyTaleUIManager; // 상세정보 보여줄 객체

    void Update()
    {
        if (OVRInput.GetDown(selectButton, controller))
        {
            Ray ray = new Ray(transform.position, transform.forward); // 컨트롤러 기준 Ray
            if (Physics.Raycast(ray, out RaycastHit hit, 20f))
            {
                GameObject hitObject = hit.collider.gameObject;

                FairyTaleCardUI cardUI = hitObject.GetComponent<FairyTaleCardUI>();
                if (cardUI != null)
                {
                    if (!cardUI.IsInViewport())
                    {
                        Debug.Log("🚫 Viewport 밖의 카드 클릭 무시됨");
                        return;
                    }

                    PlayClickSound();
                    
                    // ✅ 상세 + 선택 처리까지 포함!
                    fairyTaleUIManager.OnCardSelected(cardUI);
                    
                    
                }
            }
        }
        
    }
    
    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, volume);
        }
    }
}