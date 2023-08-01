using DisasterPR.Net.Packets.Login;
using TMPro;
using UnityEngine;

public class LandingScreen : MonoBehaviour, IScreen
{
    public TMP_Text statusText;
    public NameSubmitButton button;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnTransitionedIn()
    {
        statusText.gameObject.SetActive(false);
        UIManager.Instance.chatBox.SetActive(false);
    }

    public void OnTransitionedOut()
    {
        UIManager.Instance.chatBox.SetActive(true);
    }

    public void OnDiscordClicked()
    {
        var token = DiscordIntegrateHelper.DCGetAccessToken();
        if (string.IsNullOrEmpty(token))
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            DiscordIntegrateHelper.DCStartAuthenticate();
#else
            UIManager.Instance.AddSystemToast("該環境無法使用 Discord 登入！").SetErrorStyle();
#endif
        }
        else
        {
            button.StartLoginSequence(ServerboundLoginPacket.LoginType.Discord);
        }
    }
}