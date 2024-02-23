using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Slider>().value = gameObject.transform.name == "Slider" ? PlayerPrefs.GetFloat("Volume") : PlayerPrefs.GetFloat("BotDelta");
    }
}
