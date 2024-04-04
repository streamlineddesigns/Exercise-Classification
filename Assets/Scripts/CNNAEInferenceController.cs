
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class CNNAEInferenceController : MonoBehaviour
{
    public Tensor output;

    public int[][] outputs;
    public float[] outputsFlat;
    public HeatmapVisual HeatmapVisual;

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

        int rows = 28;
        int colums = 28;

        List<int[]> cells = new List<int[]>();
        outputsFlat = new float[784];

        for (int i = 0; i < rows; i ++) {
            cells.Add(new int[colums]);
            for (int j = 0; j < colums; j ++) { 
                int currentPosition = (int) (i * 28);
                currentPosition += j;
                int val = (output[currentPosition] >= 0.5f) ? 1 : 0;
                cells[i][j] = (val);
                outputsFlat[currentPosition] = (float) val;
            }
        }

        outputs = cells.ToArray();
        HeatmapVisual.SetHeatMap(outputs);

        inputs.Dispose();
    }
}