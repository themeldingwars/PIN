using MyGameServer.Enums.GSS.Character;
using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController
{
    [GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Events.JumpActioned)]
    public class JumpActioned
    {
        [Field] public ushort JumpTime;
    }
}