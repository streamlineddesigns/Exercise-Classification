
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class LSTMInferenceController : MonoBehaviour
{
    public Tensor output;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    [SerializeField]
    private NNModel NNModel;

    [SerializeField]
    private Queue<float[]> recentPoses = new Queue<float[]>(8);

    private Unity.Barracuda.Model runtimeNNModel;
    private Unity.Barracuda.IWorker BarracudaWorker;
    private bool isRunning;
    private bool switcher;
    private bool middle;
    private const float THRESHOLD = 0.4f;
    private const float MIDDLE_THRESHOLD = 0.2f;
    private int timesteps = 8;

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
        NNModel = AppManager.Singleton.ExerciseDataRepository.data.Where(x => x.name == name).First().LSTMModel;
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
        int count = 0;

        while(isRunning) {
            List<float> inputVector = new List<float>();
            List<float> currentPosesTemp;
            List<float> normalizedPoseDirectionTemp;

            currentPosesTemp = MoveNetSinglePoseSample.currentPoses.ToList();
            normalizedPoseDirectionTemp = MoveNetSinglePoseSample.normalizedPoseDirection.ToList();

            inputVector.AddRange(currentPosesTemp);
            inputVector.AddRange(normalizedPoseDirectionTemp);

            if (recentPoses.Count == timesteps) {
                recentPoses.Dequeue(); 
            }
            recentPoses.Enqueue(inputVector.ToArray());

            if (recentPoses.Count == timesteps) {
                ForwardPass();
            }

            if (output == null) {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            if (! switcher && output[0] >= THRESHOLD && output[0] > output[1] && output[0] > output[2]) {
                switcher = true;
            } else if (switcher && !middle && output[1] >= MIDDLE_THRESHOLD && output[1] > output[0] && output[1] > output[2]) {
                middle = true;
            } else if (switcher && middle && output[1] >= THRESHOLD && output[1] > output[0] && output[1] > output[2]) {
                switcher = false;
                middle = false;
                count++;
                Debug.Log(count);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ForwardPass()
    {
        float[][] rp = recentPoses.ToArray();
        Tensor inputs = new Tensor(1, timesteps, 68, 1, rp);
        BarracudaWorker.Execute(inputs);
        output = BarracudaWorker.PeekOutput();
        inputs.Dispose();
    }
}