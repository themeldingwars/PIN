using GameServer.Enums.GSS.Character;
using GameServer.Packets.Common;
using Shared.Udp;

namespace GameServer.Packets.GSS.Character.CombatController;

[GSSMessage(Enums.GSS.Controllers.Character_CombatController, (byte)Events.ForcedMovement)]
public class ForcedMovement
{
    [Field]
    public ushort Type;

    [Field]
    public uint Unk1;

    [Field]
    public Vector Position;

    [Field]
    public Vector AimDirection;

    [Field]
    public Vector Velocity;

    [Field]
    public uint GameTime;

    [Field]
    public ushort KeyFrame;
}