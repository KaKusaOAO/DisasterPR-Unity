using DisasterPR.Net.Packets.Login;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using Image = UnityEngine.UI.Image;

public class LandingScreen : MonoBehaviour, IScreen
{
    public TMP_Text statusText;
    public NameSubmitButton button;
    public Image modalContainer;
    private RectTransform _modalContainer;
    public RectTransform discordModal;
    private float _modalOpenTime;
    private float _modalBackAlpha;
    private Vector2 _modalInitialPos;

    void Start()
    {
        modalContainer.gameObject.SetActive(false);
        _modalContainer = modalContainer.GetComponent<RectTransform>();
        _modalBackAlpha = modalContainer.color.a;
        _modalInitialPos = discordModal.anchoredPosition;
    }

    void Update()
    {
        var color = modalContainer.color;
        var progress = Mathf.Clamp((Time.time - _modalOpenTime) / 0.5f, 0, 1);
        var alpha = _modalBackAlpha * Easing.OutCirc(Mathf.Clamp(progress * 1.5f, 0, 1));
        modalContainer.color = new Color(color.r, color.g, color.b, alpha);
        discordModal.anchoredPosition = Vector2.Lerp(_modalInitialPos + new Vector2(0, _modalContainer.rect.height),
            _modalInitialPos, Easing.OutCirc(progress));
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
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.popFX);
        var token = DiscordIntegrateHelper.DCGetAccessToken();
        if (string.IsNullOrEmpty(token))
        {
            if (!DiscordIntegrateHelper.DCStartAuthenticate())
            {
                _modalOpenTime = Time.time;
                modalContainer.gameObject.SetActive(true);
            }
        }
        else
        {
            button.StartLoginSequence(PlayerPlatform.Discord);
        }
    }
    
    public void OnDiscordModalClicked()
    {
        modalContainer.gameObject.SetActive(false);
#if UNITY_WEBGL && !UNITY_EDITOR
        DiscordIntegrateHelper.DCStartAuthenticateNoPopup();
#else
        UIManager.Instance.AddSystemToast("該環境無法使用 Discord 登入！").SetErrorStyle();
#endif
    }
}