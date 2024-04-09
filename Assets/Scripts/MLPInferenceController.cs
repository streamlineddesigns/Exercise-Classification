
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class MLPInferenceController : MonoBehaviour
{
    public Tensor output;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    [SerializeField]
    private NNModel NNModel;

    private Unity.Barracuda.Model runtimeNNModel;
    private Unity.Barracuda.IWorker BarracudaWorker;
    private bool isRunning;
    private bool switcher;
    private bool middle;
    private const float THRESHOLD = 0.7f;
    private const float MIDDLE_THRESHOLD = 0.3f;
    
    protected void OnEnable()
    {
        EventPublisher.OnExerciseSelected += OnExerciseSelected;
        EventPublisher.OnExerciseEnded    += OnExerciseEnded;
    }

    protected void OnDisable()
    {
        StopAllCoroutines();
        EventPublisher.OnExerciseSelected -= OnExerciseSelected;
        EventPublisher.OnExerciseEnded    -= OnExerciseEnded;
    }

    protected void OnExerciseSelected(string name)
    {
        isRunning = true;
        NNModel = AppManager.Singleton.ExerciseDataRepository.data.Where(x => x.name == name).First().MLPModel;
        runtimeNNModel = ModelLoader.Load(NNModel);
        BarracudaWorker = WorkerFactory.CreateWorker(runtimeNNModel, WorkerFactory.Device.CPU);
        StartCoroutine(Run());
    }

    protected void OnExerciseEnded(string name)
    {
        isRunning = false;
        StopAllCoroutines();
    }

    IEnumerator Run()
    {
        yield return new WaitUntil(() => MoveNetSinglePoseSample.resampledPoses.Count > 0);

        int count = 0;

        while(isRunning) {
            ForwardPass();

            if (! switcher && output[0] >= THRESHOLD && output[0] > output[1] && output[0] > output[2]) {
                switcher = true;
            } else if (switcher && !middle && output[1] >= MIDDLE_THRESHOLD && output[1] > output[0] && output[1] > output[2]) {
                middle = true;
            } else if (switcher && middle && output[1] >= THRESHOLD && output[1] > output[0] && output[1] > output[2]) {
                switcher = false;
                middle = false;
                count++;
                //Debug.Log(count);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ForwardPass()
    {
        List<Vector3> currentPoseDirectionVectorsTemp = VectorUtils.GetDirectionVectors(MoveNetSinglePoseSample.resampledPoses.ToList());
        List<float> currentPoseDirectionVectors = new List<float>();
        for (int i = 0; i < currentPoseDirectionVectorsTemp.Count; i++) {
            currentPoseDirectionVectors.Add(currentPoseDirectionVectorsTemp[i].x);
            currentPoseDirectionVectors.Add(currentPoseDirectionVectorsTemp[i].y);
        }
        List<float> temp = new List<float>();
        temp.AddRange(MoveNetSinglePoseSample.currentPoses);
        temp.AddRange(MoveNetSinglePoseSample.normalizedPoseDirection);
        temp.AddRange(currentPoseDirectionVectors.ToArray());

        Tensor inputs = new Tensor(1, 1, 102, 1, temp.ToArray()); 
        BarracudaWorker.Execute(inputs);
        output = BarracudaWorker.PeekOutput();
        inputs.Dispose();
    }
}