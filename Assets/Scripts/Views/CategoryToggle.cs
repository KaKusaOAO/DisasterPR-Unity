using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisasterPR.Cards;
using DisasterPR.Net.Packets.Play;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryToggle : MonoBehaviour
{
    public int index;
    public Sprite onSprite;
    public Sprite offSprite;
    public bool categoryEnabled;
    public TMP_Text text;
    
    private Image _image;

    void Start()
    {
        _image = GetComponent<Image>();
    }

    void Update()
    {
        var manager = GameManager.Instance;
        var options = manager.Session.Options;
        var cardPack = manager.Session.CardPack;
        var category = cardPack.Categories[index];

        categoryEnabled = options.EnabledCategories.Contains(category);
        _image.sprite = categoryEnabled ? onSprite : offSprite;

        text.text = category.Label;
    }

    public void OnToggle()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
        
        
        var manager = GameManager.Instance;
        if (manager.Player != manager.Session!.HostPlayer) return;
            
        var options = manager.Session.Options;
        var cardPack = manager.Session.CardPack;
        var category = cardPack.Categories[index];

        var list = new List<CardCategory>(options.EnabledCategories);
        var enabled = !categoryEnabled;
        if (enabled)
        {
            list.Add(category);
        }
        else
        {
            list.Remove(category);
        }

        if (!list.Any())
        {
            list.Add(category);
        }

        manager.Connection!.SendPacket(new ServerboundUpdateSessionOptionsPacket(
            options.WinScore, options.CountdownTimeSet, list));
    }
}