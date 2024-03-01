using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Data;

public class ExerciseDataInferenceController : MonoBehaviour
{
    public float[] startPosition;
    public float[] endPosition;
    public float[] centroid;

    [SerializeField] private TMP_Text CountText;
    [SerializeField] private TMP_Text CurrentExerciseNameText;
    [SerializeField] private ExerciseDataRecorder ExerciseDataRecorder;
    [SerializeField] private MoveNetSinglePoseSample MoveNetSinglePoseSample;
    [SerializeField] private MLPInferenceController MLPInferenceController;

    private bool isTrying;
    private Vector2 anchorPoint = new Vector2(0.5f, 0.1f);
    [SerializeField] private TMP_InputField exerciseNameText;
    [SerializeField] private TMP_Dropdown exerciseNameDropdown;
    [SerializeField] private GameObject tryButton;
    [SerializeField] private GameObject recordButton;
    [SerializeField] private GameObject saveButton;
    [SerializeField] private GameObject endButton;

    private bool switcher;
    private bool middle;
    private const float NEARBY_THRESHOLD = 0.4f;
    private const float FARAWAY_THRESHOLD = 0.3f;
    private const float MIDDLE_THRESHOLD = 0.2f;
    private int count = 0;
    private float countTime;
    
    private float[] currentPoses;
    private float[] previousPoses;

    private bool startSet;
    private float[] startPoses;

    private bool endSet;
    private float[] endPoses;

    protected void Start()
    {
        StartCoroutine(LoadExerciseNameDropdown());
    }

    public IEnumerator LoadExerciseNameDropdown()
    {
        yield return new WaitForSeconds(0.5f);

        exerciseNameDropdown.ClearOptions();
        List<string> names = ExerciseDataRecorder.exercises.Keys.ToList();
        exerciseNameDropdown.AddOptions(names);

        if (names.Count == 0) {
            exerciseNameDropdown.gameObject.SetActive(false);
            tryButton.gameObject.SetActive(false);
        }
    }

    IEnumerator ListenForBoth()
    {
        startPoses = new float[MoveNetSinglePoseSample.poses.Count * 2];

        int StartToEndCount = 0;
        int EndToStartCount = 0;
        //dv = fp -sp
        while(isTrying) {
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

            if (/*MLPInferenceController.output[2] >= 0.5f || */MoveNetSinglePoseSample.poses.Count(x => x.z >= 0.3f) <= 5) {

            } else {
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
                            Debug.Log(count);
                            count++;
                            CountText.text = count.ToString();
                            switcher = true;
                            startSet = false;
                            middle = false;
                        }
                    }
                }
            }
            
            

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Try()
    {
        isTrying = true;
        tryButton.SetActive(false);
        endButton.SetActive(true);
        recordButton.SetActive(false);
        saveButton.SetActive(false);
        exerciseNameText.gameObject.SetActive(false);
        exerciseNameDropdown.gameObject.SetActive(false);
        startPosition = ExerciseDataRecorder.exercises[exerciseNameDropdown.options[exerciseNameDropdown.value].text].startPosition;
        endPosition = ExerciseDataRecorder.exercises[exerciseNameDropdown.options[exerciseNameDropdown.value].text].endPosition;
        List<float[]> centroidList = new List<float[]>();
        centroidList.Add(startPosition);
        centroidList.Add(endPosition);
        centroid = VectorUtils.GetCentroid(centroidList.ToArray());
        Debug.Log(exerciseNameDropdown.options[exerciseNameDropdown.value].text);
        CurrentExerciseNameText.text = exerciseNameDropdown.options[exerciseNameDropdown.value].text;
        CurrentExerciseNameText.gameObject.SetActive(true);
        StartCoroutine(LoadExerciseNameDropdown());
        StartCoroutine(ListenForBoth());
    }

    public void Save()
    {
        StartCoroutine(LoadExerciseNameDropdown());
    }

    public void End()
    {
        isTrying = false;

        tryButton.SetActive(true);
        endButton.SetActive(false);
        recordButton.SetActive(true);
        saveButton.SetActive(false);
        exerciseNameText.gameObject.SetActive(true);
        exerciseNameDropdown.gameObject.SetActive(true);
        count = 0;
        CountText.text = count.ToString();
        CurrentExerciseNameText.gameObject.SetActive(false);
    }
}