using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayMenuScript : MonoBehaviour
{
    public TextMeshProUGUI oponentText;
    public Slider slider;
    public GameObject ChooseColor;
    public GameObject StartButton;
    public GameObject inputFEN;
    string[] oponents = { 
        "Pass & Play",
        "700 ELO",
        "900 ELO",
        "1100 ELO",
        "1200 ELO"
    };
    void Start()
    {
        PlayerPrefs.SetString("Bot", "Pass & Play");
    }
    public void SlideChange()
    {
        oponentText.text = oponents[(int) slider.value];
        PlayerPrefs.SetString("Bot", oponents[(int)slider.value]);
        if((int)slider.value == 0 || inputFEN.transform.GetChild(0).transform.GetChild(2).GetComponent<TextMeshProUGUI>().color == Color.green)
        {
            StartButton.SetActive(true);
            ChooseColor.SetActive(false);
        }
        else
        {
            StartButton.SetActive(false);
            ChooseColor.SetActive(true);
        }
    }
}
