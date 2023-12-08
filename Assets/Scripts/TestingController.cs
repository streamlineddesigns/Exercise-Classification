using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestingController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text CountText;

    [SerializeField]
    private GameObject JButton;

    [SerializeField]
    private GameObject PButton;

    public void JButtonClick()
    {
        EventPublisher.PublishNetworkChange("J");
        JButton.SetActive(false);
        PButton.SetActive(false);
    }

    public void PButtonClick()
    {
        EventPublisher.PublishNetworkChange("P");
        JButton.SetActive(false);
        PButton.SetActive(false);
    }

    public void BackButtonClick()
    {
        UIController.Singleton.BackButtonClick();
    }

    public void SetCountText(int count)
    {
        CountText.text = count.ToString();
    }
}
