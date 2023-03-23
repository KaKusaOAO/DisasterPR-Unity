using System;
using System.Linq;
using System.Threading.Tasks;
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
        
        _ = Task.Run(async () =>
        {
            var manager = GameManager.Instance;
            if (manager.Player != manager.Session.HostPlayer) return;
            
            var options = manager.Session.Options;
            var cardPack = manager.Session.CardPack;
            var category = cardPack.Categories[index];
            
            var enabled = !categoryEnabled;
            if (enabled)
            {
                options.EnabledCategories.Add(category);
            }
            else
            {
                options.EnabledCategories.Remove(category);
            }

            if (!options.EnabledCategories.Any())
            {
                options.EnabledCategories.Add(category);
            }

            ScreenManager.Instance.roomScreen.CountNeedsUpdate = true;
            await manager.Connection!.SendPacketAsync(new ServerboundUpdateSessionOptionsPacket(manager.Session));
        });
    }
}