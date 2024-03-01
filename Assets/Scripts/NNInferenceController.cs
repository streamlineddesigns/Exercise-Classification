
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using Data;

public class NNInferenceController : MonoBehaviour
{
    public float[] outputf;
    public Tensor output;
    public float[] startPosition;
    public float[] endPosition;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    private Dictionary<string, UserExerciseData> exercises;
    private string exerciseSaveFilePath;

    private bool isRunning;
    private bool switcher;
    private bool middle;
    private float countTime;

    private bool startSet;
    private float[] startPoses;

    private bool endSet;
    private float[] endPoses;

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
        //load start position from exercise data repository
        startPosition = AppManager.Singleton.ExerciseDataRepository.data.Where(x => x.name == name).First().startPosition;
        endPosition = AppManager.Singleton.ExerciseDataRepository.data.Where(x => x.name == name).First().endPosition;

        //check user exercise data to see if they edited the exercise
        string dir = Application.persistentDataPath;
        exerciseSaveFilePath = (dir + "/UserExerciseData.dat");
        StartCoroutine(Load());

        isRunning = true;
        StartCoroutine(Run());
    }

    protected void OnExerciseEnded(string name)
    {
        isRunning = false;
        StopAllCoroutines();
    }

    IEnumerator Load()
    {
        exercises = DataSaveManager.Deserialize<Dictionary<string, UserExerciseData>>(exerciseSaveFilePath);
        yield return new WaitUntil(() => exercises != null);
        
        if (exercises.ContainsKey(AppManager.Singleton.currentExerciseName)) {
            startPosition = exercises[AppManager.Singleton.currentExerciseName].startPosition;
            endPosition = exercises[AppManager.Singleton.currentExerciseName].endPosition;
        }
    }

    IEnumerator Run()
    {        
        startPoses = new float[MoveNetSinglePoseSample.poses.Count * 2];
        int count = 0;

        int StartToEndCount = 0;
        int EndToStartCount = 0;

        while(isRunning) {
            float[] dir = VectorUtils.GetDirection(MoveNetSinglePoseSample.previousPoses, MoveNetSinglePoseSample.currentPoses);
            float[] normDir = VectorUtils.NormalizeDirection(dir);

            float[] StartToEndDir = VectorUtils.GetDirection(startPosition, endPosition);
            float[] StartToEndNormDir = VectorUtils.NormalizeDirection(StartToEndDir);

            float[] EndToStartDir = VectorUtils.GetDirection(endPosition, startPosition);
            float[] EndToStartNormDir = VectorUtils.NormalizeDirection(EndToStartDir);

            float startToEndDistance = VectorUtils.GetDistance(dir, StartToEndDir);
            float endToStartDistance = VectorUtils.GetDistance(dir, EndToStartDir);

            float startToEndNormDistance = VectorUtils.GetDistance(normDir, StartToEndNormDir);
            float endToStartNormDistance = VectorUtils.GetDistance(normDir, EndToStartNormDir);

            float distanceThresholdForTravelingInADirection = startToEndDistance * 0.1f;

            float currentPoseStartDistance = VectorUtils.GetDistance(MoveNetSinglePoseSample.currentPoses, startPosition);
            float currentPoseEndDistance = VectorUtils.GetDistance(MoveNetSinglePoseSample.currentPoses, endPosition);

                if (switcher) {
                    //look for similarity between current pose and start position of exercise
                    if (currentPoseStartDistance < 0.5f && currentPoseStartDistance + 0.1f < currentPoseEndDistance) {
                        if (Mathf.Abs(Time.time - countTime) >= 0.0333f) {
                            countTime = Time.time;
                            switcher = false;
                            startSet = false;
                        }
                    }
                }
                
                //this might never be triggered in slow movements because startToEndDistance is a dissimilarity score between 2 directional vectors..
                //look for similarity in direction of poses and direction of start to end positions in the exercise
                if (! switcher && !middle && startToEndDistance <= 0.5f && startToEndDistance + 0.1f < endToStartDistance) {
                    StartToEndCount++;
                    //Debug.Log(StartToEndCount);
                    if (Mathf.Abs(Time.time - countTime) >= 0.0333f) {
                        countTime = Time.time;
                    
                        if (! startSet) {
                            startSet = true;
                            Array.Copy(MoveNetSinglePoseSample.currentPoses, startPoses, MoveNetSinglePoseSample.poses.Count);
                        }

                        if (VectorUtils.GetDistance(startPoses, MoveNetSinglePoseSample.currentPoses) >= distanceThresholdForTravelingInADirection) {
                            middle = true;
                        }
                    }
                }

                if (! switcher && middle) {
                    //look for similarity between current pose and end position of exercise
                    if (currentPoseEndDistance < 0.5f && currentPoseEndDistance + 0.1f < currentPoseStartDistance) {
                        if (Mathf.Abs(Time.time - countTime) >= 0.0333f) {
                            countTime = Time.time;
                            //Debug.Log(count);
                            count++;
                            switcher = true;
                            startSet = false;
                            middle = false;
                        }
                    }
                }
            
            float[] start = new float[3] {Mathf.Abs(1.0f - currentPoseStartDistance), 0.0f, 0.0f};
            float[] end = new float[3] {0.0f, Mathf.Abs(1.0f - currentPoseEndDistance), 0.0f};
            float[] falsePositives = new float[3] {0.0f, 0.0f, 0.0f};
            outputf = new float[]{start[0], end[1], falsePositives[2]};
            output = new Tensor(1, 1, 3, 1, outputf);

            yield return new WaitForSeconds(0.1f);
        }
    }
}