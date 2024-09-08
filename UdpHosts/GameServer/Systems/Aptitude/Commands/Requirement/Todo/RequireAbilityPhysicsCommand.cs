using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RequireAbilityPhysicsCommand : Command, ICommand
{
    private RequireAbilityPhysicsCommandDef Params;

    public RequireAbilityPhysicsCommand(RequireAbilityPhysicsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}