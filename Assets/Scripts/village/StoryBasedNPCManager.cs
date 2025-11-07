using System.Collections.Generic;
using UnityEngine;

public class StoryBasedNPCManager : MonoBehaviour
{
    public static StoryBasedNPCManager Instance;
    public Transform npcParent;

    void Awake()
    {
        Instance = this;
    }

    public void LoadNPCsForStory(int currentStoryId, NpcData[] npcList)
    {
        Debug.Log($"📍 스토리 {currentStoryId}에 따른 NPC 배치 시작");

        // 현재 씬에 존재하는 NPC 추적
        Dictionary<int, GameObject> existingNPCs = new Dictionary<int, GameObject>();
        foreach (Transform child in npcParent)
        {
            string[] split = child.name.Split('_');
            if (split.Length == 2 && int.TryParse(split[1], out int id))
            {
                existingNPCs[id] = child.gameObject;
            }
        }

        HashSet<int> validNpcIds = new HashSet<int>();
        foreach (var npc in npcList)
        {
            if (npc.appearStoryId <= currentStoryId && npc.disappearStoryId >= currentStoryId)
            {
                validNpcIds.Add(npc.npcId);
            }
        }

        foreach (var pair in existingNPCs)
        {
            if (!validNpcIds.Contains(pair.Key))
            {
                Destroy(pair.Value);
            }
        }

        for (int i = 0; i < npcList.Length; i++)
        {
            var npc = npcList[i];

            if (!validNpcIds.Contains(npc.npcId) || existingNPCs.ContainsKey(npc.npcId))
                continue;

            string path = $"Prefabs/npc/{npc.prefab}";
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"❗ 프리팹 로드 실패: {path}");
                continue;
            }

            Vector3 pos = new Vector3(npc.positionX, npc.positionY, npc.positionZ);
            Quaternion rot = (npc.prefab == "tiger") ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;
            GameObject npcObj = Instantiate(prefab, pos, rot, npcParent);
            npcObj.name = $"npc_{npc.npcId}";
            Debug.Log($"✅ NPC 생성됨: {npcObj.name}");
        }
    }
}