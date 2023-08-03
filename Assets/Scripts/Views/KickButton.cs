using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KickButton : MonoBehaviour
{
    public PlayerItemEntry root;
    private Button _button;
    private Image _imageBack;
    public Image image;
    private bool _selected;

    private float _lastClickTime;
    
    // Start is called before the first frame update
    void Start()
    {
        _imageBack = GetComponent<Image>();
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;
        
        var session = game.Player!.Session;
        if (session == null) return;
        
        var isHost = session.Players.FirstOrDefault() == game.Player;
        var canUse = root.index != 0 && isHost;
        _imageBack.enabled = canUse;
        image.enabled = canUse;
        _button.interactable = canUse;
    }

    public void OnButtonClicked()
    {
        root.OnKickButtonClicked();
    }
}
