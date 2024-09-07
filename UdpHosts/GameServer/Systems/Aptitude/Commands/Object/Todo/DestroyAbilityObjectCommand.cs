using GameServer.Data.SDB.Records.customdata;

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
        // context.Shard.EntityMan.Remove(...);
        return true;
    }
}