using UnityEngine;
using UnityEngine.UI; // Button 컴포넌트를 사용하기 위해 추가

[RequireComponent(typeof(Button))] // 이 스크립트는 Button이 있어야만 추가되도록 강제
public class UIButtonClickSound1 : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;
    private Button button;

    void Awake()
    {
        // 오디오소스 자동 생성 및 설정
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 사운드
        audioSource.volume = 0.7f;
        audioSource.clip = clickSound; // 미리 오디오 클립을 지정

        // 버튼 컴포넌트를 가져옴
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        // 버튼의 OnClick 이벤트에 PlayClickSound 함수를 자동으로 등록
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 등록했던 이벤트를 정리
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    // 이 함수가 버튼 클릭 시 호출됩니다.
    public void PlayClickSound()
    {
        if (audioSource.clip != null)
        {
            // Awake에서 만든 AudioSource로 소리를 재생합니다.
            audioSource.Play();
        }
    }
}