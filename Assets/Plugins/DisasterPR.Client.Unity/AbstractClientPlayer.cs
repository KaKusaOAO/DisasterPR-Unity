using System;
using System.Collections.Generic;
using DisasterPR.Client.Unity.Sessions;
using DisasterPR.Net.Packets;
using DisasterPR.Sessions;
using JetBrains.Annotations;
using UnityEngine;

namespace DisasterPR.Client.Unity
{
    public abstract class AbstractClientPlayer : IPlayer
    {
        private byte[] _avatarData;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public event Action AvatarUpdated;

        public byte[] AvatarData
        {
            get => _avatarData;
            set
            {
                if (_avatarData == value) return;
                _avatarData = value;
                InternalAvatarUpdated();
                AvatarUpdated?.Invoke();
            }
        }

        private void InternalAvatarUpdated()
        {
            if (_avatarData == null)
            {
                Sprite = null;
                return;
            }
            
            TaskManager.Instance.AddTickable(() =>
            {
                var texture = new Texture2D(2, 2);
                texture.LoadImage(_avatarData);
                Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                return true;
            });
        }

        [CanBeNull] public Sprite Sprite { get; private set; }

        public string Identifier { get; set; }
        public LocalSession? Session { get; set;  }
        ISession? IPlayer.Session => Session;
    
        public int Score { get; set; }
        public abstract List<HoldingWordCardEntry> HoldingCards { get; }
        public PlayerState State { get; set; }

        protected AbstractClientPlayer(PlayerDataModel model)
        {
            Id = model.Guid;
            Name = model.Name;
            Identifier = model.Identifier;
            AvatarData = model.AvatarData;
        }

        protected AbstractClientPlayer(string name)
        {
            Name = name;
            Identifier = "<unknown>";
        }
    }
}