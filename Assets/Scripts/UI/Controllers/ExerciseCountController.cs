using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExerciseCountController : Controller
{
    public void EndExerciseButtonClick()
    {
        UIController.Singleton.Open(ViewName.ExerciseSelect);
        EventPublisher.PublishExerciseEnded(AppManager.Singleton.currentExerciseName);
    }
}