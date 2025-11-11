// NPCInteraction.cs - 스토리 기반 대사 흐름을 위한 리팩터링 버전
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NPCInteraction : MonoBehaviour
{
    public int npcId;
    public int storyId;
    public int disappearStoryId;
    public int nextStoryId = -1;
    public static int CurrentStoryId { get; private set; } // ✅ 추가

    public float interactionDistance = 3f;
    public GameObject exclamationMark;
    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public TMP_Text storyNarrationText;
    public TMP_Text npcNameText;

    private string[] dialogueLines;
    private int currentDialogueIndex = 0;

    private int currentChoiceMode = 0; // 0: 일반 대화 / 1: 선택지 / 2: 게임

    private bool isTalking = false;
    private bool canAdvanceDialogue = false;
    private bool waitingForDialogueInput = false;
    private bool isNarrationMode = false;
    private bool hasStartedStory = false;

    public static bool isAnyDialogueActive = false;
    public static bool isexclamationMark = true;
    public static NPCInteraction currentActiveNPC = null;
    
    private string[] dialogueAudioPaths;  // 클래스 상단에 추가
    
    public bool inputLocked = false; // ✅ 선택지 클릭 후 잠시 입력 막기용


    private bool isReadyForSceneChange = false;
    private bool isSceneChangeScheduled = false;
    private int targetBackgroundId = -1;

    private string nextSceneName = "NPCInteraction 2";
    private bool fromChoice = false; // ✅ 선택지에서 넘어왔는지 여부
    private int currentActiveStoryId = -1;
    
    private bool hasHandledStoryFlow = false; // ✅ 같은 storyId 중복 방지용    
    public UnityEngine.UI.Image narrationImage; // 🎨 인스펙터에서 직접 연결

    void Start()
    {
        // 시작할 때 동기화
        CurrentStoryId = storyId;
        isAnyDialogueActive = false;
        
        
        currentActiveNPC = null;

        if (GameDataManager.nextStoryIdToLoad > 0)
        {
            storyId = GameDataManager.nextStoryIdToLoad;
        }

        hasStartedStory = false;
        isTalking = false;
        isNarrationMode = false;

        if (narrationImage != null)
        {
            narrationImage.enabled = false; // ✅ 처음에는 숨김
        }
        
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    void Update()
    {

        if (ChoiceManager.Instance != null && ChoiceManager.Instance.IsVisible())
            return; // 선택지 UI가 떠 있으면 대화/나레이션 입력 무시
        
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        if (distance < interactionDistance)
        {
            if (exclamationMark != null && isexclamationMark)
                exclamationMark.SetActive(true);

            if (isAnyDialogueActive && currentActiveNPC != this)
                return;

            if (OVRInput.GetUp(OVRInput.Button.One))
            {
                
                // 🧱 추가: 입력 잠금 중이면 클릭 무시
                if (inputLocked)
                {
                    Debug.Log("⏸️ 입력 잠금 상태 → VR 입력 무시됨");
                    return;
                }

                
                if (CompareTag("Wolf")) return;  // 🔹 늑대는 아예 무시
                if (isSceneChangeScheduled && isReadyForSceneChange)
                {
                    string sceneToLoad = "NPCInteraction " + targetBackgroundId;
                    StopAllCoroutines();
                    SceneManager.LoadScene(sceneToLoad);
                    return;
                }

                if (isNarrationMode)
                {
                    Debug.Log($"🟨 [isNarrationMode] storyId={storyId}, fromChoice={fromChoice}, hasStartedStory={hasStartedStory}, CurrentStoryId={CurrentStoryId}, currentChoiceMode={currentChoiceMode}");

                    // ✅ 선택지에서 막 넘어온 상태라면 1프레임 무시
                    if (fromChoice)
                    {
                        fromChoice = false;
                        return;
                    }
                    
                    // ✅ 이미 HandleNextStoryFlow가 한 번 실행됐다면 중복 방지
                    if (hasHandledStoryFlow)
                        return;
                    hasHandledStoryFlow = true;
                    
                    if (narrationImage != null)
                    {
                        narrationImage.enabled = false; // ✅ 나레이션 시작 시 이미지 표시
                    }

                    
                    // 일반적인 나레이션 진행
                    storyNarrationText.text = "";
                    isNarrationMode = false;
                    isReadyForSceneChange = true;

                    HandleNextStoryFlow();  // ✅ 이제 이 호출은 일반 스토리(대사 있는 경우)에만 실행됨
                }
                
                else if (isTalking && canAdvanceDialogue && !waitingForDialogueInput)
                {
                    NextDialogue();
                }
                else if (!isTalking && !isNarrationMode && !hasStartedStory)
                {
                    if (storyId == 301)
                    {
                        if (!StoryItemManager.Instance.AllItemsCollected())
                        {
                            Debug.Log("❌ 아직 재료가 다 모이지 않았습니다.");

                            // 🔹 항상 퀘스트 안내 메시지를 출력
                            if (dialogueUI != null && dialogueText != null)
                            {
                                dialogueUI.SetActive(true);
                                dialogueText.text = "양동이, 버터, 후추를 찾아오자!(A버튼으로 수집가능)";
                                StartCoroutine(HideDialogueAfterDelay(2f));
                            }

                            return; // ❌ 재료 부족 → 진행 중단
                        }
                        else
                        {
                            Debug.Log("🟢 모든 재료 수집 완료 → 302번 스토리 시작");
                            storyId = nextStoryId; // (301 → 302)
                        }
                    }


                    // ✅ 여기서는 301→302든 일반 스토리든 "정상 진행" 공통 실행
                    hasStartedStory = true;
                    isAnyDialogueActive = true;
                    currentActiveNPC = this;
                    isexclamationMark = false;

                    if (exclamationMark != null)
                        exclamationMark.SetActive(false);

                    StartCoroutine(LoadStoryNarrationAndDecide(storyId));
                }


            }
        }
        else
        {
            if (exclamationMark != null)
                exclamationMark.SetActive(false);
        }
        
        // ✅ [여기 아래에 이 줄 추가]
        // 🎯 빨간모자 전용 Ray 감지 코드 시작
        if (CompareTag("RedHood"))
        {
            Transform rayOrigin = Camera.main.transform;
            Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5f))
            {
                // Ray가 자기 자신(빨간모자 Collider)에 닿은 경우만
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    if (OVRInput.GetDown(OVRInput.Button.One))
                    {
                        Debug.Log($"🧒 [RedHood 감지] Ray Hit + One 버튼 → 나레이션 시작");

                        if (isTalking || isNarrationMode || hasStartedStory)
                            return;

                        hasStartedStory = true;
                        isAnyDialogueActive = true;
                        currentActiveNPC = this;
                        isexclamationMark = false;

                        if (exclamationMark != null)
                            exclamationMark.SetActive(false);

                        StartCoroutine(LoadStoryNarrationAndDecide(storyId));
                    }
                }
            }
        }
        // 🎯 빨간모자 전용 Ray 감지 코드 끝
    }

    private IEnumerator HideDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }
    

    IEnumerator LoadStoryNarrationAndDecide(int storyId)
    {
        
        
        Debug.Log($"🟢 [LoadStoryNarrationAndDecide 시작] storyId={storyId}, fromChoice={fromChoice}");

        
        hasHandledStoryFlow = false; // ✅ 새로운 스토리 진입 시 초기화
        this.storyId = storyId;
    
        string url = "http://127.0.0.1:5000/command";
        string jsonData = "{\"command\":\"story_get\",\"payload\":{\"story_id\":" + storyId + "}}";
    
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
    
        yield return request.SendWebRequest();
    
        if (request.result == UnityWebRequest.Result.Success)
        {
            string rawJson = request.downloadHandler.text;
    
            if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.Trim().StartsWith("{"))
                yield break;
    

            StoryData data = JsonUtility.FromJson<StoryData>(rawJson);
            

            nextStoryId = data.next_story_id;
            currentChoiceMode = data.has_choice_or_game;
            CurrentStoryId = storyId;
            
            // ✅ Flask 응답 JSON에서 audio_path를 읽어 VoiceAutoPlayer에 등록
            if (!string.IsNullOrEmpty(data.audio_path))
                VoiceAutoPlayer.RegisterStoryData(data.audio_path);
    
            // ✅ 엔딩 분기 추가 (엔딩ID 기준)
            if (data.is_ending)
            {
                Debug.Log($"🎬 엔딩 감지됨 → Ending{data.ending_id} 씬으로 페이드 이동");
    
                // 🔹 스토리의 ending_id를 기반으로 씬 이름 설정
                string endingSceneName = "Ending " + data.ending_id;
    
                // 🔹 현재 씬에서 ScreenFader 찾기
                ScreenFader fader = FindObjectOfType<ScreenFader>();
    
                if (fader != null)
                {
                    // 🔸 ScreenFader에 다음 씬 이름 설정
                    fader.nextSceneName = endingSceneName;
    
                    // 🔸 페이드 아웃 시작 (ScreenFader 내부에서 LoadScene 호출함)
                    fader.StartFadeOut();
                }
                else
                {
                    // 🔸 ScreenFader가 없으면 그냥 바로 씬 이동
                    SceneManager.LoadScene(endingSceneName);
                }
    
                yield break;
            }
    
    
    
            if (!string.IsNullOrEmpty(data.content))
            {
                isNarrationMode = true;
                if (storyNarrationText != null)
                    storyNarrationText.text = data.content;
                
                if (narrationImage != null)
                {
                    narrationImage.enabled = true; // ✅ 나레이션 시작 시 이미지 표시
                }

                
                // ✅ 선택지 직후에는 자동으로 다음 입력 받을 준비
                if (fromChoice)
                {
                    Debug.Log("🟢 선택지 직후 → 나레이션 즉시 활성화");
                    fromChoice = false;
                    canAdvanceDialogue = true;
                }
            }
            else
            {
                // ✅ 내용이 없더라도 선택지 직후면 즉시 다음 흐름 처리
                if (fromChoice)
                {
                    Debug.Log("🟢 선택지 직후 → 내용 없음 → 즉시 HandleNextStoryFlow 실행");
                    fromChoice = false;
                    HandleNextStoryFlow();
                    yield break;
                }
                HandleNextStoryFlow();
            }
            
            // 🧩 [추가] VoiceAutoPlayer에 나레이션 강제 재생 트리거
            if (!string.IsNullOrEmpty(data.audio_path))
            {
                VoiceAutoPlayer.RegisterStoryData(data.audio_path);
                if (VoiceAutoPlayer.Instance != null)
                {
                    VoiceAutoPlayer.Instance.ForcePlayNarration(); // 👈 강제 재생 메서드 호출
                }
            }

    
            if (data.should_change_scene)
            {
                GameDataManager.nextStoryIdToLoad = nextStoryId;
                isSceneChangeScheduled = true;
                targetBackgroundId = data.background_id;
                yield break;
            }
        }
    }
    


    // ✅ 모든 NPC를 일시적으로 비활성화
    public static void DisableAllNPCInteractions()
    {
        foreach (var npc in FindObjectsOfType<NPCInteraction>())
        {
            npc.enabled = false;
            if (npc.exclamationMark != null)
                npc.exclamationMark.SetActive(false);
        }
        Debug.Log("🚫 모든 NPC 상호작용 비활성화됨");
    }

 // ✅ 모든 NPC를 다시 활성화
    public static void EnableAllNPCInteractions()
    {
        foreach (var npc in FindObjectsOfType<NPCInteraction>())
        {
            npc.enabled = true;
        }
        Debug.Log("🟢 모든 NPC 상호작용 복구됨");
    }

    // IEnumerator LoadStoryNarrationAndDecide(int storyId)
    // {
    //     hasHandledStoryFlow = false; // ✅ 새로운 스토리 진입 시 초기화
    //     this.storyId = storyId;
    //
    //     string url = "http://127.0.0.1:5000/command";
    //     string jsonData = "{\"command\":\"story_get\",\"payload\":{\"story_id\":" + storyId + "}}";
    //
    //     UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonData);
    //     byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
    //     request.uploadHandler = new UploadHandlerRaw(jsonToSend);
    //     request.downloadHandler = new DownloadHandlerBuffer();
    //     request.SetRequestHeader("Content-Type", "application/json");
    //
    //     yield return request.SendWebRequest();
    //
    //     if (request.result == UnityWebRequest.Result.Success)
    //     {
    //         string rawJson = request.downloadHandler.text;
    //
    //         if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.Trim().StartsWith("{"))
    //             yield break;
    //
    //         StoryData data = JsonUtility.FromJson<StoryData>(rawJson);
    //
    //         nextStoryId = data.next_story_id;
    //         currentChoiceMode = data.has_choice_or_game;
    //         CurrentStoryId = storyId;
    //
    //         // ✅ 엔딩 분기 추가 (엔딩ID 기준)
    //         if (data.is_ending)
    //         {
    //             Debug.Log($"🎬 엔딩 감지됨 → Ending{data.ending_id} 씬으로 페이드 이동");
    //
    //             // 🔹 스토리의 ending_id를 기반으로 씬 이름 설정
    //             string endingSceneName = "Ending " + data.ending_id;
    //
    //             // 🔹 현재 씬에서 ScreenFader 찾기
    //             ScreenFader fader = FindObjectOfType<ScreenFader>();
    //
    //             if (fader != null)
    //             {
    //                 // 🔸 ScreenFader에 다음 씬 이름 설정
    //                 fader.nextSceneName = endingSceneName;
    //
    //                 // 🔸 페이드 아웃 시작 (ScreenFader 내부에서 LoadScene 호출함)
    //                 fader.StartFadeOut();
    //             }
    //             else
    //             {
    //                 // 🔸 ScreenFader가 없으면 그냥 바로 씬 이동
    //                 SceneManager.LoadScene(endingSceneName);
    //             }
    //
    //             yield break;
    //         }
    //
    //
    //
    //         if (!string.IsNullOrEmpty(data.content))
    //         {
    //             isNarrationMode = true;
    //             if (storyNarrationText != null)
    //                 storyNarrationText.text = data.content;
    //         }
    //         else
    //         {
    //             HandleNextStoryFlow();
    //         }
    //
    //         if (data.should_change_scene)
    //         {
    //             GameDataManager.nextStoryIdToLoad = nextStoryId;
    //             isSceneChangeScheduled = true;
    //             targetBackgroundId = data.background_id;
    //             yield break;
    //         }
    //     }
    // }


    IEnumerator LoadDialogueForStory(int storyId)
    {
        string url = "http://127.0.0.1:5000/command";
        string jsonData = "{\"command\":\"dialogue_list\",\"payload\":{\"story_id\":" + storyId + "}}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            DialogueListWrapper data = JsonUtility.FromJson<DialogueListWrapper>(request.downloadHandler.text);

            if (data.dialogue == null || data.dialogue.Length == 0)
            {
                if (nextStoryId > 0)
                {
                    this.storyId = nextStoryId;
                    hasStartedStory = false;
                    yield return new WaitForSeconds(0.5f);
                    StartCoroutine(LoadStoryNarrationAndDecide(this.storyId));
                }
                else
                {
                    SetNextStory(0);
                }
                yield break;
            }
            dialogueLines = new string[data.dialogue.Length];
            dialogueAudioPaths = new string[data.dialogue.Length]; // ✅ 오디오 경로 배열 추가

            for (int i = 0; i < data.dialogue.Length; i++)
            {
                dialogueLines[i] = data.dialogue[i].npc_name + ": " + data.dialogue[i].content;

                // ✅ 파일명 규칙: Voices/NPC/dialogue_{npc_id}_{story_id}_{dialogue_order}
                string audioPath = $"Voices/NPC/dialogue_{data.dialogue[i].npc_id}_{data.dialogue[i].story_id}_{data.dialogue[i].dialogue_order}";
    
                // ✅ audio_path가 DB에 있으면 그것을 우선 사용
                if (!string.IsNullOrEmpty(data.dialogue[i].audio_path))
                    audioPath = data.dialogue[i].audio_path;

                // ✅ 등록은 나중에 하기 위해 경로만 저장
                dialogueAudioPaths[i] = audioPath;

                Debug.Log($"🧩 dialogue[{i}] 저장 완료 → npc_id={data.dialogue[i].npc_id}, story_id={data.dialogue[i].story_id}, path={audioPath}");
            }
            

            // dialogueLines = new string[data.dialogue.Length];
            // for (int i = 0; i < data.dialogue.Length; i++)
            // {
            //
            //     dialogueLines[i] = data.dialogue[i].npc_name + ": " + data.dialogue[i].content;
            //
            //     // ✅ 파일명 규칙: Voices/NPC/dialogue_{npc_id}_{story_id}_{dialogue_order}
            //     string audioPath = $"Voices/NPC/dialogue_{data.dialogue[i].npc_id}_{data.dialogue[i].story_id}_{data.dialogue[i].dialogue_order}";
            //     
            //     Debug.Log($"🧩 dialogue[{i}] -> npc_id={data.dialogue[i].npc_id}, story_id={data.dialogue[i].story_id}, audio_path={data.dialogue[i].audio_path}");
            //     
            //     // ✅ audio_path가 DB에 있으면 그것을 우선 사용
            //     if (!string.IsNullOrEmpty(data.dialogue[i].audio_path))
            //         audioPath = data.dialogue[i].audio_path;
            //     
            //     // ✅ VoiceAutoPlayer에 등록
            //     VoiceAutoPlayer.RegisterDialogueData(audioPath);
            // }

            StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (narrationImage != null)
        {
            narrationImage.enabled = false; // ✅ 나레이션 시작 시 이미지 표시
        }


        
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            if (nextStoryId > 0)
            {
                this.storyId = nextStoryId;
                hasStartedStory = false;
                StartCoroutine(LoadStoryNarrationAndDecide(this.storyId));
            }
            else
            {
                SetNextStory(0);
            }
            return;
        }

        isTalking = true;
        currentDialogueIndex = 0;
        dialogueUI.SetActive(true);

        ShowDialogueLine();

        waitingForDialogueInput = true;
        canAdvanceDialogue = false;
        StartCoroutine(EnableDialogueAdvanceAfterDelay());
    }
    void NextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < dialogueLines.Length)
        {
            ShowDialogueLine();

            if (fromChoice) // ✅ 선택지에서 막 넘어온 경우
            {
                waitingForDialogueInput = false;
                canAdvanceDialogue = true;
                fromChoice = false; // 한 번만 적용
            }
            else
            {
                waitingForDialogueInput = true;
                canAdvanceDialogue = false;
                StartCoroutine(EnableDialogueAdvanceAfterDelay());
            }
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator EnableDialogueAdvanceAfterDelay()
    {
        yield return null; // 다음 프레임까지만 대기
        canAdvanceDialogue = true;
        waitingForDialogueInput = false;
    }

    void ShowDialogueLine()
    {
        string line = dialogueLines[currentDialogueIndex];

        if (line.Contains(":"))
        {
            string[] splitLine = line.Split(new char[] { ':' }, 2);
            if (npcNameText != null)
                npcNameText.text = splitLine[0].Trim();
            if (dialogueText != null)
                dialogueText.text = splitLine[1].Trim();
        }
        else
        {
            if (npcNameText != null)
                npcNameText.text = "";
            if (dialogueText != null)
                dialogueText.text = line;
        }
        //--------------------------------------------------
        // ✅ 여기 추가: 현재 대사에 해당하는 오디오 경로 등록
        // ✅ 여기서 현재 대사 오디오만 재생 등록
        if (dialogueAudioPaths != null && currentDialogueIndex < dialogueAudioPaths.Length)
        {
            string currentAudioPath = dialogueAudioPaths[currentDialogueIndex];
            Debug.Log($"🎤 [VoiceAutoPlayer 호출] 현재 대사 오디오 등록: {currentAudioPath}");
            VoiceAutoPlayer.RegisterDialogueData(currentAudioPath);
        }
    }

    void EndDialogue()
    {
        dialogueUI.SetActive(false);
        isTalking = false;
        currentDialogueIndex = 0;

        isAnyDialogueActive = false;
        currentActiveNPC = null;

        if (nextStoryId > 0)
        {
            // ✅ 300 → 301로 넘어갈 때 특수 처리
            if (storyId == 300 && nextStoryId == 301)
            {
                Debug.Log("🔔 300 대사 끝 → 301(퀘스트 시작)으로 전환!");

                storyId = 301;
                GameDataManager.nextStoryIdToLoad = 301;
                CurrentStoryId = 301;

                // 퀘스트 안내 메시지 표시
                if (dialogueUI != null && dialogueText != null)
                {
                    //dialogueUI.SetActive(true);
                    //DialogueUI.Instance?.ShowTemporaryMessage("", 2f);
                    dialogueText.text = "양동이, 버터, 후추를 찾아오자(A버튼으로 수집가능)";
                    //StartCoroutine(HideDialogueAfterDelay(2f));
                }

                return; // ❌ 301은 퀘스트 모드 → 자동 진행 중단
            }

            // ✅ 301 → 302로 넘어갈 때는 자동으로 나레이션 실행
            if (storyId == 301 && nextStoryId == 302)
            {
                Debug.Log("🟢 301(퀘스트 완료) 끝 → 302 나레이션 자동 실행 (NPC는 유지)");

                storyId = 302;
                GameDataManager.nextStoryIdToLoad = 302;
                CurrentStoryId = 302;

                hasStartedStory = false;     // 자동 실행되도록 초기화
                isAnyDialogueActive = false; // NPC 충돌 방지
                isNarrationMode = false;

                // 바로 302 나레이션 실행
                StartCoroutine(LoadStoryNarrationAndDecide(302));
                return;
            }
            

            // ✅ 그 외 일반적인 흐름
            storyId = nextStoryId;
            hasStartedStory = false;

            var spawnerGeneral = FindObjectOfType<NPCSpawner>();
            if (spawnerGeneral != null)
                spawnerGeneral.SpawnNPCsForStory(storyId);

            StartCoroutine(LoadStoryNarrationAndDecide(storyId));
        }
    }

    private IEnumerator AutoPlayNextStory(int storyId)
    {
        // NPC가 스폰될 시간을 조금 줌
        yield return new WaitForSeconds(0.2f);

        var spawner = FindObjectOfType<NPCSpawner>();
        if (spawner != null)
            spawner.SpawnNPCsForStory(storyId);

        Debug.Log($"▶ 자동 실행: {storyId}번 나레이션 시작");
        StartCoroutine(LoadStoryNarrationAndDecide(storyId));
    }
    IEnumerator LoadChoicesForStory(int storyId)
    {
        string url = "http://127.0.0.1:5000/command";
        string jsonData = "{\"command\":\"choice_list\",\"payload\":{\"story_id\":" + storyId + "}}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string fixedJson = "{\"choices\":" + request.downloadHandler.text + "}";
            ChoiceListWrapper wrapper = JsonUtility.FromJson<ChoiceListWrapper>(fixedJson);

            if (wrapper != null && wrapper.choices != null && wrapper.choices.Length > 0)
            {
                List<ChoiceData> choices = new List<ChoiceData>(wrapper.choices);

                // ✅ 첫 번째 선택지의 question_id 사용
                int questionId = choices[0].question_id;
                yield return StartCoroutine(LoadChoiceQuestion(questionId, choices));
            }
            else
            {
                Debug.LogWarning("⚠️ 선택지가 없습니다. 다음 스토리로 자동 이동");
                StartCoroutine(LoadStoryNarrationAndDecide(nextStoryId));
            }
        }
        else
        {
            Debug.LogError("❌ 선택지 불러오기 실패: " + request.error);
        }
    }
    
    IEnumerator LoadChoiceQuestion(int questionId, List<ChoiceData> choices)
    {
        string url = "http://127.0.0.1:5000/command";
        string jsonData = "{\"command\":\"choicequestion_get\",\"payload\":{\"question_id\":" + questionId + "}}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ChoiceQuestionData qData = JsonUtility.FromJson<ChoiceQuestionData>(request.downloadHandler.text);

            Debug.Log("📩 ChoiceManager 호출: 질문=" + qData.content);

            // ✅ DB에서 가져온 질문 넣기
            ChoiceManager.Instance.ShowChoices(choices, qData.content);
        }
        else
        {
            Debug.LogError("❌ 질문 가져오기 실패: " + request.error);
            // fallback
            ChoiceManager.Instance.ShowChoices(choices, "선택지를 고르세요.");
        }
    }

    public void SetNextStory(int nextId)
    {
        // ✅ 같은 스토리 ID로 중복 호출 방지
        if (nextId == storyId)
        {
            Debug.LogWarning("⚠️ 같은 스토리 ID로 중복 호출 방지됨");
            return;
        }

        // ✅ 종료 시점: 0이면 대화 종료 및 NPC 제거
        if (nextId == 0)
        {
            if (dialogueUI != null)
                dialogueUI.SetActive(false);

            var spawner = FindObjectOfType<NPCSpawner>();
            if (spawner != null)
                spawner.RemoveAllNPCs();

            ChoiceManager.Instance?.HideChoices();
            return;
        }

        // ✅ 🔥 모든 기존 코루틴 종료 (중복 진행 방지)
        StopAllCoroutines();

        // ✅ 선택지 이후 이전 스토리의 흐름 정보 완전 초기화
        // (이게 핵심: 이전 스토리의 nextStoryId, 씬 예약 정보가 남아있으면 꼬임)
        isSceneChangeScheduled = false;   // 🔥 이전 스토리에서 예약된 씬 이동 무효화
        isReadyForSceneChange = false;    // 🔥 씬 변경 가능 상태 초기화
        targetBackgroundId = -1;          // 🔥 타겟 배경 ID 초기화
        nextStoryId = -1;                 // 🔥 이전 스토리의 nextStoryId 영향 제거
        currentActiveStoryId = -1;        // 🔥 HandleNextStoryFlow 중복 방지용 ID 초기화

        // ✅ 입력 락 (선택지 클릭 후 잠깐 VR 입력 차단)
        inputLocked = true;
        StartCoroutine(ReleaseInputLockAfterDelay(0.5f));

        // ✅ 기본 상태 초기화
        isNarrationMode = false;
        isTalking = false;
        hasStartedStory = false;
        hasHandledStoryFlow = false;
        isAnyDialogueActive = false;

        fromChoice = true; // ✅ 선택지에서 넘어왔음을 명시

        Debug.Log($"➡️ [SetNextStory] {storyId} → {nextId} 전환 (이전 상태 완전 초기화)");

        // ✅ 실제 스토리 전환
        storyId = nextId;
        StartCoroutine(LoadStoryNarrationAndDecide(storyId));
    }

