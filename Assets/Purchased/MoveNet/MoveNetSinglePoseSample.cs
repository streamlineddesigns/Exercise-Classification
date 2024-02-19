using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TensorFlowLite;
using TensorFlowLite.MoveNet;

[RequireComponent(typeof(WebCamInput))]
public class MoveNetSinglePoseSample : MonoBehaviour
{
    public List<Vector3> poses = new List<Vector3>(17);

    [SerializeField]
    private bool isDebugOn = false;

    [SerializeField]
    MoveNetSinglePose.Options options = default;

    [SerializeField]
    private RectTransform cameraView = null;

    [SerializeField]
    private bool runBackground = false;

    [SerializeField, Range(0, 1)]
    private float threshold = 0.3f;
    
    private WebCamInput webCamInput;

    private MoveNetSinglePose moveNet;
    private MoveNetPose pose;
    private MoveNetDrawer drawer;

    private UniTask<bool> task;
    private CancellationToken cancellationToken;

    private void Start()
    {
        moveNet = new MoveNetSinglePose(options);
        drawer = new MoveNetDrawer(Camera.main, cameraView);

        cancellationToken = this.GetCancellationTokenOnDestroy();

        webCamInput = GetComponent<WebCamInput>();
        webCamInput.OnTextureUpdate.AddListener(OnTextureUpdate);
    }

    private void OnDestroy()
    {
        webCamInput.OnTextureUpdate.RemoveListener(OnTextureUpdate);
        moveNet?.Dispose();
        drawer?.Dispose();
    }

    public void ToggleCamera()
    {
        if (this.enabled) {
            webCamInput.ToggleCamera();
        }
    }

    public MoveNetPose GetPose()
    {
        return pose;
    }

    public void CleanUp()
    {
        runBackground = false;
        webCamInput.OnTextureUpdate.RemoveListener(OnTextureUpdate);
    }

    private void Update()
    {
        if (pose != null)
        {
            if (isDebugOn) drawer.DrawPose(pose, threshold);
            
            for (int i = 0; i < pose.Length; i++)
            {
                poses[i] = new Vector3(pose[i].x, pose[i].y, pose[i].score);
            }
        }
    }

    private void OnTextureUpdate(Texture texture)
    {
        if (runBackground)
        {
            if (task.Status.IsCompleted())
            {
                task = InvokeAsync(texture);
            }
        }
        else
        {
            Invoke(texture);
        }
    }

    private void Invoke(Texture texture)
    {
        moveNet.Invoke(texture);
        pose = moveNet.GetResult();
    }

    private async UniTask<bool> InvokeAsync(Texture texture)
    {
        await moveNet.InvokeAsync(texture, cancellationToken);
        pose = moveNet.GetResult();
        return true;
    }
}
