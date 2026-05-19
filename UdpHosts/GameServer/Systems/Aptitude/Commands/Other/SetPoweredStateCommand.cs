using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SetPoweredStateCommand : Command, ICommand
{
    private SetPoweredStateCommandDef Params;

    public SetPoweredStateCommand(SetPoweredStateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.PowerOn == null)
        {
            result.SetPass();
            return;
        }

        foreach (var target in context.Targets)
        {
            if (target is not DeployableEntity deployable)
            {
                continue;
            }

            if ((bool)Params.PowerOn)
            {
                context.Shard.Abilities.HandleActivateAbility(context.Shard, target, deployable.PoweredOnAbility, context.Shard.CurrentTime, new AptitudeTargets(), context.ExecutionId);
            }
            else
            {
                context.Shard.Abilities.HandleActivateAbility(context.Shard, target, deployable.PoweredOffAbility, context.Shard.CurrentTime, new AptitudeTargets(), context.ExecutionId);
            }
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}