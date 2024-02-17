using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WatchMenu : MonoBehaviour
{

    public TextMeshProUGUI whiteText;
    public Slider whiteSlider;
    public TextMeshProUGUI blackText;
    public Slider blackSlider;
    public GameObject inputFEN;

    string[] oponents = {
        "200 ELO",
        "400 ELO",
        "600 ELO",
        "800 ELO",
        "1000 ELO",
        "1200 ELO",
        "1400 ELO",
        "1600 ELO",
        "1800 ELO",
        "2000 ELO"
    };

    public void SlideChange(bool white)
    {
        if (white)
        {
            whiteText.text = "White: " + oponents[(int)whiteSlider.value];
            PlayerPrefs.SetString("whiteOpponent", oponents[(int)whiteSlider.value]);
        }
        else
        {
            blackText.text = "Black: " + oponents[(int)blackSlider.value];
            PlayerPrefs.SetString("blackOpponent", oponents[(int)blackSlider.value]);
        }
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
                return;
            }
        }
        PlayerPrefs.SetString("FEN", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0");
        text.color = Color.red;
    }
}

