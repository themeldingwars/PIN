using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireTookDamageCommand : Command, ICommand
{
    private RequireTookDamageCommandDef Params;

    public RequireTookDamageCommand(RequireTookDamageCommandDef par)
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