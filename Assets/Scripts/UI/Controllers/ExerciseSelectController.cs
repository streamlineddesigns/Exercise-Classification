using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExerciseSelectController : Controller
{
    public GameObject ExerciseSelectButtonPrefab;
    public Transform ButtonParent;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        
        for (int i = 0; i < AppManager.Singleton.ExerciseDataRepository.data.Count; i++) {
            GameObject go = Instantiate(ExerciseSelectButtonPrefab, ButtonParent);
            ExerciseSelectButtonView exerciseSelectButtonView = go.GetComponent<ExerciseSelectButtonView>();
            string currentExerciseDataName = AppManager.Singleton.ExerciseDataRepository.data[i].name;
            exerciseSelectButtonView.SetText(currentExerciseDataName);
            Button exerciseSelectButton = go.GetComponent<Button>();
            exerciseSelectButton.onClick.AddListener(delegate{ExerciseSelectButtonClick(currentExerciseDataName);});
        }
    }

    private void ExerciseSelectButtonClick(string name)
    {
        UIController.Singleton.Open(ViewName.ExerciseCount);
        EventPublisher.PublishExerciseSelected(name);
    }
}