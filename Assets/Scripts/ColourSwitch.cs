using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColourSwitch : MonoBehaviour
{
    public float duration = 1.0f;
    public Color color0;
    public Color color1;
    public Image image;

    public bool activate = false;

    void Update()
    {
        // Ping pong (interpolate) between two colours
        if(activate)
        {
            float t = Mathf.PingPong(Time.time, duration) / duration;
            image.color = Color.Lerp(color0, color1, t);
        }   
    }
}
