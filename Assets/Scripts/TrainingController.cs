using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrainingController : MonoBehaviour
{
    [SerializeField]
    private MoveNetSinglePoseSample MoveNetSinglePoseSample;

    [SerializeField]
    private TrainingData TrainingData;

    [SerializeField]
    private TMP_Text CountdownText;

    [SerializeField]
    private TMP_Text CountText;

    [SerializeField]
    private Animator Animator;

    [SerializeField]
    private GameObject SaveButton;

    [SerializeField]
    private GameObject DeleteButton;

    private const int TOTAL_CLASSES = 3;

    private bool isRecording;

    private Vector2 anchorPoint = new Vector2(0.5f, 0.1f);


    public void BackButtonClick()
    {
        UIController.Singleton.BackButtonClick();
    }

    public void RecordButtonClick()
    {
        if (! isRecording) {
            StartRecording();
        }
    }

    public void SaveButtonClick()
    {
        StopRecording();
        AddAdditionalClassSampling();
        SerializeTrainingData();
        //UIController.Singleton.BackButtonClick();
    }

    public void DeleteButtonClick()
    {
        StopRecording();
        //UIController.Singleton.BackButtonClick();
    }

    public void AddOutput(int classIndex, float value)
    {
        if (isRecording) {
            float[] oneHotVector = new float[TOTAL_CLASSES]{0.0f, 0.0f, 0.0f};
            oneHotVector[classIndex] = value * 1.0f;

            TrainingDataOutput tdo = new TrainingDataOutput();
            tdo.index = TrainingData.input.Count - 1;
            tdo.output = new float[TOTAL_CLASSES];
            Array.Copy(oneHotVector, tdo.output, TOTAL_CLASSES);
            TrainingData.output.Add(tdo);
        }
    }

    private void StartRecording()
    {
        isRecording = true;
        StartCoroutine(Record());
    }

    private void StopRecording()
    {
        isRecording = false;
        StopAllCoroutines();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) {
            AddOutput(0, 1.0f);
        }

        if (Input.GetKeyDown(KeyCode.U)) {
            AddOutput(1, 1.0f);
        }
    }

    IEnumerator Record()
    {
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

        Animator.SetTrigger("Record");

        while(isRecording) {
            List<float> poses = new List<float>();

            // sp + dv = fp
            //-sp       -sp
            //      dv = fp -sp
            Vector2 anchorOffset = new Vector2(MoveNetSinglePoseSample.poses[0].x - anchorPoint.x, MoveNetSinglePoseSample.poses[0].y - anchorPoint.y);

            for (int i = 0; i < MoveNetSinglePoseSample.poses.Count; i++) {
                //sp + dv = fp
                //   - dv  -dv
                //     sp = fp - dv
                poses.Add(MoveNetSinglePoseSample.poses[i].x - anchorOffset.x);
                poses.Add(MoveNetSinglePoseSample.poses[i].y - anchorOffset.y);
                //poses.Add(MoveNetSinglePoseSample.poses[i].z);
            }

            TrainingDataInput tdi = new TrainingDataInput();
            tdi.input = poses.ToArray();
            TrainingData.input.Add(tdi);

            CountText.text = TrainingData.output.Count.ToString();

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void AddAdditionalClassSampling()
    {
        List<TrainingDataOutput> tdos = new List<TrainingDataOutput>();

        for (int i = 0; i < TrainingData.input.Count; i++) {
            TrainingDataOutput[] tdo = TrainingData.output.Where(x => x.index == i).ToArray();

            if (tdo.Length != 0) {

                int startIndex = i - 1;
                int endIndex = i + 2;

                for (int j = startIndex; j < endIndex; j++) {
                    if (j != i) {
                        TrainingDataOutput tempTdo = new TrainingDataOutput();
                        tempTdo.output = new float[TOTAL_CLASSES];
                        tempTdo.index = j;
                        Array.Copy(tdo[0].output, tempTdo.output, TOTAL_CLASSES);
                        tdos.Add(tempTdo);
                    }
                }
            }
        }

        TrainingData.output.AddRange(tdos);
    }

    private void SerializeTrainingData()
    {
        string saveFilePath = Application.persistentDataPath + "/MoveTrainingData.txt";

        using (StreamWriter writer = new StreamWriter(saveFilePath))  
        {
            for (int i = 0; i < TrainingData.input.Count; i++) {
                string writeString = "";

                //comma seperated input values
                for (int j = 0; j < TrainingData.input[i].input.Length; j++) {
                    writeString += TrainingData.input[i].input[j] + ",";
                }

                //empty class
                float[] emptyClass = new float[TOTAL_CLASSES]{0.0f,0.0f,1.0f};

                //comma seperated output values
                TrainingDataOutput[] tdo = TrainingData.output.Where(x => x.index == i).ToArray();
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
}
