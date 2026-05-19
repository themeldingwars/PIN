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

    public override void Execute(Context context, ref CommandResult result)
    {
        var res = context.Shard.Entities.TryGetValue(context.Initiator.EntityId, out _);

        if (res)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}