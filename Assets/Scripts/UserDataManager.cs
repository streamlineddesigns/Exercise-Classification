using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Singleton;
    public Dictionary<string, UserExerciseData> exercises;
    private string exerciseSaveFilePath;

    protected void Awake()
    {
        if (Singleton == null) {
            Singleton = this;
        } else {
            Destroy(gameObject);
        }
    }

    protected void Start()
    {
        string dir = Application.persistentDataPath;
        exerciseSaveFilePath = (dir + "/UserExerciseData.dat");
    }

    public void Load()
    {
        exercises = null;
        LoadUserData();
    }

    public void Save()
    {
        SaveUserData();
    }

    protected void LoadUserData()
    {
        string dir = Application.persistentDataPath;
        exerciseSaveFilePath = (dir + "/UserExerciseData.dat");
        exercises = DataSaveManager.Deserialize<Dictionary<string, UserExerciseData>>(exerciseSaveFilePath);
    }

    private void SaveUserData()
    {
        DataSaveManager.Serialize<Dictionary<string, UserExerciseData>>(exercises, exerciseSaveFilePath);
    }
}