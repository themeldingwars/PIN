using System;
using System.Collections.Generic;
using GameServer.Data.SDB.Records.apt;

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
        context.Shard.Abilities.HandleActivateAbility(context.Shard, context.Initiator, Params.AbilityId, context.Shard.CurrentTime, new HashSet<IAptitudeTarget>());
        return true;
    }
}