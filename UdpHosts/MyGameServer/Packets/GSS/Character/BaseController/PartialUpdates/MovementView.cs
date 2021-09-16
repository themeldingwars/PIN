using MyGameServer.Packets.Common;
using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates
{
    [PartialUpdate.PartialShadowFieldAttribute(0x0c)]
    public class MovementView
    {
        [Field] public Vector AimDirection;

        [Field] public ushort AirGroundTimer;

        [Field] public ushort Jets;

        [Field] public ushort JumpTimer;

        [Field] public uint LastUpdateTime;

        [Field] public Vector Position;

        [Field] public Quaternion Rotation;

        [Field] public ushort State;

        [Field] public ushort Unk1;

        [Field] public byte Unk2;

        [Field] public Vector Velocity;
    }
}