using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingController : MonoBehaviour
{
    [SerializeField] private HeatmapVisual heatmapVisual;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    [SerializeField] private ExerciseDataRepository ExerciseDataRepository;

    [SerializeField] private CNNEInferenceController CNNEInferenceController;

    public GameObject ExerciseSelectView;
    public GameObject ExerciseSelectButtonPrefab;
    public Transform ButtonParent;
    private string currentExerciseName;

    [SerializeField]
    private TrainingData MLPTrainingData;

    [SerializeField]
    private TrainingData LSTMTrainingData;

    [SerializeField]
    private TrainingData CNNTrainingData;

    [SerializeField]
    private TMP_Text CountdownText;

    [SerializeField]
    private TMP_Text CountText;

    [SerializeField]
    private GameObject SaveButton;

    [SerializeField]
    private GameObject DeleteButton;

    private const int TOTAL_CLASSES = 3;

    private bool isRecording;

    private bool isCountingDown;

    void Start()
    {        
        for (int i = 0; i < ExerciseDataRepository.data.Count; i++) {
            GameObject go = Instantiate(ExerciseSelectButtonPrefab, ButtonParent);
            ExerciseSelectButtonView exerciseSelectButtonView = go.GetComponent<ExerciseSelectButtonView>();
            string currentExerciseDataName = ExerciseDataRepository.data[i].name;
            exerciseSelectButtonView.SetText(currentExerciseDataName);
            Button exerciseSelectButton = go.GetComponent<Button>();
            exerciseSelectButton.onClick.AddListener(delegate{ExerciseSelectButtonClick(currentExerciseDataName);});
        }
    }

    private void ExerciseSelectButtonClick(string name)
    {
        currentExerciseName = name;
        ExerciseSelectView.SetActive(false);
        EventPublisher.PublishExerciseSelected(name);
        Debug.Log(currentExerciseName);
    }

    public void BackButtonClick()
    {
        //UIController.Singleton.BackButtonClick();
    }

    public void RecordButtonClick()
    {
        if (! isRecording) {
            isRecording = true;
            StartCoroutine(Countdown());
            StartCoroutine(ListenForContinuousInput());
        }
    }

    public void SaveButtonClick()
    {
        ExerciseSelectView.SetActive(true);
        StopRecording();
        AddAdditionalLSTMClassSampling();
        SaveStartEndPositions();
        SerializeTrainingData();
    }

    public void DeleteButtonClick()
    {
        StopRecording();
    }

    private void StopRecording()
    {
        isRecording = false;
        StopAllCoroutines();
    }

    private void Update()
    {
        if (! isRecording) {
            return;
        }

        int classIndex = 2;
        float value = 1.0f;
        
        if (Input.GetKeyDown(KeyCode.S) || Input.GetButtonDown("joystick button 0")) {
            classIndex = 0;
            value = 1.0f;
            AddDiscreteInput();
            AddOutput(classIndex, value);
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("joystick button 3")) {
            classIndex = 1;
            value = 1.0f;
            AddDiscreteInput();
            AddOutput(classIndex, value);
            CountText.text = (MLPTrainingData.input.Count / 2).ToString();
        }

        if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("joystick button 2")) {
            classIndex = 2;
            value = 1.0f;
            AddDiscreteInput();
            AddOutput(classIndex, value);
        }
    }

    IEnumerator ListenForContinuousInput()
    {
        yield return new WaitUntil(() => ! isCountingDown);

        while (isRecording) {
            AddContinuousInput();
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Countdown()
    {
        isCountingDown = true;

        CountdownText.gameObject.SetActive(true);

        int currentTime = 10;
        while (currentTime > 0) {
            CountdownText.text = (currentTime).ToString();
            yield return new WaitForSeconds(1.0f);
            currentTime--;
        }

        CountdownText.gameObject.SetActive(false);
        CountText.gameObject.SetActive(true);
        SaveButton.SetActive(true);
        DeleteButton.SetActive(true);

        isCountingDown = false;
    }

    private void AddDiscreteInput()
    {
        AddMLPInput();
        AddCNNInput();
    }

    private void AddContinuousInput()
    {
        AddLSTMInput();
    }
    
    /*private void AddMLPInput()
    {
        TrainingDataInput tdi = new TrainingDataInput();
        tdi.input = new float[MoveNetSinglePoseSample.currentPoses.Length];
        Array.Copy(MoveNetSinglePoseSample.currentPoses, tdi.input, MoveNetSinglePoseSample.currentPoses.Length);
        MLPTrainingData.input.Add(tdi);
    }*/

    public void AddMLPInput()
    {
        TrainingDataInput tdi = new TrainingDataInput();
        //float[] weights = ExerciseDataRepository.data.Where(x => x.name == currentExerciseName).First().weights;
        //List<float> weightedPoses = VectorUtils.GetWeightedVector(MoveNetSinglePoseSample.currentPoses, weights).ToList();
        //List<float> weightedPosesDirection = VectorUtils.GetWeightedVector(MoveNetSinglePoseSample.normalizedPoseDirection, weights).ToList();
        List<Vector3> currentPoseDirectionVectorsTemp = VectorUtils.GetDirectionVectors(MoveNetSinglePoseSample.resampledPoses.ToList());
        List<float> currentPoseDirectionVectors = new List<float>();
        for (int i = 0; i < currentPoseDirectionVectorsTemp.Count; i++) {
            currentPoseDirectionVectors.Add(currentPoseDirectionVectorsTemp[i].x);
            currentPoseDirectionVectors.Add(currentPoseDirectionVectorsTemp[i].y);
        }
        //List<float> weightedPoseDirectionVectors = VectorUtils.GetWeightedVector(currentPoseDirectionVectors.ToArray(), weights).ToList();

        int vectorLength = MoveNetSinglePoseSample.currentPoses.Length + MoveNetSinglePoseSample.normalizedPoseDirection.Length + currentPoseDirectionVectors.Count;
        tdi.input = new float[vectorLength];

        List<float> temp = new List<float>();
        temp.AddRange(MoveNetSinglePoseSample.currentPoses);
        temp.AddRange(MoveNetSinglePoseSample.normalizedPoseDirection);
        temp.AddRange(currentPoseDirectionVectors.ToArray());
        //temp.AddRange(CNNEInferenceController.encodedImageRepresentation.ToList());

        Array.Copy(temp.ToArray(), tdi.input, vectorLength);
        MLPTrainingData.input.Add(tdi);
    }

    /*private void AddLSTMInput()
    {
        TrainingDataInput tdi = new TrainingDataInput();

        

        //List<float> weightedPosesTemp = VectorUtils.GetWeightedVector(MoveNetSinglePoseSample.currentPoses, ExerciseDataRepository.data.Where(x => x.name == currentExerciseName).First().weights).ToList();
        //List<float> weightedPosesDirectionTemp = VectorUtils.GetWeightedVector(MoveNetSinglePoseSample.normalizedPoseDirection, ExerciseDataRepository.data.Where(x => x.name == currentExerciseName).First().weights).ToList();
        List<Vector3> currentPosesTemp = VectorUtils.GetDirectionVectors(MoveNetSinglePoseSample.resampledPoses.ToList());
        List<float> currentPosesTempFloat = new List<float>();
        for (int i = 0; i < currentPosesTemp.Count; i++) {
            currentPosesTempFloat.Add(currentPosesTemp[i].x);
            currentPosesTempFloat.Add(currentPosesTemp[i].y);
        }

        int vectorLength = MoveNetSinglePoseSample.currentPoses.Length + MoveNetSinglePoseSample.normalizedPoseDirection.Length + currentPosesTempFloat.Count + CNNEInferenceController.encodedImageRepresentation.Length;
        tdi.input = new float[vectorLength];

        List<float> temp = new List<float>();
        temp.AddRange(MoveNetSinglePoseSample.currentPoses);
        temp.AddRange(MoveNetSinglePoseSample.normalizedPoseDirection);
        temp.AddRange(currentPosesTempFloat);
        temp.AddRange(CNNEInferenceController.encodedImageRepresentation.ToList());

        Array.Copy(temp.ToArray(), tdi.input, vectorLength);
        LSTMTrainingData.input.Add(tdi);
    }*/

    /*private void AddLSTMInput()
    {
        TrainingDataInput tdi = new TrainingDataInput();

        List<float> temp = new List<float>();
        List<float> currentPosesTemp;
        List<float> normalizedPoseDirectionTemp;

        int vectorLength = MoveNetSinglePoseSample.currentPoses.Length + CNNEInferenceController.encodedImageRepresentation.Length;
        tdi.input = new float[vectorLength];

        currentPosesTemp = MoveNetSinglePoseSample.currentPoses.ToList();
        normalizedPoseDirectionTemp = MoveNetSinglePoseSample.normalizedPoseDirection.ToList();

        temp.AddRange(currentPosesTemp);
        //temp.AddRange(normalizedPoseDirectionTemp);
        temp.AddRange(CNNEInferenceController.encodedImageRepresentation.ToList());

        Array.Copy(temp.ToArray(), tdi.input, vectorLength);
        LSTMTrainingData.input.Add(tdi);
    }*/

    /*private void AddLSTMInput()
    {
        TrainingDataInput tdi = new TrainingDataInput();

        List<Vector3> currentPosesTemp = VectorUtils.GetDirectionVectors(MoveNetSinglePoseSample.resampledPoses.ToList());
        List<float> currentPosesTempFloat = new List<float>();

        for (int i = 0; i < currentPosesTemp.Count; i++) {
            currentPosesTempFloat.Add(currentPosesTemp[i].x);
            currentPosesTempFloat.Add(currentPosesTemp[i].y);
        }

        List<float> temp = new List<float>();
        temp.AddRange(currentPosesTempFloat.ToArray());
        //temp.Add(ExerciseDataRepository.data.Where(x => x.name == currentExerciseName).First().weights);
        temp.AddRange(MoveNetSinglePoseSample.normalizedPoseDirection);

        //float[] weightedPoses = VectorUtils.GetSummation(temp.ToArray()).ToArray();
        //float[] weightedPoses = VectorUtils.GetWeightedVector(currentPosesTempFloat.ToArray(), ExerciseDataRepository.data.Where(x => x.name == currentExerciseName).First().weights);

        tdi.input = new float[temp.Count];
        Array.Copy(temp.ToArray(), tdi.input, temp.Count);
        LSTMTrainingData.input.Add(tdi);
    }*/

    private void AddLSTMInput()
    {
        heatmapVisual.SetHeatMapFlattened(MoveNetSinglePoseSample.heatmap);
        
        float[] inputs = CNNEInferenceController.encodedImageRepresentation;
        TrainingDataInput tdi = new TrainingDataInput();
        tdi.input = new float[inputs.Length];
        Array.Copy(inputs, tdi.input, inputs.Length);
        LSTMTrainingData.input.Add(tdi);
    }

    private void AddCNNInput()
    {
        float[] imageRepresentation = MoveNetSinglePoseSample.heatmap.GetFlattenedHeatmap();
        heatmapVisual.SetHeatMapFlattened(MoveNetSinglePoseSample.heatmap);

        TrainingDataInput tdi = new TrainingDataInput();
        tdi.input = new float[imageRepresentation.Length];
        Array.Copy(imageRepresentation, tdi.input, imageRepresentation.Length);
        CNNTrainingData.input.Add(tdi);
    }

    private void AddOutput(int classIndex, float value)
    {
        AddOutputByType(classIndex, value, ref MLPTrainingData);
        AddOutputByType(classIndex, value, ref LSTMTrainingData);
        AddOutputByType(classIndex, value, ref CNNTrainingData);
    }

    private void AddOutputByType(int classIndex, float value, ref TrainingData trainingData)
    {
        float[] oneHotVector = new float[TOTAL_CLASSES]{0.0f, 0.0f, 0.0f};
        oneHotVector[classIndex] = value * 1.0f;
        TrainingDataOutput tdo = new TrainingDataOutput();
        tdo.output = new float[TOTAL_CLASSES];
        tdo.index = trainingData.input.Count - 1;
        Array.Copy(oneHotVector, tdo.output, TOTAL_CLASSES);
        trainingData.output.Add(tdo);
    }

    private void SaveStartEndPositions()
    {        
        List<float[]> starts = new List<float[]>();
        List<float[]> ends = new List<float[]>();

        List<int> inputIndexs = new List<int>();

        for (int i = 0; i < MLPTrainingData.input.Count; i++) {
            TrainingDataInput tdi = MLPTrainingData.input[i];
            TrainingDataOutput[] tdoList = MLPTrainingData.output.Where(x => x.index == i).ToArray();

            if (tdoList.Length > 0) {
                TrainingDataOutput tdo = tdoList[0];
                if (tdo.output[0] == 1.0f) {
                    starts.Add(tdi.input);
                } else if (tdo.output[1] == 1.0f) {
                    ends.Add(tdi.input);
                }
            }
        }
        
        float[] startPosition = VectorUtils.GetCentroid(starts.ToArray());
        float[] endPosition = VectorUtils.GetCentroid(ends.ToArray());

        int exerciseIndex = ExerciseDataRepository.data.FindIndex(x => x.name == currentExerciseName);

        ExerciseDataRepository.data[exerciseIndex].startPosition = startPosition;
        ExerciseDataRepository.data[exerciseIndex].endPosition = endPosition;
    }

    private void SerializeTrainingData()
    {
        SerializeTrainingDataByType(InferenceType.MLP, ref MLPTrainingData);
        SerializeTrainingDataByType(InferenceType.LSTM, ref LSTMTrainingData);
        SerializeTrainingDataByType(InferenceType.CNN, ref CNNTrainingData);
    }

    private void SerializeTrainingDataByType(InferenceType inferenceType, ref TrainingData trainingData)
    {
        string saveFileName = "";

        switch(inferenceType) {
            case InferenceType.MLP :
                saveFileName = "MoveMLPTrainingData.csv";
                break;
            case InferenceType.LSTM :
                saveFileName = "MoveLSTMTrainingData.csv";
                break;
            case InferenceType.CNN :
                saveFileName = "MoveCNNTrainingData.csv";
                break;
        }

        string saveFilePath = Application.persistentDataPath + "/" + saveFileName;

        using (StreamWriter writer = new StreamWriter(saveFilePath))  
        {
            for (int i = 0; i < trainingData.input.Count; i++) {
                string writeString = "";

                //comma seperated input values
                for (int j = 0; j < trainingData.input[i].input.Length; j++) {
                    writeString += trainingData.input[i].input[j] + ",";
                }

                //empty class
                float[] emptyClass = new float[TOTAL_CLASSES]{0.0f,0.0f,0.5f};

                //comma seperated output values
                TrainingDataOutput[] tdo = trainingData.output.Where(x => x.index == i).ToArray();
                float[] outputs = (tdo.Length == 0) ? emptyClass : tdo[0].output;

                for (int k = 0; k < outputs.Length; k++) {
                    string endString = (k == outputs.Length - 1) ? "" : ",";
                    writeString += outputs[k] + endString;
                }
                    
                writer.WriteLine(writeString);
            }
        }

        Debug.Log("Save file path: " + saveFilePath);
    }

    private void AddAdditionalLSTMClassSampling()
    {
        List<TrainingDataOutput> tdos = new List<TrainingDataOutput>();

        for (int i = 0; i < LSTMTrainingData.input.Count; i++) {
            TrainingDataOutput[] tdo = LSTMTrainingData.output.Where(x => x.index == i).ToArray();

            if (tdo.Length != 0) {

                int startIndex = i - 1;
                int endIndex = i + 1;

                for (int j = startIndex; j < endIndex + 1; j++) {
                    if (j >= 0 && j < LSTMTrainingData.input.Count) {
                        int distance = Mathf.Abs(i - j);
                        float multiplier = distance * 0.1f;

                        TrainingDataOutput tempTdo = new TrainingDataOutput();
                        tempTdo.output = new float[TOTAL_CLASSES];
                        tempTdo.index = j;
                        Array.Copy(tdo[0].output, tempTdo.output, TOTAL_CLASSES);
                        tdos.Add(tempTdo);
                        int labelIndex = Array.IndexOf(tdo[0].output, 1.0f);
                        if (labelIndex != -1) {
                            //tempTdo.output[labelIndex] = (j == i) ? 1.0f : 1.0f - multiplier;//will do a smooth interpolation in the future, this is just for testing
                        }

                        float val = tempTdo.output[labelIndex];
                        float otherVal = 1.0f - val;
                        int otherLabelIndex = (labelIndex == 0) ? 1 : 0;
                        //tempTdo.output[otherLabelIndex] = otherVal;

                        tdos.Add(tempTdo);
                    }
                }
            }
        }

        LSTMTrainingData.output = tdos;
    }
}
