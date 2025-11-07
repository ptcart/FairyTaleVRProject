using UnityEngine;
using UnityEngine.Video;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// ✅ DB 전용 비디오 컨트롤러
/// - DB에서 받은 media_path(string)를 기반으로 StreamingAssets에서 영상 재생
/// - 예: DB에 "Videos/page1.mp4" → 실제 경로: Assets/StreamingAssets/Videos/page1.mp4
/// </summary>
public class OBPageVideoController : MonoBehaviour
{
    [Header("필수")]
    [Tooltip("화면에 연결된 VideoPlayer")]
    public VideoPlayer videoPlayer;

    /// <summary>
    /// ▶️ 비디오 재생
    /// </summary>
    public void PlayVideoFromPath(string mediaPath)
    {
        if (videoPlayer == null || string.IsNullOrEmpty(mediaPath))
            return;

        // StreamingAssets 기준 절대 경로
        string fullPath = Path.Combine(Application.streamingAssetsPath, mediaPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[OBPageVideoController] 파일을 찾을 수 없음: {fullPath}");
            return;
        }

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = "file://" + fullPath;
        videoPlayer.isLooping = true;
        videoPlayer.Play();

        Debug.Log($"▶️ DB 영상 재생: {videoPlayer.url}");
    }

    /// <summary>
    /// ⏹ 비디오 정지
    /// </summary>
    public void StopVideo()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        // RenderTexture 초기화 (영상이 남아있을 때 화면 검정 처리용)
        if (videoPlayer.targetTexture != null)
        {
            var rt = videoPlayer.targetTexture;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }
    }
    
    // VideoPlayer 재생 전에 해당 페이지의 RawImage.texture를 targetTexture로 지정
    public void SetTargetTextureFromRawImage(RawImage rawImage)
    {
        if (rawImage == null || rawImage.texture == null) return;

        videoPlayer.targetTexture = rawImage.texture as RenderTexture;
        Debug.Log($"🎯 VideoPlayer.targetTexture <- {rawImage.name}.{rawImage.texture.name}");
    }

}