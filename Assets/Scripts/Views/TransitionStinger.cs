using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class TransitionStinger : MonoBehaviour
{
    private RectTransform _transform;
    private float _startTime;
    [CanBeNull] public Action OnTrasition { get; set; }

    private bool _transitioned;
    
    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<RectTransform>();
        _startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        var time = Time.time - _startTime;

        if (time <= 1.25f)
        {
            var progress = Mathf.Clamp(time / 1.25f, 0, 1);
            progress = Mathf.Pow(progress, 4);
            _transform.anchoredPosition = new Vector3(0, (1 - progress) * 1080, 0);
        } 
        else if (time <= 1.8f)
        {
            if (!_transitioned)
            {
                _transitioned = true;
                OnTrasition?.Invoke();
            }
            
            _transform.anchoredPosition = new Vector3(0, 0, 0);
        }
        else if (time <= 3.05f)
        {
            var progress = Mathf.Clamp((time - 1.8f) / 1.25f, 0, 1);
            progress = Mathf.Pow(progress, 4);
            _transform.anchoredPosition = new Vector3(0, progress * -1080, 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
