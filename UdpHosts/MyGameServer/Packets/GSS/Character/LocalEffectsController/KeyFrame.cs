using MyGameServer.Enums.GSS.Character;
using Shared.Udp;
using System.Collections.Generic;

namespace MyGameServer.Packets.GSS.Character.LocalEffectsController
{
    [GSSMessage(Enums.GSS.Controllers.Character_LocalEffectsController, (byte)Events.KeyFrame)]
    public class KeyFrame
    {
        [Field] public ulong PlayerID;

        [Field] [Length(8)] public IList<byte> UnkBytes;

        public KeyFrame(IShard shard)
        {
            UnkBytes = new List<byte>
                       {
                           0xff,
                           0xff,
                           0xff,
                           0xff,
                           0xff,
                           0xff,
                           0xff,
                           0xff
                       };
        }
    }
}