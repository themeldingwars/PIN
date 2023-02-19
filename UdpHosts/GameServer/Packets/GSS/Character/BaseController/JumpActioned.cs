using GameServer.Enums.GSS.Character;
using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController;

[GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Events.JumpActioned)]
public class JumpActioned
{
    [Field]
    public ushort JumpTime;
}