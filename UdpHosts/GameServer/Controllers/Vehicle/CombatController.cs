using Serilog;

namespace GameServer.Controllers.Vehicle;

[ControllerID(Enums.GSS.Controllers.Vehicle_CombatController)]
public class CombatController : Base
{
    ILogger _logger;
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        // TODO: Implement
    }
}
