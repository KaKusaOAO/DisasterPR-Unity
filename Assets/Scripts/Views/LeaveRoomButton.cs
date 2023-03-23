using System.Threading.Tasks;
using DisasterPR.Net.Packets.Play;
using UnityEngine;

public class LeaveRoomButton : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnButtonClick()
    {
        var manager = GameManager.Instance;
        if (manager.Connection == null) return;

        _ = Task.Run(async () =>
        {
            await manager.Connection.SendPacketAsync(new ServerboundLeaveRoomPacket());
            manager.RunOnUnityThread(() => 
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.leftRoomFX));
        });
        
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
        UIManager.Instance.AddTransitionStinger(() =>
        {
            var screens = ScreenManager.Instance;
            screens.SwitchToPage(screens.lobbyScreen);
        });
    }
}