using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPublisher 
{
    public delegate void NetworkEvent(string name);
    public static event NetworkEvent OnNetworkChange;

    public delegate void ExerciseEvent(string name);
    public static event ExerciseEvent OnExerciseSelected;
    public static event ExerciseEvent OnExerciseEnded;

    public delegate void UserExerciseDataEvent(string name);
    public static event UserExerciseDataEvent OnUserExerciseDataChanged;

    public static void PublishNetworkChange(string name)
    {
        OnNetworkChange(name);
    }

    public static void PublishExerciseSelected(string name)
    {
        OnExerciseSelected(name);
    }

    public static void PublishExerciseEnded(string name)
    {
        OnExerciseEnded(name);
    }

    public static void PublishUserExerciseDataChanged(string name)
    {
        OnUserExerciseDataChanged(name);
    }
}