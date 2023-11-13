using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ClickDetection : MonoBehaviour
{
    public GameObject ClickDetect(Camera cam)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(gameObject.GetComponent<RectTransform>(), Camera.main.ScreenToWorldPoint(Input.mousePosition, cam.stereoActiveEye)))
        { 
            return gameObject; 
        }
        else
        {
            return null; 
        }

    }
}
