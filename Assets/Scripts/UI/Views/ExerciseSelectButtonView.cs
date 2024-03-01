using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExerciseSelectButtonView : View
{
    [SerializeField] private string exerciseName;
    [SerializeField] private TMP_Text exerciseNameText;

    public void SetText(string name)
    {
        exerciseName = name;
        exerciseNameText.text = name;
    }
}
    