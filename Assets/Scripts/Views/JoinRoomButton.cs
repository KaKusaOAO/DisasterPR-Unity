using System;
using System.Threading.Tasks;
using DisasterPR.Exceptions;
using DisasterPR.Net.Packets.Play;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomButton : MonoBehaviour
{
    public LobbyScreen lobbyScreen;
    public TMP_InputField roomIdInput;
    private Button _button;
    
    void Start()
    {
        _button = GetComponent<Button>();
    }

    void Update()
    {
        if (lobbyScreen.State is LobbyScreen.RoomJoinState.Joining or LobbyScreen.RoomJoinState.Succeed)
        {
            _button.interactable = false;
            return;
        }
        
        try
        {
            var id = int.Parse(roomIdInput.text);
            _button.interactable = id >= 1000 && id <= 9999;
        }
        catch (FormatException)
        {
            _button.interactable = false;
        }
    }

    public void OnButtonClick()
    {   
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
        var manager = GameManager.Instance;
        if (manager.Game == null) return;
        var id = int.Parse(roomIdInput.text);
        
        _ = Task.Run(async () =>
        {
            lobbyScreen.State = LobbyScreen.RoomJoinState.Joining;
            await manager.Connection!.SendPacketAsync(new ServerboundJoinRoomPacket(id));
        });
    }
}