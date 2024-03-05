using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Slider>().value = gameObject.transform.name == "Slider" ? PlayerPrefs.GetFloat("Volume", 0.5f) : PlayerPrefs.GetFloat("BotDelta", 0.5f);
    }
}
