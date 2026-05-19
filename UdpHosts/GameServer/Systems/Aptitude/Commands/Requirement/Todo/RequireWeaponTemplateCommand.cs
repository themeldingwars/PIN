using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireWeaponTemplateCommand : Command, ICommand
{
    private RequireWeaponTemplateCommandDef Params;

    public RequireWeaponTemplateCommand(RequireWeaponTemplateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}