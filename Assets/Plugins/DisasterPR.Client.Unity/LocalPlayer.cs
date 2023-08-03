using System;
using System.Collections.Generic;
using DisasterPR.Client.Unity.Backends.WebSockets;
using DisasterPR.Net;
using DisasterPR.Net.Packets.Handshake;
using DisasterPR.Net.Packets.Login;
using Mochi.Utils;

namespace DisasterPR.Client.Unity
{
    public class LocalPlayer : AbstractClientPlayer
    {
        public PlayerToServerConnection Connection { get; }

        public override List<HoldingWordCardEntry> HoldingCards { get; } = new();

        private PlayerPlatform _type;

        public LocalPlayer(IWebSocket webSocket, string name) : base(name)
        {
            Connection = new PlayerToServerConnection(webSocket, this);
            Connection.WebSocket.OnOpen += InternalLogin;
        }

        public void Login(PlayerPlatform type)
        {
            _type = type;
            Connection.Connect();
        }
    
        private void InternalLogin()
        {
            Logger.Info("Connection opened, sending handshake");
            Connection.SendPacket(new ServerboundHelloPacket(Constants.ProtocolVersion));
            Connection.CurrentState = PacketState.Login;

            switch (_type)
            {
                case PlayerPlatform.Plain:
                    Connection.SendPacket(new ServerboundLoginPacket(Name));
                    break;
                case PlayerPlatform.Discord:
                    var token = DiscordIntegrateHelper.DCGetAccessToken();
                    Connection.SendPacket(ServerboundLoginPacket.CreateDiscord(token));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_type), _type, null);
            }
        }
    }
}