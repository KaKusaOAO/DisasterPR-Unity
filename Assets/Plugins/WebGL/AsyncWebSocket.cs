#if !(UNITY_WEBGL && !UNITY_EDITOR)
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DisasterPR.Client.Unity.Backends.WebSockets;
using UnityEngine;
using WebSocketState = DisasterPR.Client.Unity.Backends.WebSockets.WebSocketState;

public class AsyncWebSocket : IWebSocket
{
    private ClientWebSocket _webSocket = new();
    private Uri _uri;
    private MemoryStream _buffer = new();

    public AsyncWebSocket(Uri uri)
    {
        _uri = uri;
    }

    public void Connect()
    {
        _ = Task.Run(async () =>
        {
            await _webSocket.ConnectAsync(_uri, CancellationToken.None);
            OnOpen?.Invoke();

            _ = RunEventLoopAsync();
        });
    }

    private async Task RunEventLoopAsync()
    {
        while (!_webSocket.CloseStatus.HasValue)
        {
            await Task.Delay(16);

            try
            {
                var buffer = new byte[4096];
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _buffer.Write(buffer, 0, result.Count);

                if (result.EndOfMessage)
                {
                    var buf = _buffer.GetBuffer();
                    var pos = _buffer.Position;
                    var arr = new byte[pos];
                    Array.Copy(buf, 0, arr, 0, pos);
                    OnMessage?.Invoke(arr);

                    _buffer = new MemoryStream();
                }

                if (!_webSocket.CloseStatus.HasValue) continue;

                var c = _webSocket.CloseStatus.Value switch
                {
                    WebSocketCloseStatus.NormalClosure => WebSocketCloseCode.Normal,
                    WebSocketCloseStatus.PolicyViolation => WebSocketCloseCode.PolicyViolation,
                    WebSocketCloseStatus.MessageTooBig => WebSocketCloseCode.TooBig,
                    WebSocketCloseStatus.InvalidPayloadData => WebSocketCloseCode.InvalidData,
                    _ => WebSocketCloseCode.NotSet
                };
                OnClose?.Invoke(c);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        OnClose?.Invoke(WebSocketCloseCode.Abnormal);
    }

    public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
    {
        _ = Task.Run(async () =>
        {
            var c = code switch
            {
                WebSocketCloseCode.Normal => WebSocketCloseStatus.NormalClosure,
                WebSocketCloseCode.PolicyViolation => WebSocketCloseStatus.PolicyViolation,
                WebSocketCloseCode.TooBig => WebSocketCloseStatus.MessageTooBig,
                WebSocketCloseCode.InvalidData => WebSocketCloseStatus.InvalidPayloadData,
                _ => WebSocketCloseStatus.Empty
            };
            await _webSocket.CloseAsync(c, reason ?? "", CancellationToken.None);
        });
    }

    public void Send(byte[] data)
    {
        _ = Task.Run(async () =>
        {
            await _webSocket.SendAsync(new ArraySegment<byte>(data),
                WebSocketMessageType.Binary, true, CancellationToken.None);
        });
    }

    public WebSocketState GetState()
    {
        return _webSocket.State switch
        {
            System.Net.WebSockets.WebSocketState.Connecting => WebSocketState.Connecting,
            System.Net.WebSockets.WebSocketState.Open => WebSocketState.Open,
            System.Net.WebSockets.WebSocketState.Closed => WebSocketState.Closed,
            System.Net.WebSockets.WebSocketState.CloseReceived => WebSocketState.Closing,
            System.Net.WebSockets.WebSocketState.CloseSent => WebSocketState.Closing,
            System.Net.WebSockets.WebSocketState.Aborted => WebSocketState.Closed,
            System.Net.WebSockets.WebSocketState.None => WebSocketState.Closed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public event WebSocketOpenEventHandler OnOpen;
    public event WebSocketMessageEventHandler OnMessage;
    public event WebSocketErrorEventHandler OnError;
    public event WebSocketCloseEventHandler OnClose;
}
#endif