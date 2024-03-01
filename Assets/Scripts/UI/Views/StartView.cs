using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartView : View
{
    protected void OnEnable()
    {
        UIController.Singleton.Open(ViewName.ExerciseSelect);
    }
}
    