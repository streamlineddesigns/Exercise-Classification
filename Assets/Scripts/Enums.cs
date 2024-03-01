using UnityEngine;

public enum InferenceType {
    NN,
    MLP,
    LSTM,
    CNN
}

public enum ViewType {
    Screen,
    Dialog,
    Popup,
    Component,
}

public enum ViewName {
    Start,
    ExerciseSelect,
    ExerciseCount,
    ExerciseNNRecord,
    ExerciseRealtimeRecord,
    ExerciseVideoRecord,
    ExerciseVideoAnnotate,
    ExerciseSelectButton,
}