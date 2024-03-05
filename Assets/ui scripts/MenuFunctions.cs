using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuFunctions : MonoBehaviour
{
    public GameObject inputFEN;
    public FlexibleColorPicker colorPicker;
    public GameObject colorPickerObj;
    public GameObject ChooseColor;
    public GameObject StartButton;
    public Slider slider;
    public Slider botSlider;
    public Toggle toggle;
    public TextMeshProUGUI deltaText;

    [HideInInspector]
    public int colorChange = 0;

    public void LoadPlay()
    {
        SceneManager.LoadScene("PlayMenu");
    }
    public void LoadWatch()
    {
        SceneManager.LoadScene("WatchMenu");
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
        PlayerPrefs.SetString("MoveSquares", "rgba(0.8588235, 0.9529411, 0.317647, 0.5)");
    }
    public void VolumeChange()
    {
        PlayerPrefs.SetFloat("Volume", slider.value);
    }
    public void DeltaChange()
    {
        deltaText.text = botSlider.value.ToString().Substring(0,4);
        PlayerPrefs.SetFloat("BotDelta", botSlider.value);
    }
    public void RandomiseBotOutput()
    {
        PlayerPrefs.SetInt("BotRandom", toggle.isOn ? 1 : 0);
    }
    public void ActivateColorPicker(int colorToSet)
    {
        colorChange = colorToSet;
        string prefColor = PlayerPrefs.GetString(colorToSet == 0 ? "LightSquares" : (colorToSet == 1 ? "DarkSquares" : "MoveSquares"),
            colorToSet == 0 ? "rgba(0.9339623, 0.8006562, 0.6746681, 1)" 
            :(colorToSet == 1 ? "rgba(0.4339623, 0.2914913, 0.108783, 1)" 
                                : "rgba(0.8588235, 0.9529411, 0.317647, 0.5)"));
        string[] rgba = prefColor.Substring(5, prefColor.Length - 6).Split(", ");
        colorPicker.color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
        if(colorPickerObj.activeInHierarchy && colorToSet == colorChange)
        {
            colorPickerObj.SetActive(false);
        }
        else
        {
            colorPickerObj.SetActive(true);
        }
        OnChangeColorPicker();
    }

    public void PlayButtonSound()
    {
        AudioSource audioSource = GameObject.Find("Audio Source").GetComponent<AudioSource>();
        audioSource.volume = PlayerPrefs.GetFloat("Volume", 0.5f);
        audioSource.Play();
    }
    
    public void OnChangeColorPicker()
    {
        PlayerPrefs.SetString(colorChange == 0 ? "LightSquares" : (colorChange == 1 ? "DarkSquares" : "MoveSquares"), colorPicker.color.ToString());
    }
    public void StartGame(string startingColor)
    {
        PlayerPrefs.SetString("Mode", "Player");
        PlayerPrefs.SetString("PieceColor", startingColor);
        TextMeshProUGUI text = inputFEN.transform.GetChild(0).transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        if (text.text.Length < 20) PlayerPrefs.DeleteKey("FEN");
        SceneManager.LoadScene("Game");
    }
    public void StartSpectatorGame()
    {
        PlayerPrefs.SetString("Bot", "null");
        PlayerPrefs.SetString("Mode", "Spectator");
        PlayerPrefs.SetString("PieceColor", "W");
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
