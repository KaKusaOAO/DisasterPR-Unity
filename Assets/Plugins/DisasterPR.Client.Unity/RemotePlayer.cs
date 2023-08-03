using System;
using System.Collections.Generic;
using DisasterPR.Net.Packets;

namespace DisasterPR.Client.Unity
{
    public class RemotePlayer : AbstractClientPlayer
    {
        public override List<HoldingWordCardEntry> HoldingCards => throw new NotSupportedException();

        public RemotePlayer(PlayerDataModel model) : base(model)
        {
            
        }
    }
}