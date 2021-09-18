using GameServer.Enums.GSS.Character;
using GameServer.Packets.Common;
using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController
{
    [GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Events.ConfirmedPoseUpdate)]
    public class ConfirmedPoseUpdate
    {
        [Field] public ushort LastJumpTimer;

        [Field] public ushort NextShortTime;

        [Field] public Vector Position;

        [Field] public Quaternion Rotation;

        [Field] public ushort ShortTime;

        [Field] public ushort State;

        [Field] public byte UnkByte1;

        [Field] public byte UnkByte2;

        [Field] public byte UnkByte3;

        [Field] public ushort UnkUShort1;

        [Field] public ushort UnkUShort2;

        [Field] public Vector Velocity;
    }
}