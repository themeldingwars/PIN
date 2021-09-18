using GameServer.Enums.GSS.Character;
using Shared.Udp;

namespace GameServer.Packets.GSS.Character.ObserverView
{
    [GSSMessage(Enums.GSS.Controllers.Character_ObserverView, (byte)Events.Respawned)]
    public class Respawned
    {
        [Field] public uint LastUpdateTime;

        [Field] public ushort Unk1;
    }
}