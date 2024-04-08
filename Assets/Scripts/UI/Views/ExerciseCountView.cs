using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseCountView : View
{
    void OnEnable()
    {
        MobileCamKit.Singleton.setBrightness(50);
        MobileCamKit.Singleton.setAutoFocus();
        MobileCamKit.Singleton.setAutoWhiteBalance();
        MobileCamKit.Singleton.setVideoResolution(Screen.width, Screen.height);
    }
}
    