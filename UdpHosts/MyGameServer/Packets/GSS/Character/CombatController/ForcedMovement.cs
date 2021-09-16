using MyGameServer.Enums.GSS.Character;
using MyGameServer.Packets.Common;
using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.CombatController
{
    [GSSMessage(Enums.GSS.Controllers.Character_CombatController, (byte)Events.ForcedMovement)]
    public class ForcedMovement
    {
        [Field] public Vector AimDirection;

        [Field] public uint GameTime;

        [Field] public ushort KeyFrame;

        [Field] public Vector Position;

        [Field] public ushort Type;

        [Field] public uint Unk1;

        [Field] public Vector Velocity;
    }
}