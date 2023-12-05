using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeController : MonoBehaviour
{
    public void TrainButtonClick()
    {
        SceneManager.LoadScene("Training");
    }
    
    public void TestButtonClick()
    {
        SceneManager.LoadScene("Testing");
    }
}
