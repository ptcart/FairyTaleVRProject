using UnityEngine;
using UnityEngine.Video;

public class PageVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip[] videoClips; // 페이지별 영상들
    public RenderTexture[] renderTextures; // 페이지별 RenderTexture들

    public void PlayVideoForPage(int index)
    {
        if (index < 0 || index >= videoClips.Length || index >= renderTextures.Length)
            return;

        videoPlayer.Stop();
        videoPlayer.targetTexture = renderTextures[index];
        videoPlayer.clip = videoClips[index];
        videoPlayer.Play();
    }
}