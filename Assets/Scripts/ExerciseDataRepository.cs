using UnityEngine;
using System.Collections.Generic;
using Data;

[CreateAssetMenu(fileName = "ExerciseDataRepository", menuName = "Repositories/ExerciseDataRepository")]
public class ExerciseDataRepository : ScriptableObject {
    public List<ExerciseData> data = new List<ExerciseData>();
}