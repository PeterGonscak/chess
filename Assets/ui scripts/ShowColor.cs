using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowColor : MonoBehaviour
{
    public MainMenu mainMenuScript;
    private void Update()
    {
        ChangedColor();
    }
    public void ChangedColor()
    {
        string prefColor = PlayerPrefs.GetString(gameObject.name);
        string[] rgba = prefColor.Substring(5, prefColor.Length - 6).Split(", ");
        Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
        gameObject.GetComponent<Image>().color = color;
    }
}
