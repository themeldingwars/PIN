using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireLineOfSightCommand : Command, ICommand
{
    private RequireLineOfSightCommandDef Params;

    public RequireLineOfSightCommand(RequireLineOfSightCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}