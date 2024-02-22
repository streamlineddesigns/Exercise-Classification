
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
    private int channels = 1;

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
        while(isRunning) {
            ForwardPass();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ForwardPass()
    {
        List<float[]> temp = new List<float[]>();
        for (int i = 0; i < MoveNetSinglePoseSample.translatedPoses.Count; i++) {
            temp.Add(new float[2] {MoveNetSinglePoseSample.translatedPoses[i].x, MoveNetSinglePoseSample.translatedPoses[i].y});
        }

        Tensor inputs = new Tensor(1, 1, 34, channels, temp.ToArray()); 
        BarracudaWorker.Execute(inputs);
        output = BarracudaWorker.PeekOutput();
        inputs.Dispose();
    }
}