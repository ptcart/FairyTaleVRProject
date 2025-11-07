using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SaveNPC : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(SendNPCData());
    }

    IEnumerator SendNPCData()
    {
        // 보낼 데이터 구성 (JSON 형태)
        string json = @"
        {
            ""command"": ""save_npc"",
            ""payload"": {
                ""name"": ""Grandma"",
                ""position_x"": 1.5,
                ""position_y"": 0.0,
                ""position_z"": -3.5,
                ""appear_story_id"": 2,
                ""has_voice"": true
            }
        }";

        // 요청 생성
        UnityWebRequest request = new UnityWebRequest("http://localhost:5000/command", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 요청 전송
        yield return request.SendWebRequest();

        // 결과 출력
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ NPC 저장 성공: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ 오류 발생: " + request.error);
        }
    }
}