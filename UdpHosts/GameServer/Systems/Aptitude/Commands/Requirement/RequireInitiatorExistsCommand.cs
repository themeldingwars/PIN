using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireInitiatorExistsCommand : Command, ICommand
{
    private RequireInitiatorExistsCommandDef Params;

    public RequireInitiatorExistsCommand(RequireInitiatorExistsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return context.Shard.Entities.TryGetValue(context.Initiator.EntityId, out _);
    }
}