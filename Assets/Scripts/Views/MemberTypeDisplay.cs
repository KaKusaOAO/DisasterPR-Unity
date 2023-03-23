using System;
using UnityEngine;
using UnityEngine.UI;

public class MemberTypeDisplay : MonoBehaviour
{
    public Sprite hostSprite;
    public Sprite nonHostSprite;

    private Image _image;

    void Start()
    {
        _image = GetComponent<Image>();
    }

    void Update()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;
        
        var session = game.Player!.Session;
        if (session == null) return;

        _image.sprite = session.HostPlayer == game.Player ? hostSprite : nonHostSprite;
    }
}