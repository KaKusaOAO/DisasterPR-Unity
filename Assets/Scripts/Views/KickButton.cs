using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KickButton : MonoBehaviour
{
    public PlayerItemEntry root;
    private Button _button;
    private bool _selected;

    private float _lastClickTime;
    
    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        _button.interactable = root.index != 0;
    }

    public void OnButtonClicked()
    {
        var delta = Time.time - _lastClickTime;
        _lastClickTime = Time.time;
        if (delta > 0.4f) return;
        
        root.OnKickButtonClicked();
    }
}
