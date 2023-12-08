
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class InferenceController : MonoBehaviour
{
    [SerializeField]
    private TestingController TestingController;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    [SerializeField]
    private Queue<float[]> recentPoses = new Queue<float[]>(8);

    [SerializeField]
    private NNModel JNNModel;

    [SerializeField]
    private NNModel PNNModel;

    private Unity.Barracuda.Model runtimeNNModel;
    private Unity.Barracuda.IWorker BarracudaWorker;
    private bool isRunning;
    private Vector2 anchorPoint = new Vector2(0.5f, 0.1f);

    private const int TOTAL_CLASSES = 3;
    private const float THRESHOLD = 0.7f;
    private bool switcher;
    private int count;

    protected void OnEnable()
    {
        EventPublisher.OnNetworkChange += OnNetworkChange;
    }

    protected void OnDisable()
    {
        EventPublisher.OnNetworkChange -= OnNetworkChange;
    }

    private void OnNetworkChange(string name)
    {
        if (isRunning) {
            return;
        }

        switch(name) {
            case "J" :
                runtimeNNModel = ModelLoader.Load(JNNModel);
                Debug.Log("J");
                break;

            case "P" :
                runtimeNNModel = ModelLoader.Load(PNNModel);
                Debug.Log("P");
                break;
        }
        
        BarracudaWorker = WorkerFactory.CreateWorker(runtimeNNModel, WorkerFactory.Device.CPU);
        isRunning = true;
        StartCoroutine(Run());
    }

    private void ForwardPass()
    {
        float[][] rp = recentPoses.ToArray();

        int channels = 1;//8;
        Tensor inputs = new Tensor(1, 1, 34, channels, rp); 
        BarracudaWorker.Execute(inputs);
        Tensor output = BarracudaWorker.PeekOutput();

        if (! switcher && output[0] >= THRESHOLD && output[0] > output[1] && output[0] > output[2]) {
            Debug.Log("D");
            switcher = true;
        } else if (switcher && output[1] >= THRESHOLD && output[1] > output[0] && output[1] > output[2]) {
            Debug.Log("U");
            switcher = false;
            count++;
            TestingController.SetCountText(count);
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