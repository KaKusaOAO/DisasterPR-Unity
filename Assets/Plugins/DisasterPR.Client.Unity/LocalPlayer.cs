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

        private ServerboundLoginPacket.LoginType _type;

        public LocalPlayer(IWebSocket webSocket, string name) : base(name)
        {
            Connection = new PlayerToServerConnection(webSocket, this);
            Connection.WebSocket.OnOpen += InternalLogin;
        }

        public void Login(ServerboundLoginPacket.LoginType type)
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
                case ServerboundLoginPacket.LoginType.Plain:
                    Connection.SendPacket(new ServerboundLoginPacket(Name));
                    break;
                case ServerboundLoginPacket.LoginType.Discord:
                    var token = DiscordIntegrateHelper.DCGetAccessToken();
                    Connection.SendPacket(ServerboundLoginPacket.CreateDiscord(token));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_type), _type, null);
            }
        }
    }
}