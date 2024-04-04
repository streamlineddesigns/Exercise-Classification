
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class CNNEInferenceController : MonoBehaviour
{
    public Tensor output;
    public float[] reconstructedImageRepresentation;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    [SerializeField]
    private NNModel NNModel;

    private Unity.Barracuda.Model runtimeNNModel;
    private Unity.Barracuda.IWorker BarracudaWorker;
    private bool isRunning;

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
        while(isRunning) {     
            if (MoveNetSinglePoseSample.heatmap == null) {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            ForwardPass();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ForwardPass()
    {
        float[] imageRepresentation = MoveNetSinglePoseSample.heatmap.GetFlattenedHeatmap();
        Tensor inputs = new Tensor(1, 28, 28, 1, imageRepresentation);
        BarracudaWorker.Execute(inputs);
        output = BarracudaWorker.PeekOutput();

        reconstructedImageRepresentation = new float[392];

        for (int i = 0; i < 392; i++) {
            float val = (float) output[i];
            reconstructedImageRepresentation[i] = val;
        }

        inputs.Dispose();
    }
}