
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
    private const float THRESHOLD = 0.1f;
    private const float MIDDLE_THRESHOLD = 0.05f;
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
        yield return new WaitUntil(() => AppManager.Singleton.CNNEInferenceController.reconstructedImageRepresentation != null && MoveNetSinglePoseSample.resampledPoses.Count > 0);
        int count = 0;

        while(isRunning) {
            /*List<Vector3> currentPosesTemp = VectorUtils.GetDirectionVectors(MoveNetSinglePoseSample.resampledPoses.ToList());
            List<float> currentPosesTempFloat = new List<float>();
            for (int i = 0; i < currentPosesTemp.Count; i++) {
                currentPosesTempFloat.Add(currentPosesTemp[i].x);
                currentPosesTempFloat.Add(currentPosesTemp[i].y);
            }
            
            List<float> temp = new List<float>();
            temp.AddRange(MoveNetSinglePoseSample.currentPoses);
            temp.AddRange(MoveNetSinglePoseSample.normalizedPoseDirection);
            temp.AddRange(currentPosesTempFloat);
            temp.AddRange(AppManager.Singleton.CNNEInferenceController.reconstructedImageRepresentation.ToList());*/

            if (recentPoses.Count == timesteps) {
                recentPoses.Dequeue(); 
            }
            recentPoses.Enqueue(AppManager.Singleton.CNNEInferenceController.reconstructedImageRepresentation);

            if (recentPoses.Count == timesteps) {
                ForwardPass();
            }

            if (output == null) {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            if (! switcher && output[0] >= THRESHOLD && output[0] > output[1]) {
                switcher = true;
            } else if (switcher && !middle && output[1] >= MIDDLE_THRESHOLD && output[1] > output[0]) {
                middle = true;
            } else if (switcher && middle && output[1] >= THRESHOLD && output[1] > output[0]) {
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
        float[][] rp = recentPoses.ToArray();
        //Tensor inputs = new Tensor(1, timesteps, 68, 1, rp);

        Tensor reconstructedImageRepresentation = new Tensor(1, timesteps, 392, 1, rp);

        BarracudaWorker.Execute(reconstructedImageRepresentation);
        output = BarracudaWorker.PeekOutput();
        //inputs.Dispose();
        reconstructedImageRepresentation.Dispose();
    }
}