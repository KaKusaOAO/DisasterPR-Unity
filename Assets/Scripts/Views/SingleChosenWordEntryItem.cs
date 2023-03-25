using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisasterPR.Net.Packets.Play;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Screen = UnityEngine.Device.Screen;

[ExecuteAlways]
public class SingleChosenWordEntryItem : MonoBehaviour
{
    public Image image;
    public Sprite normalSprite;
    public Sprite selectedSprite;
    public WordCardHolder holder;
    public bool revealed;
    public bool selected;

    public GameObject hiddenPart;
    public GameObject revealedPart;

    public TMP_Text text;

    public bool IsEnabled
    {
        get => enabled;
        set => enabled = value;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        image.sprite = selected ? selectedSprite : normalSprite;
        hiddenPart.SetActive(!revealed);
        revealedPart.SetActive(revealed);
        if (holder == null) return;
        
        var session = GameManager.Instance.Session;
        if (session == null) return;

        var index = holder.index;
        var words = session.LocalGameState.CurrentChosenWords[index];
        revealed = words.IsRevealed;
        selected = ScreenManager.Instance.gameScreen.LocalChosenFinalIndex == index;

        text.text = words.Words[0].Label;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}