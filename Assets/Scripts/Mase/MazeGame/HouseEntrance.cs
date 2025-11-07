using UnityEngine;
using UnityEngine.SceneManagement;

public class HouseEntrance : MonoBehaviour
{
    public AudioClip doorLockedClip;
    public AudioClip doorOpenClip;
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var popup = FindObjectOfType<PopupMessageDisplay>();
            if (GameManager.Instance.breadCount < GameManager.Instance.totalBread)
            {
                Debug.Log("아직 문을 열 수 없습니다. 빵이 부족합니다.");
                popup?.ShowMessage("열쇠가 부족해요!", 2f);
                
                // 🔒 문 잠긴 사운드
                if (doorLockedClip != null)
                    audioSource.PlayOneShot(doorLockedClip);
                
            }
            else
            {
                Debug.Log("문이 열렸습니다. 입장할 수 있습니다!");
                popup?.ShowMessage("다음 장면 넘어가는중..", 2f);
                
                // 🔓 문 열린 사운드
                if (doorOpenClip != null)
                    audioSource.PlayOneShot(doorOpenClip);
                
                // ✅ 2초 후 다음 씬으로 이동
                StartCoroutine(GoToNextSceneAfterDelay(2f));
                
                // 씬 전환, 애니메이션, etc. 추가 가능
            }
        }
    }
    
    private System.Collections.IEnumerator GoToNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 🎯 씬 이름을 "NextScene"으로 가정. 실제 이름으로 바꿔주세요
        SceneManager.LoadScene("NPCInteraction 4");
    }
}
