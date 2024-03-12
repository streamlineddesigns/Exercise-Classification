using UnityEngine;

public class MobileCamKit : MonoBehaviour {

    public static MobileCamKit Singleton;
    private AndroidJavaObject plugin;
    
    void Awake()
    {
        if (Singleton == null) {
            Singleton = this;
            transform.parent = null;
            plugin = new AndroidJavaObject("com.StudioByStorm.MobileCamKit");
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    public void setBrightness(int brightness) 
    {
        plugin.Call("setBrightness", brightness);
    }

    public void setExposureCompensation(int exposureCompensation) 
    {
        plugin.Call("setExposureCompensation", exposureCompensation);
    }

    public void setContrast(int contrast) 
    {
        plugin.Call("setContrast", contrast);
    }

    public void setAutoFocus() 
    {
        plugin.Call("setAutoFocus");
    }

    public void setAutoWhiteBalance() 
    {
        plugin.Call("setAutoWhiteBalance"); 
    }

    public void setJpegQuality(int quality) 
    {
        plugin.Call("setJpegQuality", quality);
    }

    public void setFrameRate(int min, int max)
    {
        plugin.Call("setFrameRate", min, max);
    }

    public void setVideoResolution(int width, int height)
    {
        plugin.Call("setVideoResolution", width, height);
    }
}
