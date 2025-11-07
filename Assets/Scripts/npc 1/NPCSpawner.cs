using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class NPC
{
    public int npc_id;
    public string name;
    public string prefab_name;
    public float position_x;
    public float position_y;
    public float position_z;
    public int appear_story_id;
    public int disappear_story_id;
    public bool has_voice;
}

[System.Serializable]
public class NPCListWrapper
{
    public List<NPC> npc;
}

public class NPCSpawner : MonoBehaviour
{
    public string resourcesPath = "Prefabs/NPCs";
    public int currentStoryId = 9;

    private List<GameObject> spawnedNPCs = new List<GameObject>();
    public int fixedStoryId = 9; // Inspector에서 직접 설정할 값
    
    // void Start()
    // {
    //     Debug.Log("시작됨!");
    //     StartCoroutine(LoadNPCsFromDB(currentStoryId));
    // }
    
    void Start()
    {
        Debug.Log("🟢 NPCSpawner Start() 호출됨");

        // 1. GameDataManager에 저장된 값이 있는지 먼저 확인합니다.
        if (GameDataManager.nextStoryIdToLoad > 0)
        {
            // 2. 값이 있다면, 그 값을 현재 스토리 ID로 사용합니다.
            currentStoryId = GameDataManager.nextStoryIdToLoad;
            Debug.Log($"✅ GameDataManager로부터 Story ID ({currentStoryId})를 가져왔습니다.");
        }
        else
        {
            // 3. 저장된 값이 없다면 (첫 씬일 경우), 인스펙터의 초기값을 사용합니다.
            currentStoryId = fixedStoryId;
            Debug.Log("✅ GameDataManager에 값이 없어 fixedStoryId로 설정: " + currentStoryId);
        }

        RemoveAllNPCs();
        StartCoroutine(LoadNPCsFromDB(currentStoryId));
    }

    IEnumerator LoadNPCsFromDB(int storyId)
    {
        string url = "http://127.0.0.1:5000/command";
        string jsonData = "{\"command\":\"npc_list\",\"payload\":{\"appear_story_id\":" + storyId + "}}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log("📨 요청 전송 완료");
        Debug.Log("📡 서버 응답 코드: " + request.responseCode);
        Debug.Log("📡 서버 응답 내용: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            // ✨ --- 서버로부터 받은 응답(JSON) 원본을 출력 --- ✨
            Debug.Log("📨 [Response] 서버로부터 받은 원본 JSON: " + request.downloadHandler.text);

            
            string wrappedJson = "{\"npc\":" + request.downloadHandler.text + "}";
            NPCListWrapper npcList = JsonUtility.FromJson<NPCListWrapper>(wrappedJson);

            Debug.Log("🧪 받아온 NPC 수: " + npcList.npc.Count);

            if (npcList == null || npcList.npc == null)
            {
                Debug.LogError("❌ NPC 리스트가 null입니다. JSON 파싱 실패 가능성");
                yield break;
                
            }

            foreach (var npc in npcList.npc)
            {
                string prefabPath = $"{resourcesPath}/{npc.prefab_name}";
                GameObject npcPrefab = Resources.Load<GameObject>(prefabPath);

                if (npcPrefab == null)
                {
                    Debug.LogWarning("❗ 프리팹을 찾을 수 없습니다: " + prefabPath);
                    continue;
                }

                Vector3 spawnPos = new Vector3(npc.position_x, npc.position_y, npc.position_z);
                //Debug.Log("퇴장해야할 스토리 ID " + npc.disappear_story_id);
                
                // ✅ 이곳에 조건문 추가!
                if (storyId >= npc.appear_story_id &&
                    (npc.disappear_story_id == 0 || storyId < npc.disappear_story_id))
                {
                    
                    // 이미 생성된 npc_id가 있으면 건너뛰기
                    if (spawnedNPCs.Any(obj => 
                            obj.GetComponent<NPCInteraction>()?.npcId == npc.npc_id))
                    {
                        continue;
                    }
                    
                    //Debug.Log("나 지나감~");
                    //GameObject newNPC = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
                    GameObject newNPC = Instantiate(npcPrefab, spawnPos, npcPrefab.transform.rotation);

                    spawnedNPCs.Add(newNPC);

                    // NPC 생성 후, 컴포넌트에 값 주입
                    NPCInteraction interaction = newNPC.GetComponent<NPCInteraction>();
                    if (interaction != null)
                    {
                        interaction.npcId = npc.npc_id;
                        interaction.storyId = npc.appear_story_id;
                        interaction.disappearStoryId = npc.disappear_story_id;

                        // ✅ 싱글톤으로 안전하게 연결
                        if (UIManager.Instance != null && UIManager.Instance.storyNarrationText != null)
                        {
                            interaction.storyNarrationText = UIManager.Instance.storyNarrationText;
                            Debug.Log("✅ 나래이션 텍스트 연결 성공 (UIManager)");
                        }
                        else
                        {
                            Debug.LogWarning("❗ UIManager 또는 나래이션 텍스트가 null입니다.");
                        }
                    }


                    newNPC.name = npc.name;
                }
            }

        }
        else
        {
            Debug.LogError("❌ NPC 불러오기 실패: " + request.error);
            Debug.Log("서버 응답 내용: " + request.downloadHandler.text);
        }
    }
    
    public void SpawnNPCsForStory(int storyId)
    {
        Debug.Log("🟢 SpawnNPCsForStory 호출됨 - storyId: " + storyId);

        // 기존 NPC 중 조건에 맞지 않는 것 제거
        foreach (var obj in spawnedNPCs.ToList())
        {
            var interaction = obj.GetComponent<NPCInteraction>();
            if (interaction == null) continue;

            // 퇴장 조건 도달했거나, 아직 등장 시점 안된 경우 삭제
            if (storyId < interaction.storyId || 
                (interaction.disappearStoryId > 0 && storyId >= interaction.disappearStoryId))
            {
                Destroy(obj);
                spawnedNPCs.Remove(obj);
            }
        }

        // 서버에서 받아온 NPC 리스트 기준으로 새로 등장할 NPC 스폰
        StartCoroutine(LoadNPCsFromDB(storyId));
    }
    
    public void RemoveAllNPCs()
    {
        foreach (var npc in spawnedNPCs)
        {
            if (npc != null)
                Destroy(npc);
        }

        spawnedNPCs.Clear();
    }


}
