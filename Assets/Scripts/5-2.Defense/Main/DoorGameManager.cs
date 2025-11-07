using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorGameManager : MonoBehaviour
{
    [Header("문 관련 설정")]
    public List<GameObject> doors; // 🔹 문 4개를 인스펙터에 직접 할당
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    [Header("게임 설정")]
    [Tooltip("문이 빨갛게 유지되는 시간(초)")]
    public float redDuration = 5f; 
    [Tooltip("플레이어와 문 사이 인식 거리(m)")]
    public float reactDistance = 3.0f; 
    [Tooltip("A버튼 연타 요구 횟수")]
    public int requiredPressCount = 5; 
    [Tooltip("첫 단계 문 등장 최소 간격")]
    public float baseMinDelay = 4f;    
    [Tooltip("첫 단계 문 등장 최대 간격")]
    public float baseMaxDelay = 5f;    

    [Header("사운드 설정")]
    public AudioClip knockSound; // 🔊 늑대가 문을 두드리는 소리
    private AudioSource globalAudioSource;

    [Header("씬 설정")]
    public string successSceneName = "ObstacleEnding"; // ✅ 성공 시 이동 씬
    public string failSceneName = "ObstacleFail";      // ❌ 실패 시 이동 씬

    [Header("참조")]
    public Transform player;        // OVRPlayerRig → TrackingSpace → CenterEyeAnchor
    public CountdownTimer timer;    // 손목 타이머
    public ScreenFader screenFader; // 페이드 제어

    private int destroyedDoors = 0;
    private bool gameEnded = false;

    void Start()
    {
        // 🎧 전역 오디오 소스 세팅
        globalAudioSource = gameObject.AddComponent<AudioSource>();
        globalAudioSource.playOnAwake = false;
        globalAudioSource.loop = true;
        globalAudioSource.volume = 0.8f;
        globalAudioSource.spatialBlend = 0f; // 2D (전역 사운드)

        // 🟫 문 원래 색상 저장
        foreach (var door in doors)
        {
            Renderer rend = door.GetComponent<Renderer>();
            if (rend != null)
                originalColors[door] = rend.material.color;
        }

        // ⏱️ 타이머 세팅 (1분 30초)
        timer.startTime = 90f;
        timer.ResetTimer();

        // 🎮 코루틴 시작
        StartCoroutine(DoorRoutine());
    }

    IEnumerator DoorRoutine()
    {
        yield return new WaitForSeconds(2f); // 초기 대기

        while (!gameEnded)
        {
            GameObject target = GetRandomActiveDoor();
            if (target == null)
            {
                EndGame(true); // 모든 문 제거 시 성공
                yield break;
            }

            Renderer rend = target.GetComponent<Renderer>();
            rend.material.color = Color.red; // 🔴 문 빨갛게 표시

            // 🔊 문 빨개질 때 늑대 쾅쾅 사운드 (3D 공간감)
            if (knockSound != null)
            {
                AudioSource doorAudio = target.GetComponent<AudioSource>();
                if (doorAudio == null)
                    doorAudio = target.AddComponent<AudioSource>();

                doorAudio.clip = knockSound;
                doorAudio.loop = true;
                doorAudio.spatialBlend = 1.0f; // ✅ 3D 오디오 (방향 감지 가능)
                doorAudio.volume = 0.9f;
                doorAudio.minDistance = 1f;
                doorAudio.maxDistance = 10f;
                doorAudio.rolloffMode = AudioRolloffMode.Logarithmic;
                doorAudio.Play();
            }

            float elapsed = 0f;
            bool saved = false;
            int pressCount = 0;

            while (elapsed < redDuration)
            {
                elapsed += Time.deltaTime;

                // 플레이어와 문 거리 계산
                float distance = Vector3.Distance(player.position, target.transform.position);

                // ✅ 가까이 있고 A버튼 누르면 카운트
                if (distance <= reactDistance &&
                    OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Active))
                {
                    pressCount++;
                    Debug.Log($"A 버튼 누름: {pressCount}/{requiredPressCount}");

                    // 🎮 진동 피드백
                    OVRInput.SetControllerVibration(0.4f, 0.3f, OVRInput.Controller.RTouch);
                    yield return new WaitForSeconds(0.1f);
                    OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);

                    if (pressCount >= requiredPressCount)
                    {
                        saved = true;
                        break;
                    }
                }

                yield return null;
            }

            // 🔇 사운드 정지
            AudioSource knockAudio = target.GetComponent<AudioSource>();
            if (knockAudio != null && knockAudio.isPlaying)
                knockAudio.Stop();

            // 🔹 색상 복원
            if (originalColors.ContainsKey(target))
                rend.material.color = originalColors[target];

            // 🔹 실패 시 문 파괴
            if (!saved)
            {
                target.SetActive(false);
                destroyedDoors++;
                Debug.Log($"💥 문 파괴됨 ({destroyedDoors}/2)");

                if (destroyedDoors >= 2)
                {
                    EndGame(false); // ❌ 실패 처리
                    yield break;
                }
            }

            // 🕒 다음 문 등장 간격 조정
            float elapsedRatio = GetElapsedRatio(); // 0 ~ 1
            float timeStage = Mathf.Floor(elapsedRatio * 3f); // 0,1,2 (30초 단위)

            float minDelay = baseMinDelay - (timeStage * 0.5f);
            float maxDelay = baseMaxDelay - (timeStage * 0.5f);
            minDelay = Mathf.Max(minDelay, 2.5f);
            maxDelay = Mathf.Max(maxDelay, 3.0f);

            float wait = Random.Range(minDelay, maxDelay);
            Debug.Log($"⏳ 다음 문까지 대기: {wait:F1}초 (단계 {timeStage + 1})");

            yield return new WaitForSeconds(wait);
        }
    }

    // 🔹 타이머 경과 비율 계산 (0~1)
    private float GetElapsedRatio()
    {
        var type = timer.GetType();
        var field = type.GetField("currentTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        float timeLeft = (float)field.GetValue(timer);
        float elapsed = timer.startTime - timeLeft;
        return Mathf.Clamp01(elapsed / timer.startTime);
    }

    // 🔹 활성 문 중 무작위 선택
    GameObject GetRandomActiveDoor()
    {
        List<GameObject> activeDoors = doors.FindAll(d => d.activeSelf);
        if (activeDoors.Count == 0) return null;
        return activeDoors[Random.Range(0, activeDoors.Count)];
    }

    // 🔹 게임 종료 처리 (성공/실패 분리)
    void EndGame(bool success)
    {
        if (gameEnded) return;
        gameEnded = true;

        timer.StopTimer();

        // 🔇 전체 사운드 정지
        if (globalAudioSource.isPlaying)
            globalAudioSource.Stop();

        // 🔹 씬 이동 설정
        if (success)
        {
            Debug.Log("🎉 성공! ObstacleEnding으로 이동");
            screenFader.nextSceneName = successSceneName;
        }
        else
        {
            Debug.Log("💀 실패! ObstacleFail로 이동");
            screenFader.nextSceneName = failSceneName;
        }

        // 🌓 페이드아웃 후 자동 이동
        screenFader.StartFadeOut();
    }

    void Update()
    {
        if (gameEnded) return;

        if (timer != null)
        {
            var type = timer.GetType();
            var field = type.GetField("currentTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            float timeLeft = (float)field.GetValue(timer);

            // ⏰ 타이머 종료 시 결과 판단
            if (timeLeft <= 0)
            {
                if (destroyedDoors < 2)
                    EndGame(true);  // ✅ 성공 (1분 30초 버팀)
                else
                    EndGame(false); // ❌ 실패 (2문 이상 파괴)
            }
        }
    }
}
