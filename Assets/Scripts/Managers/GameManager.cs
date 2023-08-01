using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DisasterPR;
using DisasterPR.Client.Unity;
using DisasterPR.Client.Unity.Sessions;
using DisasterPR.Events;
using DisasterPR.Exceptions;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using JetBrains.Annotations;
using Mochi.Utils;
using UnityEngine;
using Logger = Mochi.Utils.Logger;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    private Game _game;

    private ConcurrentQueue<Action> _queue = new();
    private Thread _thread = Thread.CurrentThread;
    
    [CanBeNull] public Game Game => _game;
    [CanBeNull] public LocalPlayer Player => Game?.Player;
    [CanBeNull] public LocalSession Session => Player?.Session;
    [CanBeNull] public PlayerToServerConnection Connection => Player?.Connection;

    public static CancellationToken DefaultTimeoutToken => new CancellationTokenSource(TimeSpan.FromSeconds(6)).Token;

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;
        Application.runInBackground = true;
        
        Logger.Logged += e =>
        {
            Instance.RunOnUnityThread(() =>
            {
                var tag = e.Tag.ToPlainText();
                var content = e.Content.ToPlainText();

                var line = $"[{tag}] {content}";
                switch (e.Level)
                {
                    case LogLevel.Error:
                        Debug.LogError(line);
                        break;
                    case LogLevel.Warn:
                        Debug.LogWarning(line);
                        break;
                    default:
                        Debug.Log(line);
                        break;
                }
            });
            return Task.CompletedTask;
        };
        Logger.RunManualPoll();
        Debug.Log("Registered logger");

        DontDestroyOnLoad(gameObject);
        
        DiscordIntegrateHelper.AccessTokenUpdated += () =>
        {
            var token = DiscordIntegrateHelper.DCGetAccessToken();
            if (!string.IsNullOrEmpty(token) && ScreenManager.Instance.landingScreen.isActiveAndEnabled)
            {
                UIManager.Instance.AddSystemToast("Discord 登入資訊已更新！正在以 Discord 帳號登入...");
                ScreenManager.Instance.landingScreen.button.StartLoginSequence(ServerboundLoginPacket.LoginType.Discord);
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        _instance = this;

        if (Application.isMobilePlatform)
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }

        while (!_queue.IsEmpty)
        {
            if (!_queue.TryDequeue(out var action)) continue;
            action();

            Logger.PollEvents();
        }
    }

    public void ResetGame()
    {
        if (_game == null) return;
        try
        {
            _game.Player?.Connection.WebSocket.Close();
        }
        catch (Exception)
        {
            // ignored   
        }
        
        _game = null;
    }

    public Game CreateGame(GameOptions options)
    {
        if (_game != null)
        {
            Debug.LogWarning("Game already created");
            return _game;
        }
        
        _game = Game.Instance;
        _game.Init(options);

        var connection = _game.Player!.Connection;
        var audios = AudioManager.Instance;
        var screens = ScreenManager.Instance;

        void AddPacketHandler<T>(Action<T> handler) where T : IPacket => 
            connection.AddTypedPacketHandler<T>(packet => RunOnUnityThread(() =>
            {
                Logger.Verbose($"Packet: {typeof(T)}");
                handler(packet);
            }));

        AddPacketHandler<ClientboundAckLoginPacket>(_ =>
        {
            screens.landingScreen.button.State = NameSubmitButton.LoginState.Success;
            audios.PlayOneShot(audios.loginSuccessFX);
            UIManager.Instance.AddTransitionStinger(() =>
            {
                screens.landingScreen.button.State = NameSubmitButton.LoginState.None;
                screens.SwitchToPage(screens.lobbyScreen);
            });
        });
        
        AddPacketHandler<ClientboundDisconnectPacket>(packet =>
        {
            screens.landingScreen.button.State = NameSubmitButton.LoginState.Error;
            screens.landingScreen.button.DisconnectReason = new DisconnectedEventArgs
            {
                Reason = packet.Reason,
                Message = packet.Message
            };

            audios.PlayOneShot(audios.loginFailedFX);
        });
        
        AddPacketHandler<ClientboundJoinedRoomPacket>(_ =>
        {
            screens.lobbyScreen.State = LobbyScreen.RoomJoinState.Succeed;
            audios.PlayOneShot(audios.joinedRoomFX);
            UIManager.Instance.AddTransitionStinger(() =>
            {
                screens.SwitchToPage(screens.roomScreen);
            });
        });
        
        AddPacketHandler<ClientboundRoomDisconnectedPacket>(packet =>
        {
            var screen = screens.lobbyScreen;
            screen.State = LobbyScreen.RoomJoinState.Failed;
            screen.LastError = new RoomDisconnectedException(packet);

            if (screen.gameObject.activeInHierarchy)
            {
                audios.PlayOneShot(audios.loginFailedFX);
                return;
            }
                    
            audios.PlayOneShot(audios.leftRoomFX);
            UIManager.Instance.AddTransitionStinger(() =>
            {
                ScreenManager.Instance.SwitchToPage(screen);
            });
        });
        
        AddPacketHandler<ClientboundSetCardPackPacket>(packet =>
        {
            var session = _game.Player.Session;
            if (session!.GameState.CurrentState == StateOfGame.Waiting)
            {
                screens.roomScreen.UpdateCardPack(packet.CardPack);
            }
        });
        
        AddPacketHandler<ClientboundAddPlayerPacket>(packet =>
        {
            var session = _game.Player.Session;
            if (session!.GameState.CurrentState != StateOfGame.Waiting) return;
            
            var pl = session.Players.Find(p => packet.PlayerId == p.Id);
            var index = session.Players.IndexOf(pl);
            screens.roomScreen.OnAddPlayer(index);
        });
        
        AddPacketHandler<ClientboundRemovePlayerPacket>(_ =>
        {
            var session = _game.Player.Session;
            if (session!.GameState.CurrentState == StateOfGame.Waiting)
            {
                screens.roomScreen.OnRemovePlayer();
            }
        });
        
        AddPacketHandler<ClientboundGameStateChangePacket>(packet =>
        {
            screens.gameScreen.OnChangeToState(packet.State);
        });
        
        AddPacketHandler<ClientboundUpdateTimerPacket>(packet =>
        {
            screens.gameScreen.UpdateTimer(packet.RemainTime);
        });
        
        AddPacketHandler<ClientboundSetWordsPacket>(_ =>
        {
            screens.gameScreen.UpdateWords();
        });
        
        AddPacketHandler<ClientboundAddChosenWordEntryPacket>(packet =>
        {
            screens.gameScreen.OnAddChosenWord(packet.Id);
        });
        
        AddPacketHandler<ClientboundUpdateSessionOptionsPacket>(_ =>
        {
            screens.roomScreen.CountNeedsUpdate = true;
        });
        
        AddPacketHandler<ClientboundRevealChosenWordEntryPacket>(packet =>
        {
            screens.gameScreen.OnRevealWord(packet.Guid);
        });
        
        AddPacketHandler<ClientboundSetFinalPacket>(packet =>
        {
            screens.gameScreen.OnSetFinal(packet.Index);
        });
        
        AddPacketHandler<ClientboundChatPacket>(packet =>
        {
            var line = $"【{packet.Player}】{packet.Content}";
            audios.PlayOneShot(audios.chatFX);
            UIManager.Instance.AddChatFlyout(line);
        });
        
        AddPacketHandler<ClientboundUpdateLockedWordPacket>(_ =>
        {
            audios.PlayOneShot(audios.popFX);
        });
        
        AddPacketHandler<ClientboundSystemChatPacket>(packet =>
        {
            var line = $"{packet.Content}";
            var f = UIManager.Instance.AddSystemToast(line);

            if (packet.Level == LogLevel.Error)
            {
                f.SetErrorStyle();
            }
        });

        _game.Player!.Connection.Disconnected += OnConnectionUnexpectedDisconnected;
        
        return _game;
    }

    private Task OnConnectionUnexpectedDisconnected(DisconnectedEventArgs e)
    {
        var screen = ScreenManager.Instance.landingScreen;
        screen.button.State = NameSubmitButton.LoginState.Error;
        screen.button.DisconnectReason = e;

        RunOnUnityThread(() =>
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.loginFailedFX);
            if (!screen.gameObject.activeInHierarchy)
            {
                UIManager.Instance.AddTransitionStinger(() =>
                {
                    ScreenManager.Instance.SwitchToPage(screen);
                    ResetGame();
                });
            }
            else
            {
                ResetGame();
            }
        });

        return Task.CompletedTask;
    }

    public void Logout()
    {
        if (Connection != null)
        {
            Connection.Disconnected -= OnConnectionUnexpectedDisconnected;
        }

        Connection?.WebSocket.Close();
        _game = null;
    }

    public void RunOnUnityThread(Action action)
    {
        if (_thread == Thread.CurrentThread)
        {
            action();
        }
        else
        {
            _queue.Enqueue(action);
        }
    }
}