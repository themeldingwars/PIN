using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

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
        context.Shard.Abilities.HandleActivateAbility(context.Shard, context.Initiator, Params.AbilityId, context.Shard.CurrentTime, new AptitudeTargets(), context.ExecutionId);
        return true;
    }
}