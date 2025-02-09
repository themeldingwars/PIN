using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class CallCommand : Command, ICommand
{
    private CallCommandDef Params;

    public CallCommand(CallCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.Shard.Abilities.HandleActivateAbility(context.Shard, context.Initiator, Params.AbilityId);
        return true;
    }
}