using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class UnityClient : MonoBehaviour
{
    private string serverUrl = "http://127.0.0.1:5000/receive";  // Flask 서버 주소

    void Start()
    {
        StartCoroutine(SendDataToServer());
    }

    IEnumerator SendDataToServer()
    {
        // JSON 형태로 보낼 데이터
        string jsonData = "{\"player\": \"John\", \"score\": 100}";
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("📩 Server Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("❌ Error: " + request.error);
            }
        }
    }
}