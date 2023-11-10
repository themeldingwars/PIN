using System.Collections.Generic;
using GameServer.Enums.GSS.Character;
using Shared.Udp;

namespace GameServer.Packets.GSS.Character.MissionAndMarkerController;

[GSSMessage(Enums.GSS.Controllers.Character_MissionAndMarkerController, (byte)Events.KeyFrame)]
public class KeyFrame
{
    [Field]
    public ulong PlayerID;

    [Field]
    [Length(12)]
    public IList<byte> UnkBytes;

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
                       0xff,
                       0xff,
                       0xff,
                       0xff,
                       0xff
                   };
    }
}