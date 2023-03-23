using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundScreen : MonoBehaviour
{
    [Range(0, 1)]
    public float speed = 0.02f;
    private Image _image;

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        _image.material = new Material(_image.material);
    }

    // Update is called once per frame
    void Update()
    {
        var x = Time.time * speed;
        var y = -x;
        _image.material.mainTextureOffset = new Vector2(x, y);
    }
}
