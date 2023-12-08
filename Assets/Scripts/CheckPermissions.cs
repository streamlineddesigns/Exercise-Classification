using System.Collections;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class CheckPermissions : MonoBehaviour
{
    [SerializeField]
    private MonoBehaviour WebCamInput;

    [SerializeField]
    private MonoBehaviour MoveNetSinglePoseSample;

    private bool hasCameraPermissions = false;
    private bool hasRequestedCameraPermissions = false;

    protected void Start()
    {
        #if UNITY_IOS
             StartCoroutine(CheckIOSCameraPermissions());
        #elif UNITY_ANDROID
            StartCoroutine(CheckAndroidCameraPermissions());
        #else
            hasCameraPermissions = true;
        #endif
    }

    protected void Update()
    {
        if (hasCameraPermissions) {
            this.enabled = false;
            MoveNetSinglePoseSample.enabled = true;
            WebCamInput.enabled = true;
        }
    }

    IEnumerator CheckAndroidCameraPermissions()
    {
        while(! hasCameraPermissions) {

            //android api function calls
            if (Permission.HasUserAuthorizedPermission(Permission.Camera)) {
                hasCameraPermissions = true;
            }

            if (! hasRequestedCameraPermissions) {
                Permission.RequestUserPermission(Permission.Camera);
                hasRequestedCameraPermissions = true;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator CheckIOSCameraPermissions()
    {
        while(! hasCameraPermissions) {

            //ios api function calls
            if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
                hasCameraPermissions = true;
            }

            if (! hasRequestedCameraPermissions) {
                Application.RequestUserAuthorization(UserAuthorization.WebCam);
                hasRequestedCameraPermissions = true;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
