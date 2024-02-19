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
    private const float DISSIMILARITY_THRESHOLD = 0.5f;
    private int count = 0;
    private float countTime;

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

    IEnumerator ListenForSimilarity()
    {
        yield return null;

        while(isTrying) {
            float[] poses = new float[MoveNetSinglePoseSample.poses.Count];
            Vector2 anchorOffset = new Vector2(MoveNetSinglePoseSample.poses[0].x - anchorPoint.x, MoveNetSinglePoseSample.poses[0].y - anchorPoint.y);

            for (int i = 0; i < MoveNetSinglePoseSample.poses.Count; i++) {
                poses[i] = MoveNetSinglePoseSample.poses[i].x - anchorOffset.x;
                poses[i] = MoveNetSinglePoseSample.poses[i].y - anchorOffset.y;
            }

            bool isWithinThreshold = false;

            if (MoveNetSinglePoseSample.poses.Count(x => x.z >= 0.5f) <= 8) {

            } else if (switcher) {
                isWithinThreshold = IsDistanceWithinThreshold(poses, startPosition);
                float startDistance = GetDistance(poses, startPosition);
                float endDistance = GetDistance(poses, endPosition);
                if (isWithinThreshold && startDistance < DISSIMILARITY_THRESHOLD && 0.8f - DISSIMILARITY_THRESHOLD < endDistance) {
                    if (Mathf.Abs(Time.time - countTime) >= 0.0333f) {
                        countTime = Time.time;
                        switcher = false;
                    }
                }
            } else if (! switcher && !middle) {
                isWithinThreshold = IsDistanceWithinThreshold(poses, centroid);
                float startDistance = GetDistance(poses, startPosition);
                float endDistance = GetDistance(poses, endPosition);
                float centroidDistance = GetDistance(poses, centroid);
                if (isWithinThreshold && centroidDistance < DISSIMILARITY_THRESHOLD && DISSIMILARITY_THRESHOLD / 2.0f < endDistance && DISSIMILARITY_THRESHOLD / 2.0f < startDistance) {
                    if (Mathf.Abs(Time.time - countTime) >= 0.0333f) {
                        countTime = Time.time;
                        switcher = false;
                        middle = true;
                    }
                }
            } else if (! switcher && middle) {
                isWithinThreshold = IsDistanceWithinThreshold(poses, endPosition);
                float startDistance = GetDistance(poses, startPosition);
                float endDistance = GetDistance(poses, endPosition);
                if (isWithinThreshold && endDistance < DISSIMILARITY_THRESHOLD && 0.8f - DISSIMILARITY_THRESHOLD < startDistance) {
                    if (Mathf.Abs(Time.time - countTime) >= 0.0333f) {
                        countTime = Time.time;
                        switcher = true;
                        count++;
                        CountText.text = count.ToString();
                        middle = false;
                        Debug.Log(count);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    protected bool IsDistanceWithinThreshold(float[] poses, float[] targetPose)
    {
        float distance = GetDistance(poses, targetPose);
        //Debug.Log(distance);
        if (distance <= DISSIMILARITY_THRESHOLD) {
            return true;
        }
        return false;
    }

    public float GetDistance(float[] p1, float[] p2)
    {
        float distance = 0;

        if (p1.Length != p2.Length) {
            Debug.LogError("Input vectors must be of equal length");
        }

        for (int i = 0; i < p1.Length; i++) {
            float a = p1[i] - p2[i];
            distance += (float) (a * a);
        }

        return Mathf.Sqrt(distance);
    }

    public float[] GetCentroid(float[][] data)
    {
        if ((data.Select(x => x.Length).Sum() / data.Length != data[0].Length)) {
            Debug.LogError("Input vectors must be of equal length");
        }

        float[] centroid = new float[data[0].Length];
        float[] counter = new float[data[0].Length];

        for (int i = 0; i < data.Length; i++) {
            for (int j = 0; j < centroid.Length; j++) {
                centroid[j] += data[i][j];
                counter[j]++;
            }
        }

        for (int k = 0; k < centroid.Length; k++) {
            centroid[k] /= counter[k];
        }

        return centroid;
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
        startPosition = ExerciseDataRecorder.exercises[exerciseNameDropdown.options[exerciseNameDropdown.value].text].start;
        endPosition = ExerciseDataRecorder.exercises[exerciseNameDropdown.options[exerciseNameDropdown.value].text].end;
        List<float[]> centroidList = new List<float[]>();
        centroidList.Add(startPosition);
        centroidList.Add(endPosition);
        centroid = GetCentroid(centroidList.ToArray());
        Debug.Log(exerciseNameDropdown.options[exerciseNameDropdown.value].text);
        CurrentExerciseNameText.text = exerciseNameDropdown.options[exerciseNameDropdown.value].text;
        CurrentExerciseNameText.gameObject.SetActive(true);
        StartCoroutine(LoadExerciseNameDropdown());
        StartCoroutine(ListenForSimilarity());
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