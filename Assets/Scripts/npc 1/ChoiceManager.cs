using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 🎯 ChoiceManager - NPCInteraction과 연동된 선택지 UI 컨트롤러
/// - 선택지 2개(A/B)와 질문을 표시
/// - 중복 클릭, null NPC, VR 중복 입력 방지
/// </summary>
public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager Instance { get; private set; }

    [Header("UI 요소 (Scene에 위치한 버튼+텍스트 캔버스)")]
    public GameObject canvasChoice;
    public TMP_Text questionText;
    public Button buttonA;
    public Button buttonB;
    public TMP_Text buttonAText;
    public TMP_Text buttonBText;

    [Header("VR 블러 배경 (카메라 자식으로 고정된 캔버스)")]
    public GameObject blurBackground;

    private bool uiVisible = false;
    private bool choiceLocked = false; // ✅ 중복 클릭 방지용

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 초기 비활성화
        if (canvasChoice != null) canvasChoice.SetActive(false);
        if (blurBackground != null) blurBackground.SetActive(false);
    }

    /// <summary>
    /// 🎯 선택지 표시 (질문 + 버튼 텍스트 설정)
    /// </summary>
    public void ShowChoices(List<ChoiceData> choices, string question)
    {
        if (choices == null || choices.Count < 1)
        {
            Debug.LogWarning("⚠️ 선택지가 비어 있습니다.");
            return;
        }

        // ✅ NPC가 비어 있다면 현재 활성 NPC 재등록 시도
        if (NPCInteraction.currentActiveNPC == null)
        {
            Debug.Log("🟡 currentActiveNPC가 null → 가장 최근 NPCInteraction 재등록 시도");
            NPCInteraction foundNPC = FindObjectOfType<NPCInteraction>();
            if (foundNPC != null)
            {
                NPCInteraction.currentActiveNPC = foundNPC;
                Debug.Log($"🟢 currentActiveNPC 재등록 완료: {foundNPC.name}");
            }
        }

        if (choices.Count < 2)
        {
            Debug.LogWarning("⚠️ 선택지가 2개 미만이므로 자동 진행");
            NPCInteraction.currentActiveNPC?.SetNextStoryIdOnly(choices[0].next_story_id);
            return;
        }

        if (string.IsNullOrWhiteSpace(choices[0].content) || string.IsNullOrWhiteSpace(choices[1].content))
        {
            Debug.LogWarning("🚫 선택지 텍스트가 비어 있음 → UI 표시 생략");
            return;
        }

        // ✅ UI 활성화
        if (blurBackground != null) blurBackground.SetActive(true);
        if (canvasChoice != null) canvasChoice.SetActive(true);
        uiVisible = true;

        // ✅ 텍스트 설정
        if (questionText != null) questionText.text = question;
        if (buttonAText != null) buttonAText.text = choices[0].content;
        if (buttonBText != null) buttonBText.text = choices[1].content;

        // ✅ 버튼 리스너 초기화
        buttonA.onClick.RemoveAllListeners();
        buttonB.onClick.RemoveAllListeners();

        buttonA.onClick.AddListener(() => OnChoiceClicked(choices[0].next_story_id, "A"));
        buttonB.onClick.AddListener(() => OnChoiceClicked(choices[1].next_story_id, "B"));

        choiceLocked = false;
        Debug.Log($"🟢 선택지 표시됨 | 질문: {question}, A:{choices[0].content}, B:{choices[1].content}");
    }

    /// <summary>
    /// 🚫 선택지 UI 숨기기
    /// </summary>
    public void HideChoices()
    {
        if (canvasChoice != null) canvasChoice.SetActive(false);
        if (blurBackground != null) blurBackground.SetActive(false);
        uiVisible = false;
    }

    /// <summary>
    /// 선택지 버튼 클릭 시 처리 (중복 방지 + 나레이션 초기화 포함)
    /// </summary>
    private void OnChoiceClicked(int nextStoryId, string buttonLabel)
    {
        if (choiceLocked)
        {
            Debug.Log("🚫 이미 선택 처리됨. 클릭 무시");
            return;
        }

        choiceLocked = true;
        Debug.Log($"🟢 [ChoiceButtonLogger] 버튼 클릭됨: {buttonLabel}");

        HideChoices();

        // 🔹 1️⃣ 모든 NPC 일시 비활성화
        NPCInteraction.DisableAllNPCInteractions();

        // 🔹 나레이션 초기화
        if (NPCInteraction.currentActiveNPC != null &&
            NPCInteraction.currentActiveNPC.storyNarrationText != null)
        {
            NPCInteraction.currentActiveNPC.storyNarrationText.text = "";
        }

        // 🔹 다음 스토리 진행
        if (NPCInteraction.currentActiveNPC != null)
        {
            NPCInteraction.currentActiveNPC.SetNextStoryIdOnly(nextStoryId);
        }

        // 🔹 2️⃣ 약간의 딜레이 후 NPC 다시 활성화
        Instance.StartCoroutine(ReenableNPCInteractions());
    }

    private IEnumerator ReenableNPCInteractions()
    {
        yield return new WaitForSeconds(0.6f); // 선택 이후 잠시 딜레이 (입력 잔상 방지)
        NPCInteraction.EnableAllNPCInteractions();
    }



    /// <summary>
    /// ✅ 선택지 표시 중인지 확인용 (외부에서 UI 상태 체크 가능)
    /// </summary>
    public bool IsVisible()
    {
        return uiVisible;
    }
}
