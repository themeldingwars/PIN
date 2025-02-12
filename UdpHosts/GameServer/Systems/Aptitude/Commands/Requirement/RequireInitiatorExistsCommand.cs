using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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