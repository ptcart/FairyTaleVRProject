using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class VRStoryProgressor : MonoBehaviour
{
    public int currentStoryId = 1;
    private bool isLoading = false;

    public static VRStoryProgressor Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(LoadStoryStep(currentStoryId));
    }

    void Update()
    {
        // ✅ VR 컨트롤러 One 버튼을 눌렀을 때 다음 스토리로 넘어감
        if (!isLoading && OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("🎮 One 버튼 입력 감지됨 → 다음 스토리 진행");
            LoadNextStory();
        }
    }

    public void LoadNextStory()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadStoryStep(currentStoryId));
        }
    }

    IEnumerator LoadStoryStep(int storyId)
    {
        isLoading = true;

        string url = "http://127.0.0.1:5000/command";
        string jsonData = $"{{\"command\":\"get_story_bundle\",\"payload\":{{\"story_id\":{storyId}}}}}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            StoryBundle bundle = JsonUtility.FromJson<StoryBundle>(request.downloadHandler.text);

            Debug.Log($"📘 Story ID: {storyId}");
            Debug.Log($"🧾 NPC Count: {bundle.npcs.Length}, Dialogue Count: {bundle.dialogues.Length}");

            StoryBasedNPCManager.Instance.LoadNPCsForStory(storyId, bundle.npcs);

            DialogueManager dm = FindObjectOfType<DialogueManager>();
            if (dm != null)
            {
                dm.StartDialogue(bundle.dialogues);
            }

            currentStoryId = bundle.nextStoryId;
        }
        else
        {
            Debug.LogError("❌ 서버 요청 실패: " + request.error);
        }

        isLoading = false;
    }
}
