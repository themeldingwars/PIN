using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireNotRespawnedCommand : Command, ICommand
{
    private RequireNotRespawnedCommandDef Params;

    public RequireNotRespawnedCommand(RequireNotRespawnedCommandDef par)
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