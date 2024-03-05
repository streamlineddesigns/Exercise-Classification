using System;
using System.Linq;
using System.Collections;
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
    
    public List<Vector3> translatedPoses;
    public List<Vector3> resampledPoses;
    public Heatmap heatmap;
    public float[] previousPoses;
    public float[] currentPoses;
    public float[] poseDirection;
    public float[] normalizedPoseDirection;
    public float[] interpolatedPreviousPoses;
    public float[] interpolatedCurrentPoses;
    private Vector2 anchorPoint = new Vector2(0.5f, 0.1f);

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
    private Texture currentTexture;
    private bool isInvoking;

    private void Start()
    {
        moveNet = new MoveNetSinglePose(options);
        drawer = new MoveNetDrawer(Camera.main, cameraView);

        cancellationToken = this.GetCancellationTokenOnDestroy();

        webCamInput = GetComponent<WebCamInput>();
        webCamInput.OnTextureUpdate.AddListener(OnTextureUpdate);

        currentPoses = new float[poses.Count * 2];
        previousPoses = new float[poses.Count * 2];
        interpolatedCurrentPoses = new float[poses.Count * 2];
        interpolatedPreviousPoses = new float[poses.Count * 2];

        isInvoking = true;
        StartCoroutine(Invoker());
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

    private void OnTextureUpdate(Texture texture)
    {
        currentTexture = texture;
    }

    protected IEnumerator Invoker()
    {
        while(isInvoking) {

            if (currentTexture == null) {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            if (runBackground) {
                if (task.Status.IsCompleted()) {
                    task = InvokeAsync(currentTexture);
                }
            } else {
                Invoke(currentTexture);
            }

            yield return new WaitForSeconds(0.05f);
            //interpolating pose positions in-between current pose updates
            InterpolatePose();
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void Invoke(Texture texture)
    {
        moveNet.Invoke(texture);
        pose = moveNet.GetResult();
        UpdateData();
    }

    private async UniTask<bool> InvokeAsync(Texture texture)
    {
        await moveNet.InvokeAsync(texture, cancellationToken);
        pose = moveNet.GetResult();
        UpdateData();
        return true;
    }

    private void UpdateData()
    {
        if (pose != null)
        {            
            for (int i = 0; i < pose.Length; i++)
            {
                poses[i] = new Vector3(pose[i].x, pose[i].y, pose[i].score);
            }

            //the position the poses will be translated relative to
            Vector2 anchorOffset = new Vector2(poses[0].x - anchorPoint.x, poses[0].y - anchorPoint.y);
            //calculate translated poses relative to a postion
            translatedPoses = VectorUtils.TranslateRelativeToOffset(poses, anchorOffset);
            //calculate resampled poses so positions are equidistant
            resampledPoses = VectorUtils.ResampleToUniformMagnitude(translatedPoses);
            //calculate heatmap
            heatmap = VectorUtils.GetHeatMap(resampledPoses);
            //cache current, translated, resampled poses into a temp object
            List<float> tempPoses = new List<float>();
            for (int i = 0; i < resampledPoses.Count; i++) {
                tempPoses.Add(resampledPoses[i].x);
                tempPoses.Add(resampledPoses[i].y);
            }
            //store previous poses using current poses before it gets updated
            Array.Copy(currentPoses, previousPoses, currentPoses.Length);
            //store current poses as an array (using temp poses) for faster access
            currentPoses = tempPoses.ToArray();

            Array.Copy(interpolatedCurrentPoses, interpolatedPreviousPoses, currentPoses.Length);
            Array.Copy(currentPoses, interpolatedCurrentPoses, currentPoses.Length);
            //calculate direction between previous poses and current poses
            poseDirection = VectorUtils.GetDirection(previousPoses, currentPoses);
            //calculate a normalized pose direction
            normalizedPoseDirection = VectorUtils.NormalizeDirection(poseDirection);
        }
    }

    /*
     * Interpolate position of poses by using previous and current poses.
     * Should only be called in-between current pose updates!
     */
    private void InterpolatePose()
    {
        Array.Copy(interpolatedCurrentPoses, interpolatedPreviousPoses, currentPoses.Length);
        float magnitude = VectorUtils.GetDistance(previousPoses, currentPoses);
        float[] scaledPoseDirection = new float[normalizedPoseDirection.Length];
        for (int i = 0; i < normalizedPoseDirection.Length; i++) {
            scaledPoseDirection[i] = normalizedPoseDirection[i] * (magnitude / 2.0f);
        }
        interpolatedCurrentPoses = VectorUtils.GetSummation(new float[][]{currentPoses, scaledPoseDirection});
    }

    private void Update()
    {
        if (pose != null)
        {
            if (isDebugOn) drawer.DrawPose(pose, threshold);
        }
    }
}
