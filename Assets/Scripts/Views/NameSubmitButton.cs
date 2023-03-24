using System;
using System.Collections;
using System.Threading;
using DisasterPR;
using DisasterPR.Client;
using DisasterPR.Client.Unity;
using DisasterPR.Events;
using HybridWebSocket;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = KaLib.Utils.Logger;
using Task = System.Threading.Tasks.Task;

public class NameSubmitButton : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text statusText;

    private Button _button;
    public LoginState State { get; set; }
    public DisconnectedEventArgs DisconnectReason { get; set; }

    public enum LoginState
    {
        None,
        LoggingIn,
        Error,
        Success
    }

    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (State is LoginState.LoggingIn or LoginState.Success)
        {
            _button.interactable = false;
        }
        else
        {
            _button.interactable = !string.IsNullOrWhiteSpace(inputField.text);
        }
        
        if (State == LoginState.None)
        {
            statusText.gameObject.SetActive(false);
            return;
        }
        
        statusText.gameObject.SetActive(true);

        if (State == LoginState.LoggingIn)
        {
            var text = "登入中";
            for (var i = 0; i < Mathf.RoundToInt(Time.time * 4) % 4; i++)
            {
                text += ".";
            }

            statusText.text = text;
        }
        else if (State == LoginState.Error)
        {
            if (DisconnectReason != null)
            {
                statusText.text = DisconnectReason.Reason switch
                {
                    PlayerKickReason.Disconnected => "已失去連線！",
                    PlayerKickReason.InvalidName => "無效的名字！",
                    PlayerKickReason.ClientTooOld => "請更新你的遊戲！",
                    PlayerKickReason.ServerTooOld => "你的遊戲太新了！？",
                    PlayerKickReason.Custom => DisconnectReason.Message,
                    _ => "發生未知錯誤！"
                };
            }
            else
            {
                statusText.text = "已失去連線！";
            }
        }
        else
        {
            statusText.text = "登入成功！";
        }
    }


    public void OnButtonClick()
    {
        var audios = AudioManager.Instance;
        audios.PlayOneShot(audios.buttonFX);
        
        var manager = GameManager.Instance;
        if (manager.Game != null) return;
        
        var game = manager.CreateGame(new GameOptions
        {
            WebSocket = WebSocketFactory.CreateInstance(Constants.ServerUri.ToString()),
            PlayerName = inputField.text
        });
        
        Debug.Log("Logging in...");
        State = LoginState.LoggingIn;
        game.LoginPlayer();

        Debug.Log("Logged in!");
        Debug.Log("Checking connection...");
        if (!game.Player!.Connection.IsConnected)
        {
            Debug.Log("Connection is dead");
            State = LoginState.Error;
            DisconnectReason = new DisconnectedEventArgs()
            {
                Reason = PlayerKickReason.Custom,
                Message = "無法連線到伺服器！"
            };
            manager.ResetGame();
                
            manager.RunOnUnityThread(() =>
            {
                audios.PlayOneShot(audios.loginFailedFX);
            });
        }
        else
        {
            Debug.Log("Connection is determined alive");
        }
    }
}
