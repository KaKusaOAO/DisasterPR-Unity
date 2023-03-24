/*
 * unity-websocket-webgl
 * 
 * @author Jiri Hybek <jiri@hybek.cz>
 * @copyright 2018 Jiri Hybek <jiri@hybek.cz>
 * @license Apache 2.0 - See LICENSE file distributed with this source code.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;
using DisasterPR.Client.Unity.Backends.WebSockets;

namespace HybridWebSocket
{
#if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary>
    /// WebSocket class bound to JSLIB.
    /// </summary>
    public class WebSocket: IWebSocket
    {

        /* WebSocket JSLIB functions */
        [DllImport("__Internal")]
        public static extern int WebSocketConnect(int instanceId);

        [DllImport("__Internal")]
        public static extern int WebSocketClose(int instanceId, int code, string reason);

        [DllImport("__Internal")]
        public static extern int WebSocketSend(int instanceId, byte[] dataPtr, int dataLength);

        [DllImport("__Internal")]
        public static extern int WebSocketGetState(int instanceId);

        /// <summary>
        /// The instance identifier.
        /// </summary>
        protected int instanceId;

        /// <summary>
        /// Occurs when the connection is opened.
        /// </summary>
        public event WebSocketOpenEventHandler OnOpen;

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event WebSocketMessageEventHandler OnMessage;

        /// <summary>
        /// Occurs when an error was reported from WebSocket.
        /// </summary>
        public event WebSocketErrorEventHandler OnError;

        /// <summary>
        /// Occurs when the socked was closed.
        /// </summary>
        public event WebSocketCloseEventHandler OnClose;

        /// <summary>
        /// Constructor - receive JSLIB instance id of allocated socket
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        public WebSocket(int instanceId)
        {

            this.instanceId = instanceId;

        }

        /// <summary>
        /// Destructor - notifies WebSocketFactory about it to remove JSLIB references
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:HybridWebSocket.WebSocket"/> is reclaimed by garbage collection.
        /// </summary>
        ~WebSocket()
        {
            WebSocketFactory.HandleInstanceDestroy(this.instanceId);
        }

        /// <summary>
        /// Return JSLIB instance ID
        /// </summary>
        /// <returns>The instance identifier.</returns>
        public int GetInstanceId()
        {

            return this.instanceId;

        }

        /// <summary>
        /// Open WebSocket connection
        /// </summary>
        public void Connect()
        {

            int ret = WebSocketConnect(this.instanceId);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

        }

        /// <summary>
        /// Close WebSocket connection with optional status code and reason.
        /// </summary>
        /// <param name="code">Close status code.</param>
        /// <param name="reason">Reason string.</param>
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {

            int ret = WebSocketClose(this.instanceId, (int)code, reason);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

        }

        /// <summary>
        /// Send binary data over the socket.
        /// </summary>
        /// <param name="data">Payload data.</param>
        public void Send(byte[] data)
        {

            int ret = WebSocketSend(this.instanceId, data, data.Length);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

        }

        /// <summary>
        /// Return WebSocket connection state.
        /// </summary>
        /// <returns>The state.</returns>
        public WebSocketState GetState()
        {

            int state = WebSocketGetState(this.instanceId);

            if (state < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(state, null);

            switch (state)
            {
                case 0:
                    return WebSocketState.Connecting;

                case 1:
                    return WebSocketState.Open;

                case 2:
                    return WebSocketState.Closing;

                case 3:
                    return WebSocketState.Closed;

                default:
                    return WebSocketState.Closed;
            }

        }

        /// <summary>
        /// Delegates onOpen event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        public void DelegateOnOpenEvent()
        {

            this.OnOpen?.Invoke();

        }

        /// <summary>
        /// Delegates onMessage event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="data">Binary data.</param>
        public void DelegateOnMessageEvent(byte[] data)
        {

            this.OnMessage?.Invoke(data);

        }

        /// <summary>
        /// Delegates onError event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="errorMsg">Error message.</param>
        public void DelegateOnErrorEvent(string errorMsg)
        {

            this.OnError?.Invoke(errorMsg);

        }

        /// <summary>
        /// Delegate onClose event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="closeCode">Close status code.</param>
        public void DelegateOnCloseEvent(int closeCode)
        {

            this.OnClose?.Invoke(WebSocketHelpers.ParseCloseCodeEnum(closeCode));

        }

    }
