
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class CNNInferenceController : MonoBehaviour
{
    public Tensor output;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    [SerializeField] private HeatmapVisual heatmapVisual;

    [SerializeField]
    private NNModel NNModel;

    private Unity.Barracuda.Model runtimeNNModel;
    private Unity.Barracuda.IWorker BarracudaWorker;
    private bool isRunning;
    private bool switcher;
    private bool middle;
    private const float THRESHOLD = 0.7f;
    private const float MIDDLE_THRESHOLD = 0.3f;

    protected void Start()
    {
        runtimeNNModel = ModelLoader.Load(NNModel);
        BarracudaWorker = WorkerFactory.CreateWorker(runtimeNNModel, WorkerFactory.Device.CPU);
        isRunning = true;
        StartCoroutine(Run());
    }

    protected void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator Run()
    {
        int count = 0;

        while(isRunning) {
            
            if (MoveNetSinglePoseSample.heatmap == null) {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            heatmapVisual.SetHeatMapFlattened(MoveNetSinglePoseSample.heatmap);

            ForwardPass();

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
        float[] imageRepresentation = MoveNetSinglePoseSample.heatmap.GetFlattenedHeatmap();
        Tensor inputs = new Tensor(1, 28, 28, 1, imageRepresentation);
        BarracudaWorker.Execute(inputs);
        output = BarracudaWorker.PeekOutput();
        inputs.Dispose();
    }
}