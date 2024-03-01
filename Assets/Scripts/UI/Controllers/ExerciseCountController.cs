using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExerciseCountController : Controller
{
    [SerializeField] private TMP_Text ExerciseNameText;
    [SerializeField] private TMP_Text CountText;
    private bool isRunning;

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
        ExerciseNameText.text = name;
        isRunning = true;
        StartCoroutine(Run());        
    }

    protected void OnExerciseEnded(string name)
    {
        isRunning = false;
        StopAllCoroutines();
    }

    public void EditButtonClick()
    {
        UIController.Singleton.Open(ViewName.ExerciseNNRecord);
        EventPublisher.PublishExerciseEnded(AppManager.Singleton.currentExerciseName);
    }

    IEnumerator Run()
    {
        while(isRunning)
        {
            CountText.text = AppManager.Singleton.PredictionManager.count.ToString();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void EndExerciseButtonClick()
    {
        UIController.Singleton.Open(ViewName.ExerciseSelect);
        EventPublisher.PublishExerciseEnded(AppManager.Singleton.currentExerciseName);
    }

    
}