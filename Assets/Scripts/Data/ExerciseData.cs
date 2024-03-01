using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

namespace Data {

    [System.Serializable]
    public class ExerciseData
    {
        public string name;
        public NNModel MLPModel;
        public NNModel LSTMModel;
        public NNModel CNNModel;
        public float[] startPosition;
        public float[] endPosition;
    }

}