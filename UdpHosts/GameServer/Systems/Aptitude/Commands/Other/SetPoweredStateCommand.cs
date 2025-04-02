using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Deployable;

namespace GameServer.Aptitude;

public class SetPoweredStateCommand : Command, ICommand
{
    private SetPoweredStateCommandDef Params;

    public SetPoweredStateCommand(SetPoweredStateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.PowerOn == null)
        {
            return true;
        }

        foreach (var target in context.Targets)
        {
            if (target is not DeployableEntity deployable)
            {
                continue;
            }

            if ((bool)Params.PowerOn)
            {
                context.Shard.Abilities.HandleActivateAbility(context.Shard, target, deployable.PoweredOnAbility);
            }
            else
            {
                context.Shard.Abilities.HandleActivateAbility(context.Shard, target, deployable.PoweredOffAbility);
            }
        }

        return true;
    }
}