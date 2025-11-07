using System;
using System.Collections;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;




// ---------- 서버 응답 DTO ----------
[Serializable] public class IntroLineDTO {
    public int dialogue_order;
    public string content;
    public string npc_name;
    public string audio_path;
}

[Serializable] public class IntroPayloadDTO {
    public int story_id;
    public IntroLineDTO[] lines;
}

// ---------- (선택) 상태 SO ----------
public class GameState : ScriptableObject {
    public string selectedTaleId = "RedHood";
    public string nextSceneAfterIntro = "NPCInteraction 1";
}

public class IntroNarration : MonoBehaviour
{
    [Header("UI (World Space)")]
    [SerializeField] private TMP_Text subtitle;   // 중앙 자막
    [SerializeField] private TMP_Text hint;       // 하단 힌트(선택)

    [Header("Network (/command)")]
    [SerializeField] private string apiBase = "http://127.0.0.1:5000";
    [SerializeField] private int requestTimeoutSec = 7;
    [SerializeField] private int fairyTaleId = 1; // 빨간모자=1

    [Header("Flow")]
    [SerializeField] private bool requireClickToAdvance = true; // 클릭으로만 다음 진행
    [SerializeField] private float autoAdvanceSeconds = 2.8f;   // 자동 진행 시 한 줄 대기
    [SerializeField] private string nextSceneName = "NPCInteraction 1";

    [Header("Skip (길게)")]
    [SerializeField] private float skipHoldSeconds = 1.1f;

    [Header("Audio")]
    [SerializeField] private AudioSource voice; // 없으면 자동 추가(2D)

    [Header("Local Fallback (서버 실패 시)")]
    [TextArea(2,5)] public string[] localLines = {
        "옛날 옛날에, 숲 근처 작은 마을에 빨간 모자를 쓴 소녀가 살고 있었어요.",
        "소녀는 모두에게 '빨간 모자'라 불렸답니다.",
        "오늘은 엄마의 부탁을 받아, 외할머니께 음식을 전해드리러 떠나야 했지요.",
        "그 길은 숲을 지나야만 하는 위험한 여정이었습니다..."
    };

    // 내부 상태
    private bool waitingForClick = false; // 다음줄 대기
    private bool skipRequested   = false; // 스킵 감지
    private float skipHoldTimer  = 0f;

    private void Awake()
    {
        if (voice == null) voice = gameObject.AddComponent<AudioSource>();
        voice.playOnAwake  = false;
        voice.loop         = false;
        voice.spatialBlend = 0f; // 2D
    }

    private void Start()
    {
        if (hint != null)
            hint.text = "A / 오른손 트리거: 다음  |  B 길게: 스킵";

        StartCoroutine(Run());
    }
    
    // ---------- ‘다음’(트리거/A) 혹은 ‘스킵’(B 길게) 대기 ----------
    private IEnumerator WaitClickOrSkip()
    {
        waitingForClick = true;
        skipRequested   = false;
        skipHoldTimer   = 0f;

        while (waitingForClick)
        {
            // ▶ 다음: 오른손 Index Trigger 또는 A 버튼
            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) ||
                OVRInput.GetDown(OVRInput.RawButton.A))
            {
                waitingForClick = false;
            }

            // ▶ 스킵: B 버튼 길게
            if (OVRInput.Get(OVRInput.RawButton.B))
            {
                skipHoldTimer += Time.deltaTime;
                if (skipHoldTimer >= skipHoldSeconds)
                {
                    skipRequested   = true;
                    waitingForClick = false;
                    break;
                }
            }
            else
            {
                skipHoldTimer = 0f;
            }

            yield return null;
        }
    }



    /// <summary>
    /// Canvas의 풀스크린 버튼 OnClick에 바인딩하세요.
    /// (CustomOVRInputModule이 중앙 응시 + 트리거를 Left Click으로 보냄)
    /// </summary>
    public void NextLine()
    {
        if (waitingForClick) waitingForClick = false;
    }

    private IEnumerator Run()
    {
        // 1) 서버에서 라인 가져오기 (콜백으로 수신)
        IntroLineDTO[] arr = null;
        yield return StartCoroutine(PostIntroGet(fairyTaleId, r => arr = r));

        // 2) 폴백 (서버 실패/빈 응답)
        if (arr == null || arr.Length == 0)
        {
            arr = localLines.Select((s, i) =>
                new IntroLineDTO { dialogue_order = i + 1, content = s }).ToArray();
        }

        // 3) 정렬
        arr = arr.OrderBy(l => l.dialogue_order).ToArray();

        // 4) 줄 진행
        for (int i = 0; i < arr.Length; i++)
        {
            var line = arr[i];
            if (subtitle) subtitle.text = line.content ?? "";

            if (!string.IsNullOrEmpty(line.audio_path))
            {
                yield return StartCoroutine(PlayRemoteAudio(line.audio_path));
                if (requireClickToAdvance) yield return WaitClickOrSkip();
                else yield return new WaitForSeconds(0.2f);
            }
            else
            {
                if (requireClickToAdvance) yield return WaitClickOrSkip();
                else yield return new WaitForSeconds(autoAdvanceSeconds);
            }

            if (skipRequested) break;
        }

        // 5) 다음 씬 이동
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    // ---------- /command : intro_get ----------
    private IEnumerator PostIntroGet(int taleId, Action<IntroLineDTO[]> onDone)
    {
        IntroLineDTO[] result = null;

        if (!string.IsNullOrEmpty(apiBase))
        {
            var url  = $"{apiBase}/command";
            var body = $"{{\"command\":\"intro_get\",\"payload\":{{\"fairy_tale_id\":{taleId}}}}}";
            var bytes = Encoding.UTF8.GetBytes(body);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler   = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = requestTimeoutSec;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var payload = JsonUtility.FromJson<IntroPayloadDTO>(req.downloadHandler.text);
                    if (payload != null && payload.lines != null)
                        result = payload.lines;
                }
                catch { /* 파싱 실패 시 폴백 사용 */ }
            }
        }

        onDone?.Invoke(result);
    }

    // ---------- 원격 오디오 재생 (mp3 → 실패 시 wav 재시도) ----------
    private IEnumerator PlayRemoteAudio(string url)
    {
        using var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            using var reqWav = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            yield return reqWav.SendWebRequest();
            if (reqWav.result != UnityWebRequest.Result.Success)
                yield break;

            var clipWav = DownloadHandlerAudioClip.GetContent(reqWav);
            PlayClip(clipWav);
            // 클릭 진행 모드일 때: 오디오가 끝나거나 사용자가 '다음' 누르면 탈출
            yield return new WaitWhile(() => voice.isPlaying && waitingForClick);
            yield break;
        }

        var clip = DownloadHandlerAudioClip.GetContent(req);
        PlayClip(clip);
        yield return new WaitWhile(() => voice.isPlaying && waitingForClick);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        voice.Stop();
        voice.clip = clip;
        voice.Play();
    }

    
}
