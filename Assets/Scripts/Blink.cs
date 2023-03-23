using System;
using UnityEngine;
using UnityEngine.UI;

public class Blink : MonoBehaviour
{
    [Range(1, 100)]
    public float smoothInv;
    public float interval;
    public float offset;
    public Image graphic;

    void Update()
    {
        var alpha = Mathf.Sin(Time.time / interval * Mathf.PI) * smoothInv;
        alpha = alpha / 2f + 0.5f;
        alpha += offset;
        
        alpha = Mathf.Clamp(alpha, 0, 1);

        var color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}