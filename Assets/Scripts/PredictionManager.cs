
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using Data;

public class PredictionManager : MonoBehaviour
{
    public float[] output;
    public int count;

    [SerializeField] private MoveNetSinglePoseSample MoveNetSinglePoseSample;
    [SerializeField] private NNInferenceController NNInferenceController;
    [SerializeField] private MLPInferenceController MLPInferenceController;
    [SerializeField] private LSTMInferenceController LSTMInferenceController;
    [SerializeField] private CNNInferenceController CNNInferenceController;

    private Dictionary<string, UserExerciseData> exercises;
    private string currentExerciseName;
    private string exerciseSaveFilePath;
    private bool isRunning;
    private const int CLASSES_COUNT = 3;
    
    private bool switcher;
    private bool middle;
    private const float THRESHOLD = 0.6f;
    private const float MIDDLE_THRESHOLD = 0.3f;

    protected void Start()
    {
        output = new float[CLASSES_COUNT];
    }

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

    public void RefreshUserExerciseData()
    {
        LoadUserExerciseData();
    }

    protected void OnExerciseSelected(string name)
    {
        currentExerciseName = name;
        isRunning = true;
        LoadUserExerciseData();
        StartCoroutine(Run());
    }

    protected void OnExerciseEnded(string name)
    {
        isRunning = false;
        StopAllCoroutines();
    }

    protected void LoadUserExerciseData()
    {
        string dir = Application.persistentDataPath;
        exerciseSaveFilePath = (dir + "/UserExerciseData.dat");
        exercises = DataSaveManager.Deserialize<Dictionary<string, UserExerciseData>>(exerciseSaveFilePath);
    }

    IEnumerator Run()
    {
        yield return new WaitUntil(() => exercises != null);

        count = 0;

        while(isRunning) {

            if (NNInferenceController.output == null || MLPInferenceController.output == null || LSTMInferenceController.output == null || CNNInferenceController.output == null) {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            ForwardPass();

            if (output[2] >= 0.25f || MoveNetSinglePoseSample.poses.Count(x => x.z >= 0.3f) <= 7) {

            } else if (! switcher && output[0] >= THRESHOLD && output[0] > output[1] && output[0] > output[2]) {
                switcher = true;
            } else if (switcher && !middle && output[1] >= MIDDLE_THRESHOLD && output[1] > output[0] && output[1] > output[2]) {
                middle = true;
            } else if (switcher && middle && output[1] >= THRESHOLD && output[1] > output[0] && output[1] > output[2]) {
                switcher = false;
                middle = false;
                count++;
                //Debug.Log(count);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    protected void ForwardPass()
    {
        float nw = 0.25f;
        float pw = 0.25f;

        if (exercises.ContainsKey(currentExerciseName) && exercises[currentExerciseName].hasUserData) {
            float dataBalancingValue = exercises[currentExerciseName].balancingValue;
            float tempPW = 1.0f - dataBalancingValue;
            if (tempPW > 0.0f) {
                nw = dataBalancingValue / 3.0f;
                pw = 1.0f - dataBalancingValue;
            }
        }

        //Debug.Log("nw: " + nw + " pw: " + pw);

        float[] NNOutput = new float[CLASSES_COUNT]   {NNInferenceController.output[0]   * pw, NNInferenceController.output[1]   * pw, NNInferenceController.output[2]   * pw};
        float[] MLPOutput = new float[CLASSES_COUNT]  {MLPInferenceController.output[0]  * nw, MLPInferenceController.output[1]  * nw, MLPInferenceController.output[2]  * nw};
        float[] LSTMOutput = new float[CLASSES_COUNT] {LSTMInferenceController.output[0] * nw, LSTMInferenceController.output[1] * nw, LSTMInferenceController.output[2] * nw};
        float[] CNNOutput = new float[CLASSES_COUNT]  {CNNInferenceController.output[0]  * nw, CNNInferenceController.output[1]  * nw, CNNInferenceController.output[2]  * nw};
        
        output = VectorUtils.GetSummation(new float[][]{NNOutput, MLPOutput, LSTMOutput, CNNOutput});
    }

}