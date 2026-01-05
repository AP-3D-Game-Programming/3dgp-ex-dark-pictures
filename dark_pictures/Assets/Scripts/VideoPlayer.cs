using UnityEngine;
using UnityEngine.Video;


public class VideoPlayer : MonoBehaviour
{
    [SerializeField] string videoFileName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayVideo();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PlayVideo()
    {
        var videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        if (videoPlayer)
        {
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            Debug.Log(videoPath);
            videoPlayer.url = videoPath;
            videoPlayer.Play();
        }

    }
}
