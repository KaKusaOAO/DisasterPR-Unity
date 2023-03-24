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
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.buttonFX);
        
        var manager = GameManager.Instance;
        if (manager.Connection == null) return;
        
        manager.Connection.SendPacket(new ServerboundLeaveRoomPacket());
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.leftRoomFX);
        
        UIManager.Instance.AddTransitionStinger(() =>
        {
            var screens = ScreenManager.Instance;
            screens.SwitchToPage(screens.lobbyScreen);
        });
    }
}