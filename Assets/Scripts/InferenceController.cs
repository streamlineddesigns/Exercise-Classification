
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class InferenceController : MonoBehaviour
{
    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    [SerializeField]
    private Queue<float[]> recentPoses = new Queue<float[]>(8);

    [SerializeField]
    private NNModel NNModel;

    private Unity.Barracuda.Model runtimeNNModel;
    private Unity.Barracuda.IWorker BarracudaWorker;
    private bool isRunning;
    private Vector2 anchorPoint = new Vector2(0.5f, 0.1f);

    private const int TOTAL_CLASSES = 3;
    public float[][] Inputs;
    public float[] BarracudaPredictions;

    // Start is called before the first frame update
    void Start()
    {
        runtimeNNModel = ModelLoader.Load(NNModel);
        BarracudaWorker = WorkerFactory.CreateWorker(runtimeNNModel, WorkerFactory.Device.CPU);
        isRunning = true;
        StartCoroutine(Run());
    }

    public void ForwardPass()
    {
        float[][] rp = recentPoses.ToArray();

        Tensor inputs = new Tensor(1, 1, 34, 8, rp); 
        BarracudaWorker.Execute(inputs);
        Tensor output = BarracudaWorker.PeekOutput();

        if (output[0] > output[1] && output[0] > output[2]) {
            Debug.Log("D");
        }
        if (output[1] > output[0] && output[1] > output[2]) {
            Debug.Log("U");
        }

        inputs.Dispose();
    }

    IEnumerator Run()
    {
        while(isRunning) {
            List<float> poses = new List<float>();

            // sp + dv = fp
            //-sp       -sp
            //      dv = fp -sp
            Vector2 anchorOffset = new Vector2(MoveNetSinglePoseSample.poses[0].x - anchorPoint.x, MoveNetSinglePoseSample.poses[0].y - anchorPoint.y);

            for (int i = 0; i < MoveNetSinglePoseSample.poses.Count; i++) {
                //sp + dv = fp
                //   - dv  -dv
                //     sp = fp - dv
                poses.Add(MoveNetSinglePoseSample.poses[i].x - anchorOffset.x);
                poses.Add(MoveNetSinglePoseSample.poses[i].y - anchorOffset.y);
            }

            if (recentPoses.Count == 8) {
                recentPoses.Dequeue(); 
            }
            recentPoses.Enqueue(poses.ToArray());

            if (recentPoses.Count == 8) {
                ForwardPass();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}