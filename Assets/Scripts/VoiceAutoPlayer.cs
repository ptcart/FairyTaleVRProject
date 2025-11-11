using UnityEngine;
using TMPro;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class VoiceAutoPlayer : MonoBehaviour
{
    public static VoiceAutoPlayer Instance;

    [Header("Audio Settings")]
    public AudioSource narrationSource;   // 🎙 나레이션용 오디오 소스
    public AudioSource dialogueSource;    // 💬 대사용 오디오 소스

    [Header("UI References")]
    public TMP_Text storyNarrationText;   // 나레이션 출력 TMP
    public TMP_Text dialogueText;         // 대사 TMP (자동 탐색)
    public TMP_Text npcNameText;          // NPC 이름 TMP (자동 탐색)

    public string lastStoryText = "";
    public string lastDialogueText = "";

    private static string currentStoryAudioPath = "";
    private static string currentDialogueAudioPath = "";

    private Coroutine narrationCoroutine;
    private Coroutine dialogueCoroutine;

    // 현재 재생 중인 대사 식별용
    private static int currentNpcId = -1;
    private static int currentStoryId = -1;
    private static int currentDialogueOrder = -1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        if (SceneManager.GetActiveScene().name == "NPCInteraction 1")
        {
            currentStoryId = 0;
            Debug.Log("🧹 NPCInteraction 1 씬 → CurrentStoryId 초기화 완료");
        }
    }

    void Start()
    {
        TryFindDialogueText();
        // 🎯 자동으로 나레이션 Text 찾아 연결
        if (storyNarrationText == null)
        {
            var narrationCanvas = GameObject.Find("Canvas_StoryNarration");
            if (narrationCanvas != null)
            {
                storyNarrationText = narrationCanvas.GetComponentInChildren<TMP_Text>(true);
                if (storyNarrationText != null)
                    Debug.Log($"🟢 자동 연결 성공: {storyNarrationText.name}");
                else
                    Debug.LogWarning("⚠️ Canvas_StoryNarration 안에서 TMP_Text를 찾지 못함");
            }
            else
            {
                Debug.LogWarning("⚠️ Canvas_StoryNarration 오브젝트 자체를 찾을 수 없음");
            }
        }
    }

    void Update()
    {
        // 🔹 나레이션 감지
        if (storyNarrationText != null && storyNarrationText.text != lastStoryText)
        {
            if (!string.IsNullOrEmpty(storyNarrationText.text))
            {
                if (narrationCoroutine != null)
                    StopCoroutine(narrationCoroutine);
                narrationCoroutine = StartCoroutine(OnNarrationChanged());
            }
            lastStoryText = storyNarrationText.text;
        }

        // 🔹 자동 TMP 탐색
        if (dialogueText == null)
            TryFindDialogueText();
        
        // 🔹 나레이션 감지
        if (storyNarrationText != null && storyNarrationText.text != lastStoryText)
        {
            if (!string.IsNullOrEmpty(storyNarrationText.text))
            {
                if (narrationCoroutine != null)
                    StopCoroutine(narrationCoroutine);
                narrationCoroutine = StartCoroutine(OnNarrationChanged());
            }
            lastStoryText = storyNarrationText.text;
        }

        // 🔹 자동 TMP 탐색
        if (dialogueText == null)
            TryFindDialogueText();

        // 🧩 나중에 스폰된 Canvas_StoryNarration을 위한 지연 탐색
        if (storyNarrationText == null)
        {
            var narrationCanvas = GameObject.Find("Canvas_StoryNarration");
            if (narrationCanvas != null)
            {
                storyNarrationText = narrationCanvas.GetComponentInChildren<TMPro.TMP_Text>(true);
                if (storyNarrationText != null)
                {
                    Debug.Log($"🟢 [VoiceAutoPlayer] 나중에 생성된 나레이션 TMP 자동 연결됨: {storyNarrationText.name}");
                }
            }
        }
    }

    // =======================================================
    // TMP 자동 탐색
    // =======================================================
    private void TryFindDialogueText()
    {
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
        foreach (var t in allTexts)
        {
            if (t.name.Contains("DialogueText") || t.name.Contains("dialogue"))
                dialogueText = t;
            else if (t.name.Contains("NpcName") || t.name.Contains("npc"))
                npcNameText = t;
        }

        if (dialogueText != null)
            Debug.Log($"🟢 대화 TMP_Text 자동 연결됨: {dialogueText.name}");
    }

    // =======================================================
    // 오디오 경로 등록
    // =======================================================
    public static void RegisterStoryData(string audioPath)
    {
        if (!string.IsNullOrEmpty(audioPath))
        {
            currentStoryAudioPath = audioPath;
            Debug.Log($"🎵 Story audio_path 등록됨: {audioPath}");
        }
    }

    public static void RegisterDialogueData(string audioPath)
    {
        if (string.IsNullOrEmpty(audioPath))
        {
            Debug.LogWarning("⚠️ Dialogue audio_path가 비어 있습니다 — 재생 불가");
            return;
        }

        currentDialogueAudioPath = audioPath;
        Debug.Log($"💬 Dialogue audio_path 등록됨: {audioPath}");

        // ✅ 즉시 재생 트리거
        if (Instance != null)
        {
            if (Instance.dialogueCoroutine != null)
                Instance.StopCoroutine(Instance.dialogueCoroutine);

            Instance.dialogueCoroutine = Instance.StartCoroutine(Instance.OnDialogueChanged());
        }
    }

    // =======================================================
    // 나레이션 재생
    // =======================================================
    private IEnumerator OnNarrationChanged()
    {
        if (string.IsNullOrEmpty(currentStoryAudioPath))
        {
            Debug.LogWarning("⚠️ 나레이션 audio_path가 등록되지 않음 — 재생 건너뜀");
            yield break;
        }

        yield return PlayAudioClip(currentStoryAudioPath, narrationSource, true);
    }

    // =======================================================
    // 대사 재생
    // =======================================================
    private IEnumerator OnDialogueChanged()
    {
        if (string.IsNullOrEmpty(currentDialogueAudioPath))
        {
            Debug.LogWarning("⚠️ 대사 audio_path가 등록되지 않음 — 재생 건너뜀");
            yield break;
        }

        yield return PlayAudioClip(currentDialogueAudioPath, dialogueSource, false);
    }

    // =======================================================
    // 오디오 로드 & 재생
    // =======================================================
    private IEnumerator PlayAudioClip(string resourcePath, AudioSource targetSource, bool isNarration)
    {
        if (string.IsNullOrEmpty(resourcePath))
            yield break;

        string cleanPath = Path.ChangeExtension(resourcePath, null);
        AudioClip clip = Resources.Load<AudioClip>(cleanPath);

        Debug.Log($"🎧 [디버그] {cleanPath} 로드 시도 → {(clip != null ? "✅ 성공" : "❌ 실패")}");

        if (clip == null)
        {
            Debug.LogWarning($"❌ 오디오 파일을 찾을 수 없습니다: {cleanPath}");
            yield break;
        }

        // 🔸 다른 종류의 오디오 즉시 중단
        if (isNarration && dialogueSource != null && dialogueSource.isPlaying)
        {
            dialogueSource.Stop();
        }
        else if (!isNarration && narrationSource != null && narrationSource.isPlaying)
        {
            narrationSource.Stop();
        }

        // 💬 대사 중복 방지 로직
        if (!isNarration)
        {
            int npcId = ExtractNpcId(cleanPath);
            int storyId = ExtractStoryId(cleanPath);
            int dialogueOrder = ExtractOrder(cleanPath);

            // 현재 재생 중인 대사와 완전히 동일하면 재생 스킵
            if (npcId == currentNpcId && storyId == currentStoryId && dialogueOrder == currentDialogueOrder)
            {
                Debug.Log("🔸 동일한 대사 이미 재생 중 — 건너뜀");
                yield break;
            }

            // 다른 order이거나 다른 NPC면 중단 후 새로 재생
            if (targetSource.isPlaying)
            {
                Debug.Log($"⏹ 이전 대사 중단 → {targetSource.clip?.name}");
                targetSource.Stop();
            }

            currentNpcId = npcId;
            currentStoryId = storyId;
            currentDialogueOrder = dialogueOrder;
        }

        // 🔊 새 오디오 재생
        targetSource.clip = clip;
        targetSource.loop = false;
        targetSource.Play();

        Debug.Log($"▶️ {(isNarration ? "나레이션" : "대사")} 재생 시작: {cleanPath}");
        yield return null;
    }

    // =======================================================
    // 파일명 파싱 유틸
    // =======================================================
    private int ExtractNpcId(string path)
    {
        if (string.IsNullOrEmpty(path)) return -1;
        string[] parts = path.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int id)) return id;
        return -1;
    }

    private int ExtractStoryId(string path)
    {
        if (string.IsNullOrEmpty(path)) return -1;
        string[] parts = path.Split('_');
        if (parts.Length >= 3 && int.TryParse(parts[2], out int id)) return id;
        return -1;
    }

    private int ExtractOrder(string path)
    {
        if (string.IsNullOrEmpty(path)) return -1;
        string[] parts = path.Split('_');
        if (parts.Length >= 4 && int.TryParse(parts[3], out int id)) return id;
        return -1;
    }
    
    public void ForcePlayNarration()
    {
        if (string.IsNullOrEmpty(currentStoryAudioPath))
        {
            Debug.LogWarning("⚠️ [VoiceAutoPlayer] 나레이션 경로가 비어 있음 — 재생 불가");
            return;
        }

        if (narrationCoroutine != null)
            StopCoroutine(narrationCoroutine);

        Debug.Log($"🎬 [VoiceAutoPlayer] 강제 나레이션 재생 시작: {currentStoryAudioPath}");
        narrationCoroutine = StartCoroutine(OnNarrationChanged());
    }
}
