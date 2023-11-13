using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject inputFEN;
    public FlexibleColorPicker colorPicker;
    public GameObject colorPickerObj;
    public GameObject ChooseColor;
    public GameObject StartButton;
    public Slider slider;

    [HideInInspector]
    public bool whiteChange = true;

    public void LoadPlay()
    {
        SceneManager.LoadScene("PlayMenu");
    }
    public void LoadSettings()
    {
        SceneManager.LoadScene("Settings");
    }
    public void ShutDown()
    {
        Application.Quit();
    }
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void SetColorToDefault()
    {
        PlayerPrefs.SetString("LightSquares", "rgba(0.9339623, 0.8006562, 0.6746681, 1)");
        PlayerPrefs.SetString("DarkSquares", "rgba(0.4339623, 0.2914913, 0.108783, 1)");
    }
    public void VolumeChange()
    {
        PlayerPrefs.SetFloat("Volume", slider.value);
    }
    public void ActivateColorPicker(bool white)
    {
        if(colorPickerObj.activeInHierarchy && white == whiteChange)
        {
            colorPickerObj.SetActive(false);
        }
        else
        {
            colorPickerObj.SetActive(true);
        }
        whiteChange = white;
        string prefColor = PlayerPrefs.GetString(white ? "LightSquares" : "DarkSquares");
        string[] rgba = prefColor.Substring(5, prefColor.Length - 6).Split(", ");
        colorPicker.color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
        OnChangeColorPicker();
    }
    public void OnChangeColorPicker()
    {
        PlayerPrefs.SetString(whiteChange ? "LightSquares" : "DarkSquares", colorPicker.color.ToString());
    }
    public void StartGame(string startingColor)
    {
        PlayerPrefs.SetString("PieceColor", startingColor);
        TextMeshProUGUI text = inputFEN.transform.GetChild(0).transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        if (text.text.Length < 20) PlayerPrefs.DeleteKey("FEN");
        SceneManager.LoadScene("Game");
    }
    public void FENcheck()
    {
        TextMeshProUGUI text = inputFEN.transform.GetChild(0).transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        string[] divFEN = text.text.Split(" ");
        if (divFEN.Length == 6)
        {
            bool correct = (divFEN[1] == "w" || divFEN[1] == "b");
            string KQkq = "KQkq";
            if (divFEN[2] != "-")
                for (int i = 0; i < divFEN[2].Length; i++)
                {
                    if (KQkq.Contains(divFEN[2][i]))
                        KQkq.Remove(KQkq.IndexOf(divFEN[2][i]), 1);
                    else
                    {
                        correct = false;
                        break;
                    }
                }
            correct = correct && divFEN[3] == "-"
                    || (divFEN[3].Length == 2
                        && "abcdefgh".Contains(divFEN[3][0])
                        && "12345678".Contains(divFEN[3][1]));
            if (correct)
            {
                PlayerPrefs.SetString("FEN", text.text);
                text.color = Color.green;
                ChooseColor.SetActive(false);
                StartButton.SetActive(true);
                return; 
            }
        }
        ChooseColor.SetActive(true);
        StartButton.SetActive(false);
        PlayerPrefs.SetString("FEN", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0");
        text.color = Color.red;
    }
}