// ✅ ChoiceManager에서 nextStoryId만 전달할 때 사용
    public void SetNextStoryIdOnly(int nextId)
    {
        Debug.Log($"🟣 [SetNextStoryIdOnly 호출됨] nextId={nextId}");

        //Debug.Log($"📘 선택 결과 전달됨: 다음 스토리 {nextId}");
        // 🔹 깜빡임 최소화를 위한 나레이션 즉시 클리어
        if (storyNarrationText != null)
        {
            storyNarrationText.text = "";
            Canvas.ForceUpdateCanvases(); // 🔥 UI를 즉시 갱신 (다음 프레임까지 기다리지 않음)
        }

        // 🔹 혹시 남아 있는 코루틴이 있다면 중복 방지 위해 정리
        StopAllCoroutines();

        // 🔹 실제 다음 스토리 실행을 약간 늦춰서 VR 입력이 완전히 끝나도록 함
        StartCoroutine(ProceedAfterChoice(nextId));
    }

    private IEnumerator ProceedAfterChoice(int nextId)
    {
        // 🔹 VR 입력 잔상 방지용 (0.2~0.3초 딜레이)
        yield return new WaitForSeconds(0.2f);

        fromChoice = true;
        isNarrationMode = false;
        hasStartedStory = false;
        isAnyDialogueActive = false;
        hasHandledStoryFlow = false;

        Debug.Log($"🟢 선택지 이후 스토리 {nextId}로 이동 시작");
        StartCoroutine(LoadStoryNarrationAndDecide(nextId));
    }


    // public void SetNextStory(int nextId)
    // {
    //     if (nextId == storyId)
    //     {
    //         Debug.LogWarning("⚠️ 같은 스토리 ID로 중복 호출 방지됨");
    //         return; // 같은 ID일 경우 다시 로딩하지 않음
    //     }
    //
    //     if (nextId == 0)
    //     {
    //         if (dialogueUI != null)
    //             dialogueUI.SetActive(false);
    //
    //         var spawner = FindObjectOfType<NPCSpawner>();
    //         if (spawner != null)
    //             spawner.RemoveAllNPCs();
    //
    //         ChoiceManager.Instance?.HideChoices();
    //         return;
    //     }
    //
    //     // ✅ 조건 충족 or 일반적인 흐름 → 진행
    //     isNarrationMode = false;
    //     isTalking = false;
    //
    //     storyId = nextId;
    //     hasStartedStory = false;
    //     isAnyDialogueActive = false;
    //
    //     fromChoice = true; // ✅ 선택지에서 넘어옴 표시
    //     hasHandledStoryFlow = false;  // ✅ 선택지 후 다음 스토리 진입 전 상태 초기화
    //
    //     StartCoroutine(LoadStoryNarrationAndDecide(storyId));
    //
    //     inputLocked = true;  
    //     StartCoroutine(ReleaseInputLockAfterDelay(0.3f));
    // }

    
    public IEnumerator ReleaseInputLockAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputLocked = false;
    }
    

    void HandleNextStoryFlow()
    {
        Debug.Log($"🟢 [LoadStoryNarrationAndDecide 시작] storyId={storyId}, fromChoice={fromChoice}");

    // // ✅ 중복 호출 방지
    //     if (hasHandledStoryFlow)
    //     {
    //         Debug.Log($"🚫 HandleNextStoryFlow 중복 호출 차단됨 (storyId={storyId})");
    //         return;
    //     }
    //     hasHandledStoryFlow = true; // 🔒 한 번만 실행
    //
    //     Debug.Log($"➡️ HandleNextStoryFlow 호출됨: storyId={storyId}, currentChoiceMode={currentChoiceMode}, fromChoice={fromChoice}");    
    //
    //     Debug.Log($"➡️ HandleNextStoryFlow 호출됨: storyId={storyId}, currentChoiceMode={currentChoiceMode}, fromChoice={fromChoice}");
    //     
        if (currentChoiceMode == 0)
        {
            if (isSceneChangeScheduled)
            {
                string sceneToLoad = "NPCInteraction " + targetBackgroundId;
                SceneManager.LoadScene(sceneToLoad);
                return;
            }
            // ✨ 중복 스토리 호출 방지
            if (storyId == currentActiveStoryId)
            {
                Debug.Log("⚠️ 중복된 스토리 ID입니다. HandleNextStoryFlow 스킵");
                return;
            }
            currentActiveStoryId = storyId;
            StartCoroutine(LoadDialogueForStory(storyId));
        }
        else if (currentChoiceMode == 1)
        {
            StartCoroutine(LoadChoicesForStory(storyId));
        }
        else if (currentChoiceMode == 2)
        {
            GameDataManager.nextStoryIdToLoad = nextStoryId;
    
            string puzzleSceneName = storyId switch
            {
                101  => "ObstacleIntro",
                13 => "MazeIntro",
                22 => "DefenseIntro",
                //_  => "SlidePuzzleIntro"
            };
    
            SceneManager.LoadScene(puzzleSceneName);
        }
    }

    [System.Serializable]
    public class StoryData
    {
        public string content;
        public int next_story_id;
        public int has_choice_or_game;
        public bool is_ending;
        public bool should_change_scene;
        public int background_id;
        public int ending_id;
        public string audio_path; 
    }

    [System.Serializable]
    public class DialogueData
    {
        public int story_id;          // 🔹 추가
        public int npc_id;            // 🔹 추가
        public string npc_name;
        public string content;
        public string audio_path;     // 🔹 추가
        public int dialogue_order;
    }


    [System.Serializable]
    public class DialogueListWrapper
    {
        public DialogueData[] dialogue;
    }

    [System.Serializable]
    public class ChoiceListWrapper
    {
        public ChoiceData[] choices;
    }

    [System.Serializable]
    public class ChoiceQuestionData
    {
        public int question_id;
        public string content;  // 질문 내용
    }

    


}
