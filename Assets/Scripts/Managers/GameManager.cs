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
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using JetBrains.Annotations;
using KaLib.Utils;
using UnityEngine;
using Logger = KaLib.Utils.Logger;

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
    }

    public void ResetGame()
    {
        if (_game == null) return;
        _game.Player?.Connection.WebSocket.Close();
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
        
        _game.Player!.Connection.ReceivedPacket += e =>
        {
            var packet = e.Packet;
            var audios = AudioManager.Instance;

            if (packet is ClientboundAckPacket)
            {
                var screens = ScreenManager.Instance;
                screens.landingScreen.button.State = NameSubmitButton.LoginState.Success;
                RunOnUnityThread(() =>
                {
                    audios.PlayOneShot(audios.loginSuccessFX);
                    UIManager.Instance.AddTransitionStinger(() =>
                    {
                        screens.landingScreen.button.State = NameSubmitButton.LoginState.None;
                        screens.SwitchToPage(screens.lobbyScreen);
                    });
                });
            }
            
            if (packet is ClientboundDisconnectPacket dp)
            {
                var screens = ScreenManager.Instance;
                screens.landingScreen.button.State = NameSubmitButton.LoginState.Error;
                screens.landingScreen.button.DisconnectReason = new DisconnectedEventArgs
                {
                    Reason = dp.Reason,
                    Message = dp.Message
                };
                audios.PlayOneShot(audios.loginFailedFX);
            }
            
            if (packet is ClientboundJoinedRoomPacket)
            {
                var screens = ScreenManager.Instance;
                screens.lobbyScreen.State = LobbyScreen.RoomJoinState.Succeed;
                RunOnUnityThread(() =>
                {
                    audios.PlayOneShot(audios.joinedRoomFX);
                    UIManager.Instance.AddTransitionStinger(() =>
                    {
                        screens.SwitchToPage(screens.roomScreen);
                    });
                });
            }

            if (packet is ClientboundRoomDisconnectedPacket rdp)
            {
                var screen = ScreenManager.Instance.lobbyScreen;
                screen.State = LobbyScreen.RoomJoinState.Failed;
                screen.LastError = new RoomDisconnectedException(rdp);

                RunOnUnityThread(() =>
                {
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
            }
            
            if (packet is ClientboundSetCardPackPacket cp)
            {
                var session = _game.Player.Session;
                if (session!.GameState.CurrentState == StateOfGame.Waiting)
                {
                    RunOnUnityThread(() => ScreenManager.Instance.roomScreen.UpdateCardPack(cp.CardPack));
                }
            }

            if (packet is ClientboundAddPlayerPacket adp)
            {
                var session = _game.Player.Session;
                if (session!.GameState.CurrentState == StateOfGame.Waiting)
                {
                    var pl = session.Players.Find(p => adp.PlayerId == p.Id);
                    var index = session.Players.IndexOf(pl);
                    RunOnUnityThread(() =>
                    {
                        ScreenManager.Instance.roomScreen.OnAddPlayer(index);
                    });
                }
            }
            
            if (packet is ClientboundRemovePlayerPacket rpp)
            {
                var session = _game.Player.Session;
                if (session!.GameState.CurrentState == StateOfGame.Waiting)
                {
                    RunOnUnityThread(() =>
                    {
                        ScreenManager.Instance.roomScreen.OnRemovePlayer();
                    });
                }
            }

            if (packet is ClientboundGameStateChangePacket gscp)
            {
                var screens = ScreenManager.Instance;
                var screen = screens.gameScreen;
                
                if (gscp.State == StateOfGame.Started)
                {
                    RunOnUnityThread(() =>
                    {
                        audios.PlayOneShot(audios.loginSuccessFX);
                        UIManager.Instance.AddTransitionStinger(() =>
                        {
                            ScreenManager.Instance.SwitchToPage(screen);
                        });
                    });
                }

                if (gscp.State == StateOfGame.ChoosingTopic)
                {
                    RunOnUnityThread(() =>
                    {
                        audios.PlayOneShot(audios.topicChooseFX);
                        screen.SwitchToFrame(Player == Session!.GameState.CurrentPlayer ?
                            screen.chooseTopicFrame : screen.waitChooseTopicFrame);
                    });
                }

                if (gscp.State == StateOfGame.ChoosingWord)
                {
                    RunOnUnityThread(() =>
                    {
                        audios.PlayOneShot(audios.topicAppearFX);
                        screen.SwitchToFrame(screen.topicFrame);
                    });
                }
                
                if (gscp.State == StateOfGame.ChoosingFinal)
                {
                    RunOnUnityThread(() =>
                    {
                        audios.PlayOneShot(audios.finalChooseFX);
                    });
                }

                if (gscp.State == StateOfGame.WinResult)
                {
                    RunOnUnityThread(() =>
                    {
                        UIManager.Instance.AddTransitionStinger(() =>
                        {
                            screens.SwitchToPage(screens.winScreen);
                        });
                    });
                }
            }

            if (packet is ClientboundUpdateTimerPacket utp)
            {
                RunOnUnityThread(() => ScreenManager.Instance.gameScreen.UpdateTimer(utp.RemainTime));
            }

            if (packet is ClientboundSetWordsPacket)
            {
                RunOnUnityThread(() => ScreenManager.Instance.gameScreen.UpdateWords());
            }

            if (packet is ClientboundAddChosenWordEntryPacket acwep)
            {
                RunOnUnityThread(() => ScreenManager.Instance.gameScreen.OnAddChosenWord(acwep.Id));
            }

            if (packet is ClientboundUpdateSessionOptionsPacket)
            {
                ScreenManager.Instance.roomScreen.CountNeedsUpdate = true;
            }

            if (packet is ClientboundRevealChosenWordEntryPacket cwep)
            {
                RunOnUnityThread(() => ScreenManager.Instance.gameScreen.OnRevealWord(cwep.Guid));
            }

            if (packet is ClientboundSetFinalPacket sfp)
            {
                RunOnUnityThread(() => ScreenManager.Instance.gameScreen.OnSetFinal(sfp.Index));
            }

            if (packet is ClientboundChatPacket chat)
            {
                RunOnUnityThread(() =>
                {
                    var line = $"【{chat.Player}】{chat.Content}";
                    audios.PlayOneShot(audios.chatFX);
                    UIManager.Instance.AddChatFlyout(line);
                });
            }

            return Task.CompletedTask;
        };

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
    
    // Update is called once per frame
    void Update()
    {
        _instance = this;
        while (!_queue.IsEmpty)
        {
            if (!_queue.TryDequeue(out var action)) continue;
            action();
            
            Logger.PollEvents();
        }
    }

    public void RunOnUnityThread(Action action)
    {
        if (_thread == Thread.CurrentThread)
        {
            action();
        }
        else
        {
#if UNITY_WEBGL
            Debug.LogWarning("Should not be using this!");
#endif
            _queue.Enqueue(action);
        }
    }
}