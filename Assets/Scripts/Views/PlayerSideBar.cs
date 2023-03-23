using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSideBar : MonoBehaviour
{
    public Sprite[] spriteCountMap;
    public Image graphic;

    void Update()
    {
        var manager = GameManager.Instance;
        var session = manager.Session;
        if (session == null) return;

        var count = Math.Clamp(session.Players.Count, 2, 8);
        if (spriteCountMap.Length - 1 < count) return;

        graphic.sprite = spriteCountMap[count];
    }
}