using System.Threading.Tasks;
using DisasterPR.Net.Packets.Play;
using TMPro;
using UnityEngine;

public class WinScreen : MonoBehaviour, IScreen
{
    public TMP_Text playerNameText;
    
    void Start()
    {
        
    }

    void Update()
    {
        var session = GameManager.Instance.Session;
        if (session == null) return;

        var winner = session.GameState.WinnerPlayer;
        playerNameText.text = winner?.Name ?? "???";
    }

    public void OnBackClicked()
    {
        var manager = GameManager.Instance;
        if (manager.Connection == null) return;

        _ = Task.Run(async () =>
        {
            await manager.Connection.SendPacketAsync(new ServerboundLeaveRoomPacket());
        });
        
        UIManager.Instance.AddTransitionStinger(() =>
        {
            var screens = ScreenManager.Instance;
            screens.SwitchToPage(screens.lobbyScreen);
        });
    }

    public void OnReplayClicked()
    {
        UIManager.Instance.AddTransitionStinger(() =>
        {
            ScreenManager.Instance.SwitchToPage(ScreenManager.Instance.roomScreen);
        });
    }

    public void OnTransitionedIn()
    {
        ScreenManager.Instance.SwitchToSpinnerBackground();
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            GameManager.Instance.RunOnUnityThread(() =>
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.partyFX);
            });
        });
    }

    public void OnTransitionedOut()
    {
        ScreenManager.Instance.SwitchToFlowingBackground();
    }
}