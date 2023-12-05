using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrainingData
{
    public List<TrainingDataInput> input;
    public List<TrainingDataOutput> output;
}