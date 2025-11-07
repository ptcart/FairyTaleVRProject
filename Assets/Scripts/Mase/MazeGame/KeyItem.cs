using UnityEngine;

public class KeyItem : MonoBehaviour
{
    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return; // 이미 먹은 경우 무시

        if (other.CompareTag("Player"))
        {
            isCollected = true; // 플래그 설정

            // 🔇 물리/시각적 충돌 막기
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;

            // 🎵 사운드 재생
            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null && audio.clip != null)
            {
                audio.Play();
            }

            // ✅ 카운트 증가 (한 번만 실행됨)
            GameManager.Instance.CollectBread();

            // ⌛ 사운드 끝난 후 오브젝트 제거
            Destroy(gameObject, audio != null ? audio.clip.length : 0f);
        }
    }
}
