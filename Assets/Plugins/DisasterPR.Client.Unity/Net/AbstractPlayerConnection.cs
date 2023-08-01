using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DisasterPR.Client.Unity.Backends.WebSockets;
using DisasterPR.Events;
using DisasterPR.Net;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Login;
using DisasterPR.Net.Packets.Play;
using Mochi.IO;
using Mochi.Texts;
using Mochi.Utils;

namespace DisasterPR.Client.Unity.Net
{
    public delegate Task ReceivedPacketEventAsyncDelegate(ReceivedPacketEventArgs e);

    public abstract class AbstractPlayerConnection
    {
        public IWebSocket WebSocket { get; }
        public bool IsConnected { get; set; }
        public RawPacketIO RawPacketIO { get; }
        protected Dictionary<PacketState, IPacketHandler> Handlers { get; } = new();

        public PacketState CurrentState
        {
            get => _currentState;
            set
            {
                Logger.Verbose($"Current state updated to {value}");
                _currentState = value;
            }
        }

        public event ReceivedPacketEventAsyncDelegate ReceivedPacket;
        public event DisconnectedEventDelegate Disconnected;

        private Stopwatch _stopwatch = new();
        private PacketState _currentState;

        private Dictionary<Type, Action<IPacket>> _typedPacketHandlers = new();

        public void AddTypedPacketHandler<T>(Action<T> handler) where T : IPacket
        {
            _typedPacketHandlers.Add(typeof(T), p => handler((T)p));
        }

        public void ClearTypedPacketHandlers()
        {
            _typedPacketHandlers.Clear();
        }

        protected AbstractPlayerConnection(IWebSocket webSocket)
        {
            WebSocket = webSocket;
            webSocket.OnClose += e =>
            {
                if (e == WebSocketCloseCode.Normal) return;
            
                Disconnected?.Invoke(new DisconnectedEventArgs
                {
                    Reason = PlayerKickReason.Disconnected
                });
            };
        
            RawPacketIO = new RawPacketIO(webSocket);
            RawPacketIO.OnPacketReceived += RawPacketIOOnOnPacketReceived;
            _stopwatch.Start();
        
            TaskManager.Instance.AddTickable(() =>
            {
                Update();
                return !IsConnected;
            });
        }

        public void Update()
        {
            UpdateHeartbeat();
        }

        private void RawPacketIOOnOnPacketReceived(List<MemoryStream> packets)
        {
            foreach (var stream in packets.Select(s => new BufferReader(s)))
            {
                try
                {
                    var id = stream.ReadVarInt();
                    var protocol = ConnectionProtocol.OfState(CurrentState);
                    var packet = protocol.CreatePacket(PacketFlow.Clientbound, id, stream);

                    Logger.Verbose(TranslateText.Of("Received packet: %s")
                        .AddWith(Text.RepresentType(packet.GetType(), TextColor.Gold)));

                    var handler = Handlers[CurrentState];
                    packet.Handle(handler);

                    var type = packet.GetType();
                    if (_typedPacketHandlers.ContainsKey(type))
                    {
                        _typedPacketHandlers[type].Invoke(packet);
                    }

                    ReceivedPacket?.Invoke(new ReceivedPacketEventArgs
                    {
                        Packet = packet
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void UpdateHeartbeat()
        {
            if (_stopwatch.Elapsed.TotalSeconds > 5 && CurrentState == PacketState.Play)
            {
                _stopwatch.Restart();
                SendPacket(new ServerboundHeartbeatPacket());
            }
        }

        public void SendPacket(IPacket packet)
        {
            if (WebSocket.GetState() != WebSocketState.Open)
            {
                IsConnected = false;
                return;
            }
        
            var protocol = ConnectionProtocol.OfState(CurrentState);
            RawPacketIO.SendPacket(protocol, PacketFlow.Serverbound, packet);
        
            Logger.Verbose(TranslateText.Of("Sent packet: %s")
                .AddWith(Text.RepresentType(packet.GetType(), TextColor.Gold)));
        }

        public void HandleDisconnect(ClientboundDisconnectPacket packet)
        {
            Disconnected?.Invoke(new DisconnectedEventArgs()
            {
                Reason = packet.Reason,
                Message = packet.Message
            });
        }
    }
}