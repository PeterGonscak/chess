using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("Volume");
    }
}
