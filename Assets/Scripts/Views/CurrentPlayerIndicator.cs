using UnityEngine;
using UnityEngine.UI;

public class CurrentPlayerIndicator : MonoBehaviour
{ 
    public Image image;

    void Update()
    {
        image.enabled = Mathf.RoundToInt(Time.time) % 2 == 0;
    }
}