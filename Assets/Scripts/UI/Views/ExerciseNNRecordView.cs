using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExerciseNNRecordView : View
{
    protected void OnEnable()
    {
        ExerciseNNRecordController ExerciseNNRecordController = AppManager.Singleton.ControllerRegistry.getController(ViewName.ExerciseNNRecord) as ExerciseNNRecordController;
        ExerciseNNRecordController.RecordUserExerciseData();
    }
}
    