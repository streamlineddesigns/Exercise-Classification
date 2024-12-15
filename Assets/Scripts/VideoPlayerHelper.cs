using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerHelper : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public int frameCount = 0;

    public int currentFrameID = 0;
    private int _currentFrameID = 0;

    // Start is called before the first frame update
    void Start()
    {
        frameCount = (int) videoPlayer.frameCount;
        Debug.Log(frameCount);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) {
            currentFrameID = ((currentFrameID - 1) >= 0) ? (currentFrameID - 1) : 0;
        }

        if (Input.GetMouseButton(1)) {
            currentFrameID = ((currentFrameID + 1) <= frameCount) ? (currentFrameID + 1) : frameCount;
        }

        if (_currentFrameID != currentFrameID) {
            _currentFrameID = currentFrameID;
            videoPlayer.frame = _currentFrameID;
        }
    }
}
