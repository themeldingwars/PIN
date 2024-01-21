using GameServer.Data.SDB.Records.apt;
using System;

namespace GameServer.Aptitude;

public class CallCommand : ICommand
{
    private CallCommandDef Params;

    public CallCommand(CallCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.Shard.Abilities.HandleActivateAbility(context.Shard, context.Initiator, Params.AbilityId, context.Shard.CurrentTime, Array.Empty<IAptitudeTarget>());
        return true;
    }
}