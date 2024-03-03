using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Data;

public class ExerciseSettingsController : Controller
{
    public TMP_Text ExerciseNameText;
    public GameObject dataBalancingAvailabilityMessage;
    public Slider dataBalancingSlider;
    public TMP_Text YourDataPercent;
    public TMP_Text OurDataPercent;

    public bool isDataBalancingAvailable;
    private float balancingValue;

    protected void OnEnable()
    {
        EventPublisher.OnExerciseSelected += OnExerciseSelected;
    }

    protected void OnDisable()
    {
        StopAllCoroutines();
        EventPublisher.OnExerciseSelected -= OnExerciseSelected;
    }

    protected void OnExerciseSelected(string name)
    {
        ExerciseNameText.text = name;
        LoadUserExerciseData();
        StartCoroutine(LoadUserBalancingValue());
    }

    public void PersonalizeDataButtonClick()
    {
        UIController.Singleton.CloseImmediately(ViewName.ExerciseSettings);
        UIController.Singleton.Open(ViewName.ExerciseNNRecord);
        EventPublisher.PublishExerciseEnded(AppManager.Singleton.currentExerciseName);
    }

    public void OnBalancingSliderValueChanged()
    {
        if (isDataBalancingAvailable) {
            balancingValue = dataBalancingSlider.value;
            UserDataManager.Singleton.exercises[ExerciseNameText.text].balancingValue = balancingValue;
            UpdateDataBalancingText();
        }
    }

    public void CloseButtonClick()
    {
        UIController.Singleton.CloseImmediately(ViewName.ExerciseSettings);
        if (isDataBalancingAvailable) SaveUserExerciseData();
    }

    IEnumerator LoadUserBalancingValue()
    {
        yield return null;
        
        if (UserDataManager.Singleton.exercises.ContainsKey(ExerciseNameText.text)) {
            isDataBalancingAvailable = true;
            balancingValue = UserDataManager.Singleton.exercises[ExerciseNameText.text].balancingValue;
        } else {
            isDataBalancingAvailable = false;
            balancingValue = 1.0f;
        }

        dataBalancingAvailabilityMessage.SetActive(! isDataBalancingAvailable);
        dataBalancingSlider.interactable = isDataBalancingAvailable;
        dataBalancingSlider.value = balancingValue;

        UpdateDataBalancingText();
    }

    protected void UpdateDataBalancingText()
    {
        int ourDataPercent = (int)(balancingValue * 100.0f);
        int yourDataPercent = (int)(100.0f - ourDataPercent);

        OurDataPercent.text = (ourDataPercent).ToString() + "%";
        YourDataPercent.text = (yourDataPercent).ToString() + "%";
    }

    protected void LoadUserExerciseData()
    {
        UserDataManager.Singleton.Load();
    }

    private void SaveUserExerciseData()
    {
        UserDataManager.Singleton.Save();
        EventPublisher.PublishUserExerciseDataChanged(ExerciseNameText.text);
    }
}