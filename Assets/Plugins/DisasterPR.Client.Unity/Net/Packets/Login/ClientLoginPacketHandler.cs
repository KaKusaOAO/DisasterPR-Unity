using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using Mochi.Utils;

namespace DisasterPR.Client.Unity.Net.Packets.Login
{
    public class ClientLoginPacketHandler : IClientLoginPacketHandler
    {
        public PlayerToServerConnection Connection { get; }
        public LocalPlayer Player => Connection.Player;

        public ClientLoginPacketHandler(PlayerToServerConnection connection)
        {
            Connection = connection;
        }

        public void HandleAckLogin(ClientboundAckLoginPacket packet)
        {
            var model = packet.Player;
            Logger.Verbose($"Player {model.Name} ID is {model.Guid}");
            Player.Name = model.Name;
            Player.Id = model.Guid;
            Player.Identifier = model.Identifier;
            Player.AvatarData = model.AvatarData;
            Connection.CurrentState = PacketState.Play;
        }

        public void HandleDisconnect(ClientboundDisconnectPacket packet)
        {
            Connection.HandleDisconnect(packet);
        }

        public void HandleSystemChat(ClientboundSystemChatPacket packet)
        {
            
        }
    }
}