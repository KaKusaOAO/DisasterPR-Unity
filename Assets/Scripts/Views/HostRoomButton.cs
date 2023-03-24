using System.Threading.Tasks;
using DisasterPR.Exceptions;
using DisasterPR.Net.Packets.Play;
using UnityEngine;
using UnityEngine.UI;

public class HostRoomButton : MonoBehaviour
{
    public LobbyScreen lobbyScreen;
    private Button _button;
    
    void Start()
    {
        _button = GetComponent<Button>();
    }

    void Update()
    {
        _button.interactable = lobbyScreen.State is 
            LobbyScreen.RoomJoinState.None or LobbyScreen.RoomJoinState.Failed;
    }

    public void OnButtonClick()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
        var manager = GameManager.Instance;
        if (manager.Game == null) return;

        lobbyScreen.State = LobbyScreen.RoomJoinState.Joining;
        manager.Connection!.SendPacket(new ServerboundHostRoomPacket());
    }
}