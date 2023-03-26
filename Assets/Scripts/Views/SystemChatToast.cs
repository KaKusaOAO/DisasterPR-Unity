using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SystemChatToast : MonoBehaviour
{
    public Image image;
    public TMP_Text text;
    public string content;
    public float alpha = 0.5f;

    private RectTransform _transform;
    private float _startTime;
    private bool _useLerp;
    private bool _needGetOff;

    private static List<SystemChatToast> _toasts = new();
    
    // Start is called before the first frame update
    void Start()
    {
        _toasts.Add(this);
        _startTime = Time.time;
        _transform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var index = _toasts.IndexOf(this);
        var pad = 0f;
        for (var i = 0; i < index; i++)
        {
            pad += _toasts[i]._transform.rect.height + 5;
        }

        var shouldGetOff = _needGetOff || (_toasts.Count > 3 && index == 0);

        var inDuration = shouldGetOff ? 0 : 1f;
        var duration = shouldGetOff ? 0 : 3f;
        var outDuration = shouldGetOff ? 0.33f : 1f;
        var total = inDuration + duration + outDuration;

        if (!_needGetOff && shouldGetOff)
        {
            _needGetOff = true;
            _startTime = Time.time;
        }
        
        var rect = UIManager.Instance.canvas.GetComponent<RectTransform>().rect;
        var x = 0;
        var y = rect.height * 0.15f + pad;
        var lerpSpeed = 0.33f;
        
        if (Time.time - _startTime <= inDuration)
        {
            var progress = Mathf.Clamp((Time.time - _startTime) / inDuration, 0, 1);
            progress = Mathf.Pow(progress, 0.25f);

            var color = image.color;
            color.a = progress * alpha;
            image.color = color;
            
            color = text.color;
            color.a = progress;
            text.color = color;
            
            var start = new Vector2(x, y + 100);
            var end = new Vector2(x, y);
            var target = Vector2.Lerp(start, end, progress);

            if (!_useLerp)
            {
                _transform.anchoredPosition = target;
            }
            else
            {
                _transform.anchoredPosition = Vector2.Lerp(_transform.anchoredPosition, target, lerpSpeed);
            }
        } else if (Time.time - _startTime <= inDuration + duration)
        {
            if (_useLerp)
            {
                var target = new Vector2(x, y);
                _transform.anchoredPosition = Vector2.Lerp(_transform.anchoredPosition, target, lerpSpeed);
            }
        } else if (Time.time - _startTime <= inDuration + duration + outDuration)
        {
            var mark = _startTime + inDuration + duration;
            var progress = Mathf.Clamp((Time.time - mark) / outDuration, 0, 1);
            progress = Mathf.Pow(progress, 3);
            
            var color = image.color;
            color.a = (1 - progress) * alpha;
            image.color = color;

            color = text.color;
            color.a = 1 - progress;
            text.color = color;
            
            var start = new Vector2(x, y);
            var end = new Vector2(x, y - 100);
            var target = Vector2.Lerp(start, end, progress);
            
            if (!_useLerp)
            {
                _transform.anchoredPosition = target;
            }
            else
            {
                _transform.anchoredPosition = Vector2.Lerp(_transform.anchoredPosition, target, lerpSpeed);
            }
        }

        _useLerp = true;
        text.text = content;
        text.enabled = true;
        image.enabled = true;

        if (Time.time - _startTime > total)
        {
            _toasts.Remove(this);
            Destroy(gameObject);
        }
    }
    
    public void SetDefaultStyle()
    {
        image.color = Color.black;
        alpha = 0.5f;
    }

    public void SetErrorStyle()
    {
        image.color = GameConstants.Red;
        alpha = 0.75f;
    }
}