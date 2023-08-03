using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileWidget : MonoBehaviour
{
    public Image avatarImage;
    public TMP_Text nameText;
    public TMP_Text identifierText;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;
        
        var sprites = AvatarManager.Instance.sprites;
        var sprite = player.Sprite;
        if (sprite == null)
        {
            avatarImage.sprite = sprites[Math.Abs(player.Id.GetHashCode()) % sprites.Length];
        }
        else
        {
            avatarImage.sprite = sprite;
        }
        
        nameText.text = player.Name;
        identifierText.text = player.Identifier;
    }
}
