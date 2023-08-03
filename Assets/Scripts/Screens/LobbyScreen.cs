using System;
using DisasterPR;
using DisasterPR.Exceptions;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class LobbyScreen : MonoBehaviour, IScreen
{
    public ProfileWidget profile;
    public TMP_Text statusText;
    public RoomJoinState State { get; set; }

    public enum RoomJoinState
    {
        None,
        Joining,
        Succeed,
        Failed
    }
    
    [CanBeNull] public RoomDisconnectedException LastError { get; set; }
    
    void Start()
    {
        
    }

    void Update()
    {
        if (State == RoomJoinState.None)
        {
            statusText.gameObject.SetActive(false);
            return;
        }
        
        statusText.gameObject.SetActive(true);

        if (State == RoomJoinState.Joining)
        {
            var text = "正在加入房間";
            for (var i = 0; i < Mathf.RoundToInt(Time.time * 4) % 4; i++)
            {
                text += ".";
            }

            statusText.text = text;
        } else if (State == RoomJoinState.Succeed)
        {
            statusText.text = "已成功加入房間！";
        }
        else
        {
            if (LastError == null)
            {
                statusText.text = "加入房間時發生錯誤！";
                return;
            }

            if (LastError.Reason == RoomDisconnectReason.Custom)
            {
                statusText.text = LastError.Message;
                return;
            }

            switch (LastError.Reason)
            {
                case RoomDisconnectReason.Kicked:
                    statusText.text = "您已被踢出房間！";
                    break;
                case RoomDisconnectReason.GuidDuplicate:
                    statusText.text = "玩家內部 ID 重複！請重新登入。";
                    break;
                case RoomDisconnectReason.NotFound:
                    statusText.text = "找不到該房間！";
                    break;
                case RoomDisconnectReason.RoomFull:
                    statusText.text = "房間已滿！";
                    break;
                case RoomDisconnectReason.RoomPlaying:
                    statusText.text = "該房間已開始遊戲！";
                    break;
                case RoomDisconnectReason.NoRoomLeft:
                    statusText.text = "已經沒有更多空房！";
                    break;
                case RoomDisconnectReason.SomeoneLeftWhileInGame:
                    statusText.text = "有人中離遊戲，因此本場遊戲已中止！";
                    break;
            }
        }
    }

    public void OnTransitionedIn()
    {
        if (State != RoomJoinState.Failed)
        {
            State = RoomJoinState.None;
        }
    }

    public void OnTransitionedOut()
    {
        State = RoomJoinState.None;
    }
}