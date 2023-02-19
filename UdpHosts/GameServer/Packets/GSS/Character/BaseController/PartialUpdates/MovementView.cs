using GameServer.Packets.Common;
using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates;

[PartialUpdate.PartialShadowFieldAttribute(0x0c)]
public class MovementView
{
    [Field]
    public uint LastUpdateTime;

    [Field]
    public Vector Position;

    [Field]
    public Quaternion Rotation;

    [Field]
    public Vector AimDirection;

    [Field]
    public Vector Velocity;

    [Field]
    public ushort State;

    [Field]
    public ushort Unknown1;

    [Field]
    public ushort Jets;

    [Field]
    public ushort AirGroundTimer;

    [Field]
    public ushort JumpTimer;

    [Field]
    public byte Unknown2;
}