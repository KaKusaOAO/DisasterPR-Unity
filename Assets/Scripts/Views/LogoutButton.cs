using UnityEngine;

public class LogoutButton : MonoBehaviour
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
        if (manager.Game == null) return;
        
        manager.Logout();
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.leftRoomFX);
        
        UIManager.Instance.AddTransitionStinger(() =>
        {
            var screens = ScreenManager.Instance;
            screens.SwitchToPage(screens.landingScreen);
        });
    }
}