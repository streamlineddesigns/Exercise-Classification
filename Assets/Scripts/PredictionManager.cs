
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
    public float moveStep = 0.222f;
    public float thresholdToMove = 0.0f;
    public float movingZeroPrediction = 0.0f;
    public float movingOnePrediction = 0.0f;
    private Queue<float> movingZeroPredictions = new Queue<float>(4);
    private Queue<float> movingOnePredictions = new Queue<float>(4);
    private Queue<float> originalOnePredictions = new Queue<float>(4);

    [SerializeField] private MoveNetSinglePoseSample MoveNetSinglePoseSample;
    [SerializeField] private NNInferenceController NNInferenceController;
    [SerializeField] private MLPInferenceController MLPInferenceController;
    [SerializeField] private LSTMInferenceController LSTMInferenceController;
    [SerializeField] private CNNInferenceController CNNInferenceController;

    private string currentExerciseName;
    private bool isRunning;
    private const int CLASSES_COUNT = 3;
    
    private bool switcher;
    private bool middle;
    private const float THRESHOLD = 0.7f;
    private const float MIDDLE_THRESHOLD = 0.45f;
    private float countTime;

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
        UserDataManager.Singleton.Load();
    }

    IEnumerator Run()
    {
        yield return new WaitUntil(() => UserDataManager.Singleton.exercises != null);

        count = 0;

        float previousZeroPrediction = 0.0f;
        float currentZeroPredicition = 0.0f;
        float previousOnePrediction = 0.0f;
        float currentOnePredicition = 0.0f;

        while(isRunning) {
            //wait for dependencies to load up
            if (NNInferenceController.output == null || MLPInferenceController.output == null || LSTMInferenceController.output == null || CNNInferenceController.output == null) {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            //run the forward pass
            ForwardPass();

            //old inference routine
            /*if (output[0] >= THRESHOLD && output[0] > output[1]) {
                if (! switcher) {
                    movingOnePrediction = 0.0f;
                    switcher = true;
                }
            } else if (output[1] >= THRESHOLD && output[1] > output[0]) {
                if (switcher) {
                    switcher = false;
                    count++;
                    movingZeroPrediction = 0.0f;
                }
            }*/

            /*
             * Averaging Filter
             */

            //manage the queue size ie time horizon
            if (originalOnePredictions.Count == 4) {
                originalOnePredictions.Dequeue(); 
            }

            //queue up the output prediction's one class
            originalOnePredictions.Enqueue(output[1]);

            //reset the output prediction's one class to the average of the queue values
            if (originalOnePredictions.Count == 4) {
                float[] oops = originalOnePredictions.ToArray();
                float average = oops.Sum() / oops.Length;
                output[1] = average;
            }

#region predictionStep
            /*
             * Rate Limiter Filter
             */

            //checks if the current output is greater than the previous output. ie increasing in value
            if (output[0] > previousZeroPrediction + thresholdToMove) { 
                //checks if the currrent prediction is less than the output                
                if (movingZeroPrediction < output[0]) {
                    //if so, add a small step to it; making its movement linear, and incrementing it towards the current prediction
                    movingZeroPrediction += moveStep;
                }
            //checks if the current output is less than the previous output. ie decreasing in value
            } else if (output[0] < previousZeroPrediction - thresholdToMove) {
                //checks if the currrent prediction is greater than the output    
                if (movingZeroPrediction > output[0]) {
                    //if so, subtract a small step to it; making its movement linear, and incrementing it towards the current prediction
                    movingZeroPrediction -= moveStep;
                }
            }

#endregion

            //" "
            //same as #region predictionStep except performed on one class predictions
            if (output[1] > previousOnePrediction + thresholdToMove) {
                if (movingOnePrediction < output[1]) {
                    movingOnePrediction += moveStep;
                }
            } else if (output[1] < previousOnePrediction - thresholdToMove) {
                if (movingOnePrediction > output[1]) {
                    movingOnePrediction -= moveStep;
                }
            }

            //clamp the predicted values between 0.0 and 1.0
            movingZeroPrediction = Mathf.Clamp(movingZeroPrediction, 0.0f, 1.0f);//(movingZeroPrediction < 0.0f) ? 0.0f : (movingZeroPrediction > 1.0f) ? 1.0f : movingZeroPrediction;
            movingOnePrediction = Mathf.Clamp(movingOnePrediction, 0.0f, 1.0f);//(movingOnePrediction < 0.0f) ? 0.0f : (movingOnePrediction > 1.0f) ? 1.0f : movingOnePrediction;

            //keep track of previous and current zero/one predictions
            previousZeroPrediction = currentZeroPredicition;
            currentZeroPredicition = output[0];
            previousOnePrediction = currentOnePredicition;
            currentOnePredicition = output[1];

            //manage the queue size ie time horizon
            if (movingOnePredictions.Count == 4) {
                movingOnePredictions.Dequeue(); 
            }

            //queue up the movingOnePrediction
            movingOnePredictions.Enqueue(movingOnePrediction);

            //reset movingOnePrediction to the average of the queue values
            if (movingOnePredictions.Count == 4) {
                float[] mops = movingOnePredictions.ToArray();
                float average = mops.Sum() / mops.Length;
                movingOnePrediction = average;
            }

            //a check to ignore output if false positive channel class prediction is higher than a threshold or if confident pose predictions are below a threshold
            if (output[2] >= 0.3f || MoveNetSinglePoseSample.poses.Count(x => x.z >= 0.3f) <= 4) {

            } else {

                /*
                 * Hysteresis Filter
                 */
                //checks if one class prediction is less than a threshold. effectively 'resetting' it
                if (movingOnePrediction < MIDDLE_THRESHOLD) {
                    if (! switcher) {
                        switcher = true;
                    }
                }
                
                //checks if one class prediction is more than a threshold. effectively 'counting' it
                if (movingOnePrediction >= THRESHOLD) {
                    if (switcher) {
                        switcher = false;
                        count++;
                    }
                }

            }
            
            yield return new WaitForSeconds(0.0333f);
        }
    }

    protected void ForwardPass()
    {
        //the original weights of each output
        float nw = 0.25f;//network's prediction weight
        float pw = 0.25f;//personalized data prediction weight

        //check if the user has personalized data for the current exercise
        if (UserDataManager.Singleton.exercises.ContainsKey(currentExerciseName) && UserDataManager.Singleton.exercises[currentExerciseName].hasUserData) {
            //formulate new weights based on their user preferences such that the weights sum to 1.0f
            float dataBalancingValue = UserDataManager.Singleton.exercises[currentExerciseName].balancingValue;
            float tempPW = 1.0f - dataBalancingValue;
            if (tempPW > 0.0f) {
                nw = dataBalancingValue / 3.0f;
                pw = 1.0f - dataBalancingValue;
            }
        }

        //Debug.Log("nw: " + nw + " pw: " + pw);

        //perform element wise multiplication between each output vector and assigned weight 
        float[] NNOutput = new float[CLASSES_COUNT]   {NNInferenceController.output[0]   * pw, NNInferenceController.output[1]   * pw, NNInferenceController.output[2]   * pw};
        float[] MLPOutput = new float[CLASSES_COUNT]  {MLPInferenceController.output[0]  * nw, MLPInferenceController.output[1]  * nw, MLPInferenceController.output[2]  * nw};
        float[] LSTMOutput = new float[CLASSES_COUNT] {LSTMInferenceController.output[0] * nw, LSTMInferenceController.output[1] * nw, LSTMInferenceController.output[2] * nw};
        float[] CNNOutput = new float[CLASSES_COUNT]  {CNNInferenceController.output[0]  * nw, CNNInferenceController.output[1]  * nw, CNNInferenceController.output[2]  * nw};
        
        //sum the weighted outputs to produce the final output
        output = VectorUtils.GetSummation(new float[][]{NNOutput, MLPOutput, LSTMOutput, CNNOutput});
    }

}