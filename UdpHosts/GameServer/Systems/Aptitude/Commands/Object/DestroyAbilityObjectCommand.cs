using GameServer.Entities;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Object;

public class DestroyAbilityObjectCommand : Command, ICommand
{
    private DestroyAbilityObjectCommandDef Params;

    public DestroyAbilityObjectCommand(DestroyAbilityObjectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // occurs often after TargetClear -> TargetSelf
        foreach (var target in context.Targets)
        {
            context.Shard.EntityMan.Remove((IEntity)target);
        }

        result.SetPass();
        return;
    }
}