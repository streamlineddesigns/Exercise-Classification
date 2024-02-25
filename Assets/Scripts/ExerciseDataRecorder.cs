using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Data;

public class ExerciseDataRecorder : MonoBehaviour
{
    [SerializeField] private MoveNetSinglePoseSample MoveNetSinglePoseSample;
    public Dictionary<string, ExerciseData> exercises;
    private string exerciseSaveFilePath;

    private bool isRecording;
    private Vector2 anchorPoint = new Vector2(0.5f, 0.1f);
    [SerializeField] private Image ScreenshotImage;
    [SerializeField] private TMP_Text CountDownText;
    [SerializeField] private TMP_Text StartPositionText;
    [SerializeField] private TMP_Text EndPositionText;
    [SerializeField] private TMP_InputField exerciseNameText;
    [SerializeField] private TMP_Dropdown exerciseNameDropdown;
    [SerializeField] private GameObject tryButton;
    [SerializeField] private GameObject recordButton;
    [SerializeField] private GameObject saveButton;
    [SerializeField] private GameObject endButton;

    protected void Start()
    {
        string dir = Application.persistentDataPath;
        exerciseSaveFilePath = (dir + "/Exercises.dat");
        Load();
    }

    public void Record()
    {
        isRecording = true;
        exerciseNameText.gameObject.SetActive(false);
        recordButton.SetActive(false);
        saveButton.SetActive(true);
        endButton.SetActive(false);
        exerciseNameDropdown.gameObject.SetActive(false);
        tryButton.SetActive(false);
        StartCoroutine(RecordingAnimation());
    }

    public void Load()
    {
        exercises = Deserialize<Dictionary<string, ExerciseData>>(exerciseSaveFilePath);
    }

    public void Save()
    {
        Serialize<Dictionary<string, ExerciseData>>(exercises, exerciseSaveFilePath);
        saveButton.SetActive(false);
        exerciseNameDropdown.gameObject.SetActive(true);
        tryButton.SetActive(true);
        exerciseNameText.gameObject.SetActive(true);
        recordButton.SetActive(true);
        isRecording = false;
        exerciseNameText.text = "";
    }

    IEnumerator RecordingAnimation()
    {
        StartPositionText.gameObject.SetActive(true);
        int secondsToWait = 10;
        
        yield return StartCoroutine(WaitForSeconds(secondsToWait));
        
        Capture(true);
        ScreenshotImage.DOFade(1.0f, 0.25f).OnComplete(() => {
            ScreenshotImage.DOFade(0.0f, 0.25f);
        });

        yield return new WaitForSeconds(0.3f);

        StartPositionText.gameObject.SetActive(false);
        EndPositionText.gameObject.SetActive(true);

        yield return StartCoroutine(WaitForSeconds(secondsToWait));

        Capture(false);
        ScreenshotImage.DOFade(1.0f, 0.25f).OnComplete(() => {
            ScreenshotImage.DOFade(0.0f, 0.25f);
        });

        StartPositionText.gameObject.SetActive(false);
        EndPositionText.gameObject.SetActive(false);
    }

    IEnumerator WaitForSeconds(int secondsToWait)
    {        
        CountDownText.gameObject.SetActive(true);

        for (int i = secondsToWait; i > 0; i--) {
            CountDownText.text = (i).ToString();
            yield return new WaitForSeconds(1.0f);
        }

        CountDownText.gameObject.SetActive(false);
    }

    private void Capture(bool isStartPosition)
    {
        AddExercise(exerciseNameText.text, MoveNetSinglePoseSample.currentPoses, isStartPosition);
    }

    private void AddExercise(string exerciseName, float[] data, bool isStartPosition)
    {
        string key = NormalizeText(exerciseName);

        if (! exercises.ContainsKey(key)) {
            ExerciseData currentExerciseData = new ExerciseData();
            exercises.Add(key, currentExerciseData);
        }

        if (isStartPosition) {
            exercises[key].start = data;
            return;
        }

        exercises[key].end = data;
    }

    private void Serialize<T>(T obj, string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Create);

        try
        {
            formatter.Serialize(stream, obj);
        }
        catch (SerializationException e)
        {
            Debug.LogError("Serialization failed! " + e.Message);
        }
        finally
        {
            stream.Close();
        }
    }

    private T Deserialize<T>(string filePath) where T : new()
    {
        if (!File.Exists(filePath))
        {
            Debug.Log("Serialization file not found at " + filePath);
            return new T();
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Open);
        T obj = default(T);

        try
        {
            obj = (T)formatter.Deserialize(stream);
        }
        catch (SerializationException e)
        {
            Debug.LogError("Deserialization failed! " + e.Message);
        }
        finally
        {
            stream.Close();
        }

        return obj;
    }

    string NormalizeText(string input) 
    {
        // Replace all whitespace with empty string to remove
        input = input.Trim();

        // Capitalize first letter of each word
        System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("en-US",false).TextInfo;
        input = textInfo.ToTitleCase(input);
        
        return input;
    }

}