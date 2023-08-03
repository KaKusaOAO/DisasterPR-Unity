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
    public TMP_Text identifierText;
    public TMP_Text statusText;
    public Image indicator;
    public Button kickButton;

    private Color _readyColor = new Color(0.1949163f, 0.5943396f, 0.1934407f);
    private Color _notReadyColor = new Color(0.7943396f, 0.1949163f, 0.1934407f);

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
        
        var dots = "";
        for (var i = 0; i < Mathf.RoundToInt(Time.time * 4) % 4; i++)
        {
            dots += ".";
        }

        statusText.text = player.State switch
        {
            PlayerState.Joining => "加入中" + dots,
            PlayerState.NotReady => "尚未準備",
            PlayerState.Ready => "已準備！",
            PlayerState.InGame => "遊戲中" + dots,
            _ => "無法取得狀態"
        };

        statusText.color = player.State switch
        {
            PlayerState.Joining => Color.gray,
            PlayerState.NotReady => _notReadyColor,
            PlayerState.Ready => _readyColor,
            PlayerState.InGame => Color.gray,
            _ => Color.gray
        };

        indicator.color = statusText.color;
    }

    public void OnKickButtonClicked()
    {
        var game = GameManager.Instance.Game;
        if (game == null) return;
        
        var session = game.Player!.Session;
        if (session == null) return;
        
        var player = session.Players[index];
        game.Player.Connection.SendPacket(new ServerboundRequestKickPlayerPacket(player));
    }
}