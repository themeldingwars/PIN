using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities;

namespace GameServer.Aptitude;

public class DestroyAbilityObjectCommand : Command, ICommand
{
    private DestroyAbilityObjectCommandDef Params;

    public DestroyAbilityObjectCommand(DestroyAbilityObjectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // occurs often after TargetClear -> TargetSelf
        foreach (var target in context.Targets)
        {
            context.Shard.EntityMan.Remove((IEntity)target);
        }

        return true;
    }
}