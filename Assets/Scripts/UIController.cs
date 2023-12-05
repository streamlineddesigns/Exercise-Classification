using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Singleton;

    protected void Awake()
    {
        if (Singleton == null) {
            Singleton = this;
            this.gameObject.transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this);
        }
    }

    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void BackButtonClick()
    {
        StartCoroutine(DelayedBackButtonClick());
    }

    IEnumerator DelayedBackButtonClick()
    {
        MoveNetSinglePoseSample[] MoveNetSinglePoseSamples = FindObjectsOfType<MoveNetSinglePoseSample>();

        for (int i = 0; i < MoveNetSinglePoseSamples.Length; i++) {
            MoveNetSinglePoseSamples[i].CleanUp();
        }

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < MoveNetSinglePoseSamples.Length; i++) {
            Destroy(MoveNetSinglePoseSamples[i].gameObject);
        }

        yield return new WaitForSeconds(0.25f);

        SceneManager.LoadScene("Home", LoadSceneMode.Single);
    }
}