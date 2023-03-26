using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DisasterPR.Net.Packets.Play;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

[ExecuteAlways]
public class HoldingWordCard : MonoBehaviour
{
    public int index;
    public Image image;
    public Sprite normalSprite;
    public Sprite selectedSprite;
    public bool selected;

    public TMP_Text labelText;
    
    public GameObject badge;
    public TMP_Text badgeText;

    public WordCardLock cardLock;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        image.transform.localPosition = Vector3.Lerp(image.transform.localPosition,
            new Vector3(selected ? 100 : 0, 0, 0), 1 - Time.fixedDeltaTime * 20);
    }

    // Update is called once per frame
    void Update()
    {
        image.sprite = selected ? selectedSprite : normalSprite;

        var manager = GameManager.Instance;
        if (manager == null) return;
        
        var player = manager.Player;
        var session = player?.Session;
        if (session == null) return;

        if (session.GameState.CurrentTopic == null) return;
        
        var word = player.HoldingCards[index];
        var screen = ScreenManager.Instance.gameScreen;
        selected = screen.LocalSelectedWords.Contains(word);
        labelText.text = word.Card.Label;
        cardLock.locked = word.IsLocked;

        // Badge code starts, nothing goes under this part!!!!
        var isSingle = session.GameState.CurrentTopic.AnswerCount == 1;
        if (isSingle)
        {
            badge.SetActive(false);
            return;
        }
        badge.SetActive(selected);
        
        if (selected)
        {
            badgeText.text = (screen.LocalSelectedWords.ToList().IndexOf(word) + 1).ToString();
        }
    }

    public void OnButtonClicked()
    {
        var player = GameManager.Instance.Player;
        var session = player?.Session;
        if (session == null) return;

        var word = player.HoldingCards[index];
        var screen = ScreenManager.Instance.gameScreen;
        var hasSelected = screen.LocalSelectedWords.Contains(word);
        var audios = AudioManager.Instance;
        
        if (hasSelected)
        {
            Debug.Log($"Removing card: {word.Card.Label}");
            screen.LocalSelectedWords.Remove(word);
            audios.PlayOneShot(audios.popFX);
        }
        else
        {
            var count = session.GameState.CurrentTopic.AnswerCount;
            if (count == 1)
            {
                Debug.Log($"Replacing with card: {word.Card.Label}");
                screen.LocalSelectedWords.Clear();
                screen.LocalSelectedWords.Add(word);
                audios.PlayOneShot(audios.wordSelectFX);
            }
            else
            {
                if (count > screen.LocalSelectedWords.Count)
                {
                    Debug.Log($"Adding card: {word.Card.Label}");
                    screen.LocalSelectedWords.Add(word);
                    audios.PlayOneShot(audios.wordSelectFX);
                }
                else
                {
                    Debug.Log($"Not adding card because it's full");
                }
            }
        }
    }

    public void OnLockClicked()
    {
        var player = GameManager.Instance.Player;
        var session = player?.Session;
        if (session == null) return;

        var locked = player.HoldingCards[index].IsLocked;
        Debug.Log($"Try to set card {index} IsLocked to {!locked}");
        player.Connection.SendPacket(new ServerboundUpdateLockedWordPacket(index, !locked));
    }

    void OnMouseOver()
    {
        
    }

    void OnMouseExit()
    {
        
    }
}
