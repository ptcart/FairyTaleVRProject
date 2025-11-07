using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

/// <summary>
/// ✅ 트리거 진입 시 Flask로 게임 이벤트 정보 요청 → 성공 Story ID 저장 → 다음 씬으로 전환
/// </summary>
public class GoalTriggerPuzzleGame : MonoBehaviour
{
    [Header("🎯 트리거 조건")]
    public string targetTag = "Player";               // 어떤 태그의 오브젝트가 들어오면 발동할지

    [Header("🧩 PuzzleGame 정보")]
    public int gameEventId;                           // 트리거에 연결된 게임 이벤트 ID
    public string nextSceneName = "ObstacleEnding";   // 이동할 씬 이름

    [Header("🎞 연출 옵션")]
    public bool useFadeOut = true;                    // ScreenFader로 페이드아웃 연출할지 여부

    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered || !other.CompareTag(targetTag)) return;

        _triggered = true;
        Debug.Log($"🚩 트리거 진입! 이벤트 ID={gameEventId} → 성공스토리ID 요청 시작");
        StartCoroutine(HandleSuccessStoryAndSceneLoad());
    }

    IEnumerator HandleSuccessStoryAndSceneLoad()
    {
        string url = "http://localhost:5000/command";

        // ✅ JsonUtility는 중첩 객체 직렬화 불가 → 문자열 직접 구성
        string json = "{\"command\":\"puzzlegame_get\",\"payload\":{\"game_event_id\":" + gameEventId + "}}";
        Debug.Log("📤 전송 JSON: " + json);

        // ✅ PostWwwForm 사용 → 실제로는 Raw JSON 전송
        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Flask 요청 실패: " + req.error);
            yield break;
        }

        Debug.Log("✅ Flask 응답 성공: " + req.downloadHandler.text);

        // ✅ JSON 파싱
        PuzzleGameDTO game = JsonUtility.FromJson<PuzzleGameDTO>(req.downloadHandler.text);
        GameDataManager.nextStoryIdToLoad = game.success_story_id;

        Debug.Log("🎯 성공 스토리 ID 저장됨: " + game.success_story_id);

        // ✅ 씬 전환 (페이드 아웃 우선)
        if (useFadeOut)
        {
            var fader = FindObjectOfType<ScreenFader>();
            if (fader != null)
            {
                fader.nextSceneName = nextSceneName;
                fader.StartFadeOut();
                yield break;
            }
        }

        // 바로 씬 전환
        SceneManager.LoadScene(nextSceneName);
    }
}

[System.Serializable]
public class PuzzleGameDTO
{
    public int game_event_id;
    public int success_story_id;
    public int fail_story_id;
    public string game_type;
    public string title;
}
