
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using Data;
using DG.Tweening;

public class NNInferenceController : MonoBehaviour
{
    public float[] outputf;
    public Tensor output;
    public float[] startPosition;
    public float[] endPosition;
    public Ease easeFunction;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    private bool isRunning;
    private string currentExerciseName;

    private float[] previousPositionRepresentation;
    private float[] currentPositionRepresentation;

    private float startPositionSimilarity;
    private float endPositionSimilarity;

    protected void OnEnable()
    {
        EventPublisher.OnExerciseSelected += OnExerciseSelected;
        EventPublisher.OnExerciseEnded    += OnExerciseEnded;
        EventPublisher.OnUserExerciseDataChanged += OnUserExerciseDataChanged;
    }

    protected void OnDisable()
    {
        StopAllCoroutines();
        EventPublisher.OnExerciseSelected -= OnExerciseSelected;
        EventPublisher.OnExerciseEnded    -= OnExerciseEnded;
        EventPublisher.OnUserExerciseDataChanged -= OnUserExerciseDataChanged;
    }

    protected void OnExerciseSelected(string name)
    {
        currentExerciseName = name;

        LoadRepoExerciseData();

        StartCoroutine(Load());

        isRunning = true;
        StartCoroutine(Run());
    }

    protected void OnExerciseEnded(string name)
    {
        isRunning = false;
        StopAllCoroutines();
    }

    protected void OnUserExerciseDataChanged(string name)
    {
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        UserDataManager.Singleton.Load();
        
        yield return new WaitUntil(() => UserDataManager.Singleton.exercises != null);
        
        bool isUserDataAvailable = false;

        if (UserDataManager.Singleton.exercises.ContainsKey(AppManager.Singleton.currentExerciseName)) {
            float dataBalancingValue = UserDataManager.Singleton.exercises[AppManager.Singleton.currentExerciseName].balancingValue;
            float pw = 1.0f - dataBalancingValue;
            if (pw > 0.0f) {
                //Debug.Log("Loading USER data for NN");
                isUserDataAvailable = true;
                startPosition = UserDataManager.Singleton.exercises[AppManager.Singleton.currentExerciseName].startPosition;
                endPosition = UserDataManager.Singleton.exercises[AppManager.Singleton.currentExerciseName].endPosition;
            }

            if (! isUserDataAvailable) {
                LoadRepoExerciseData();
            }
        }
    }

    protected void LoadRepoExerciseData()
    {
        //Debug.Log("Loading REPO data for NN");
        startPosition = AppManager.Singleton.ExerciseDataRepository.data.Where(x => x.name == currentExerciseName).First().startPosition;
        endPosition = AppManager.Singleton.ExerciseDataRepository.data.Where(x => x.name == currentExerciseName).First().endPosition;
    }

    IEnumerator Run()
    {        
        yield return new WaitUntil(() => MoveNetSinglePoseSample.resampledPoses.Count > 0);

        currentPositionRepresentation = new float[MoveNetSinglePoseSample.currentPoses.Length * 3];
        previousPositionRepresentation = new float[MoveNetSinglePoseSample.currentPoses.Length * 3];
        int count = 0;

        int StartToEndCount = 0;
        int EndToStartCount = 0;

        while(isRunning) {
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

            previousPositionRepresentation = currentPositionRepresentation.ToArray();
            currentPositionRepresentation = temp.ToArray();

            //float[] normDir = VectorUtils.NormalizeDirection(VectorUtils.GetDirection(previousPositionRepresentation, currentPositionRepresentation));
            //float[] StartToEndNormDir = VectorUtils.NormalizeDirection(VectorUtils.GetDirection(startPosition, endPosition));
            //float[] EndToStartNormDir = VectorUtils.NormalizeDirection(VectorUtils.GetDirection(endPosition, startPosition));

            startPositionSimilarity = VectorUtils.CosineSimilarity(currentPositionRepresentation, startPosition); 
            endPositionSimilarity = VectorUtils.CosineSimilarity(currentPositionRepresentation, endPosition);

            //startPositionSimilarity -= VectorUtils.CosineSimilarity(normDir, StartToEndNormDir);
            //endPositionSimilarity -= VectorUtils.CosineSimilarity(normDir, EndToStartNormDir);

            float easedStart = DOVirtual.EasedValue(0.0f, 1.0f, startPositionSimilarity, easeFunction);
            float easedEnd = DOVirtual.EasedValue(0.0f, 1.0f, endPositionSimilarity, easeFunction);
            
            float[] falsePositives = new float[3] {0.0f, 0.0f, 0.0f};
            outputf = new float[]{easedStart, easedEnd, falsePositives[2]};
            output = new Tensor(1, 1, 3, 1, outputf);

            yield return new WaitForSeconds(0.1f);
        }
    }
}