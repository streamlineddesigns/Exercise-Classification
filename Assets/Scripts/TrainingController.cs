using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrainingController : MonoBehaviour
{
    [SerializeField] private HeatmapVisual heatmapVisual;

    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

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

    public void BackButtonClick()
    {
        UIController.Singleton.BackButtonClick();
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
        StopRecording();
        AddAdditionalLSTMClassSampling();
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
    
    private void AddMLPInput()
    {
        TrainingDataInput tdi = new TrainingDataInput();
        tdi.input = new float[MoveNetSinglePoseSample.currentPoses.Length];
        Array.Copy(MoveNetSinglePoseSample.currentPoses, tdi.input, MoveNetSinglePoseSample.currentPoses.Length);
        MLPTrainingData.input.Add(tdi);
    }

    private void AddLSTMInput()
    {
        TrainingDataInput tdi = new TrainingDataInput();

        List<float> temp = new List<float>();
        List<float> currentPosesTemp;
        List<float> normalizedPoseDirectionTemp;

        int vectorLength = MoveNetSinglePoseSample.currentPoses.Length + MoveNetSinglePoseSample.normalizedPoseDirection.Length;
        tdi.input = new float[vectorLength];

        currentPosesTemp = MoveNetSinglePoseSample.currentPoses.ToList();
        normalizedPoseDirectionTemp = MoveNetSinglePoseSample.normalizedPoseDirection.ToList();

        temp.AddRange(currentPosesTemp);
        temp.AddRange(normalizedPoseDirectionTemp);

        Array.Copy(temp.ToArray(), tdi.input, vectorLength);
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
                float[] emptyClass = new float[TOTAL_CLASSES]{0.0f,0.0f,1.0f};

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

                int startIndex = i - 2;
                int endIndex = i + 2;

                for (int j = startIndex; j < endIndex + 1; j++) {
                    if (j >= 0 && j < LSTMTrainingData.input.Count) {
                        TrainingDataOutput tempTdo = new TrainingDataOutput();
                        tempTdo.output = new float[TOTAL_CLASSES];
                        tempTdo.index = j;
                        Array.Copy(tdo[0].output, tempTdo.output, TOTAL_CLASSES);
                        int labelIndex = Array.IndexOf(tdo[0].output, 1.0f);
                        if (labelIndex != -1) {
                            tempTdo.output[labelIndex] = (j == i) ? 1.0f : 0.85f;//will do a smooth interpolation in the future, this is just for testing
                            tdos.Add(tempTdo);
                        }
                        
                    }
                }
            }
        }

        LSTMTrainingData.output = tdos;
    }
}
