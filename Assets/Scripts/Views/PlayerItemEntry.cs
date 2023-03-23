using System;
using System.Threading.Tasks;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemEntry : MonoBehaviour
{
    public int index;
    public Image avatarImage;
    public TMP_Text nameText;
    public TMP_Text statusText;
    public Button kickButton;

    private Color _readyColor = new Color(0.1949163f, 0.5943396f, 0.1934407f);

    void Update()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;
        
        var session = game.Player!.Session;
        if (session == null) return;

        if (index >= session.Players.Count)
        {
            Destroy(gameObject);
            return;
        }
        
        var player = session.Players[index];
        var sprites = AvatarManager.Instance.sprites;
        avatarImage.sprite = sprites[Math.Abs(player.Id.GetHashCode()) % sprites.Length];
        nameText.text = player.Name;
        
        var dots = "";
        for (var i = 0; i < Mathf.RoundToInt(Time.time * 4) % 4; i++)
        {
            dots += ".";
        }

        statusText.text = player.State switch
        {
            PlayerState.Joining => "加入中" + dots,
            PlayerState.Ready => "已就緒",
            PlayerState.InGame => "遊戲中" + dots,
            _ => "無法取得狀態"
        };

        statusText.color = player.State switch
        {
            PlayerState.Joining => Color.gray,
            PlayerState.Ready => _readyColor,
            PlayerState.InGame => Color.gray,
            _ => Color.gray
        };
    }

    public void OnKickButtonClicked()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;
        
        var session = game.Player!.Session;
        if (session == null) return;
        
        var player = session.Players[index];
        _ = Task.Run(async () =>
        {
            await game.Player.Connection.SendPacketAsync(new ServerboundRequestKickPlayerPacket(player));
        });
    }
}