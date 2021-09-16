using MyGameServer.Enums.GSS.Generic;
using Shared.Udp;

namespace MyGameServer.Packets.GSS.Generic
{
    [GSSMessage(Enums.GSS.Controllers.Generic, (byte)Commands.ScheduleUpdateRequest)]
    public class ScheduleUpdateRequest
    {
        [Field] public uint requestClientTime;
    }
}