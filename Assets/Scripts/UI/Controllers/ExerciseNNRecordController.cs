using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Data;

public class ExerciseNNRecordController : Controller
{
    [SerializeField] private MoveNetSinglePoseSample MoveNetSinglePoseSample;
    [SerializeField] private Image ScreenshotImage;
    [SerializeField] private TMP_Text CountDownText;
    [SerializeField] private TMP_Text StartPositionText;
    [SerializeField] private TMP_Text EndPositionText;
    [SerializeField] private TMP_Text exerciseNameText;

    private string exerciseName;
    private Dictionary<string, UserExerciseData> exercises;
    private string exerciseSaveFilePath;

    protected void OnEnable()
    {
        EventPublisher.OnExerciseSelected += OnExerciseSelected;
    }

    protected void OnDisable()
    {
        EventPublisher.OnExerciseSelected -= OnExerciseSelected;
    }

    protected void OnExerciseSelected(string name)
    {
        exerciseName = name;
        exerciseNameText.text = exerciseName;
        LoadUserExerciseData();
    }

    public void RecordUserExerciseData()
    {
        StartCoroutine(RecordingAnimation());
    }
    
    IEnumerator RecordingAnimation()
    {
        StartPositionText.gameObject.SetActive(true);
        int secondsToWait = 5;
        
        yield return StartCoroutine(PrintSeconds(secondsToWait));
        
        Capture(true);
        ScreenshotImage.DOFade(1.0f, 0.25f).OnComplete(() => {
            ScreenshotImage.DOFade(0.0f, 0.25f);
        });

        yield return new WaitForSeconds(0.3f);

        StartPositionText.gameObject.SetActive(false);
        EndPositionText.gameObject.SetActive(true);

        yield return StartCoroutine(PrintSeconds(secondsToWait));

        Capture(false);
        ScreenshotImage.DOFade(1.0f, 0.25f).OnComplete(() => {
            ScreenshotImage.DOFade(0.0f, 0.25f);
        });

        StartPositionText.gameObject.SetActive(false);
        EndPositionText.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.0f);

        SaveUserExerciseData();
    }

    IEnumerator PrintSeconds(int secondsToWait)
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
            UserExerciseData currentExerciseData = new UserExerciseData();
            exercises.Add(key, currentExerciseData);
            AddDefaultDataBalancingValue();
        }

        if (isStartPosition) {
            exercises[key].startPosition = data;
            return;
        }

        exercises[key].endPosition = data;
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

    private void AddDefaultDataBalancingValue()
    {
        exercises[exerciseNameText.text].balancingValue = 0.75f;
        exercises[exerciseNameText.text].hasUserData = true;
    }

    protected void LoadUserExerciseData()
    {
        string dir = Application.persistentDataPath;
        exerciseSaveFilePath = (dir + "/UserExerciseData.dat");
        exercises = DataSaveManager.Deserialize<Dictionary<string, UserExerciseData>>(exerciseSaveFilePath);
    }

    private void SaveUserExerciseData()
    {
        DataSaveManager.Serialize<Dictionary<string, UserExerciseData>>(exercises, exerciseSaveFilePath);
        UIController.Singleton.Open(ViewName.ExerciseCount);
        EventPublisher.PublishExerciseSelected(AppManager.Singleton.currentExerciseName);
        UIController.Singleton.OpenImmediately(ViewName.ExerciseSettings);
    }
}