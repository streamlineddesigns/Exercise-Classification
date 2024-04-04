using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public static AppManager Singleton;
    public string currentExerciseName;
    public PredictionManager PredictionManager;
    public ExerciseDataRepository ExerciseDataRepository;
    public ControllerRegistry ControllerRegistry;
    public CNNAEInferenceController CNNAEInferenceController;
    public CNNEInferenceController CNNEInferenceController;

    public void Awake()
    {
        if (Singleton == null) {
            Singleton = this;
        } else {
            Destroy(gameObject);
        }
    }

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
        currentExerciseName = name;
    }
}