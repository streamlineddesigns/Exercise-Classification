using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestingController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text CountText;

    public void BackButtonClick()
    {
        UIController.Singleton.BackButtonClick();
    }

    public void SetCountText(int count)
    {
        CountText.text = count.ToString();
    }
}
