using System;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class CreditParticle : MonoBehaviour
{
    private RectTransform _transform;
    private Vector2 _velocity;
    private float _startTime;
    private Image _image;
    
    // Start is called before the first frame update
    void Start()
    {
        var random = new Random();
        var r = (float) random.NextDouble() * Mathf.PI * 2;
        _velocity = new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * Mathf.Lerp(5, 10, (float) random.NextDouble());
        _startTime = Time.time;
        _image = GetComponent<Image>();
        _transform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _velocity *= 0.9f;
        _velocity += Vector2.down * 0.1f;
        _transform.anchoredPosition += _velocity;
    }

    void Update()
    {
        var delta = Time.time - _startTime;
        if (delta >= 2)
        {
            Destroy(gameObject);
            return;
        }

        if (_image == null) return;
        var alpha = 1 - delta / 2f;
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
    }
}