#else
    public class WebSocket : IWebSocket
    {

        /// <summary>
        /// Occurs when the connection is opened.
        /// </summary>
        public event WebSocketOpenEventHandler OnOpen;

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event WebSocketMessageEventHandler OnMessage;

        /// <summary>
        /// Occurs when an error was reported from WebSocket.
        /// </summary>
        public event WebSocketErrorEventHandler OnError;

        /// <summary>
        /// Occurs when the socked was closed.
        /// </summary>
        public event WebSocketCloseEventHandler OnClose;

        /// <summary>
        /// The WebSocketSharp instance.
        /// </summary>
        protected WebSocketSharp.WebSocket ws;

        /// <summary>
        /// WebSocket constructor.
        /// </summary>
        /// <param name="url">Valid WebSocket URL.</param>
        public WebSocket(string url)
        {

            try
            {
                   
                // Create WebSocket instance
                this.ws = new WebSocketSharp.WebSocket(url);

                // Bind OnOpen event
                this.ws.OnOpen += (sender, ev) =>
                {
                    this.OnOpen?.Invoke();
                };

                // Bind OnMessage event
                this.ws.OnMessage += (sender, ev) =>
                {
                    if (ev.RawData != null)
                        this.OnMessage?.Invoke(ev.RawData);
                };

                // Bind OnError event
                this.ws.OnError += (sender, ev) =>
                {
                    this.OnError?.Invoke(ev.Message);
                };

                // Bind OnClose event
                this.ws.OnClose += (sender, ev) =>
                {
                    this.OnClose?.Invoke(
                        WebSocketHelpers.ParseCloseCodeEnum( (int)ev.Code )
                    );
                };

            }
            catch (Exception e)
            {

                throw new WebSocketUnexpectedException("Failed to create WebSocket Client.", e);

            }

        }

        /// <summary>
        /// Open WebSocket connection
        /// </summary>
        public void Connect()
        {

            // Check state
            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Open || this.ws.ReadyState == WebSocketSharp.WebSocketState.Closing)
                throw new WebSocketInvalidStateException("WebSocket is already connected or is closing.");

            try
            {
                this.ws.ConnectAsync();
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to connect.", e);
            }

        }

        /// <summary>
        /// Close WebSocket connection with optional status code and reason.
        /// </summary>
        /// <param name="code">Close status code.</param>
        /// <param name="reason">Reason string.</param>
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {

            // Check state
            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Closing)
                throw new WebSocketInvalidStateException("WebSocket is already closing.");

            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Closed)
                throw new WebSocketInvalidStateException("WebSocket is already closed.");

            try
            {
                this.ws.CloseAsync((ushort)code, reason);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to close the connection.", e);
            }

        }

        /// <summary>
        /// Send binary data over the socket.
        /// </summary>
        /// <param name="data">Payload data.</param>
        public void Send(byte[] data)
        {

            // Check state
            if (this.ws.ReadyState != WebSocketSharp.WebSocketState.Open)
                throw new WebSocketInvalidStateException("WebSocket is not in open state.");

            try
            {
                this.ws.Send(data);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to send message.", e);
            }

        }

        /// <summary>
        /// Return WebSocket connection state.
        /// </summary>
        /// <returns>The state.</returns>
        public WebSocketState GetState()
        {

            switch (this.ws.ReadyState)
            {
                case WebSocketSharp.WebSocketState.Connecting:
                    return WebSocketState.Connecting;

                case WebSocketSharp.WebSocketState.Open:
                    return WebSocketState.Open;

                case WebSocketSharp.WebSocketState.Closing:
                    return WebSocketState.Closing;

                case WebSocketSharp.WebSocketState.Closed:
                    return WebSocketState.Closed;

                default:
                    return WebSocketState.Closed;
            }

        }

    }
#endif

    /// <summary>
    /// Class providing static access methods to work with JSLIB WebSocket or WebSocketSharp interface
    /// </summary>
    public static class WebSocketFactory
    {

#if UNITY_WEBGL && !UNITY_EDITOR
        /* Map of websocket instances */
        private static Dictionary<Int32, WebSocket> instances = new Dictionary<Int32, WebSocket>();

        /* Delegates */
        public delegate void OnOpenCallback(int instanceId);
        public delegate void OnMessageCallback(int instanceId, System.IntPtr msgPtr, int msgSize);
        public delegate void OnErrorCallback(int instanceId, System.IntPtr errorPtr);
        public delegate void OnCloseCallback(int instanceId, int closeCode);

        /* WebSocket JSLIB callback setters and other functions */
        [DllImport("__Internal")]
        public static extern int WebSocketAllocate(string url);

        [DllImport("__Internal")]
        public static extern void WebSocketFree(int instanceId);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnClose(OnCloseCallback callback);

        /* If callbacks was initialized and set */
        private static bool isInitialized = false;

        /*
         * Initialize WebSocket callbacks to JSLIB
         */
        private static void Initialize()
        {

            WebSocketSetOnOpen(DelegateOnOpenEvent);
            WebSocketSetOnMessage(DelegateOnMessageEvent);
            WebSocketSetOnError(DelegateOnErrorEvent);
            WebSocketSetOnClose(DelegateOnCloseEvent);

            isInitialized = true;

        }

        /// <summary>
        /// Called when instance is destroyed (by destructor)
        /// Method removes instance from map and free it in JSLIB implementation
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        public static void HandleInstanceDestroy(int instanceId)
        {

            instances.Remove(instanceId);
            WebSocketFree(instanceId);

        }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void DelegateOnOpenEvent(int instanceId)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                instanceRef.DelegateOnOpenEvent();
            }

        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void DelegateOnMessageEvent(int instanceId, System.IntPtr msgPtr, int msgSize)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                byte[] msg = new byte[msgSize];
                Marshal.Copy(msgPtr, msg, 0, msgSize);

                instanceRef.DelegateOnMessageEvent(msg);
            }

        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void DelegateOnErrorEvent(int instanceId, System.IntPtr errorPtr)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {

                string errorMsg = Marshal.PtrToStringAuto(errorPtr);
                instanceRef.DelegateOnErrorEvent(errorMsg);

            }

        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void DelegateOnCloseEvent(int instanceId, int closeCode)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                instanceRef.DelegateOnCloseEvent(closeCode);
            }

        }
#endif

        /// <summary>
        /// Create WebSocket client instance
        /// </summary>
        /// <returns>The WebSocket instance.</returns>
        /// <param name="url">WebSocket valid URL.</param>
        public static IWebSocket CreateInstance(string url)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!isInitialized)
                Initialize();

            int instanceId = WebSocketAllocate(url);
            WebSocket wrapper = new WebSocket(instanceId);
            instances.Add(instanceId, wrapper);

            return wrapper;
#else
            // return new AsyncWebSocket(new Uri(url));
            return new WebSocket(url);
#endif
        }

    }

}