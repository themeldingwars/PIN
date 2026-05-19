using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireSquadLeaderCommand : Command, ICommand
{
    private RequireSquadLeaderCommandDef Params;

    public RequireSquadLeaderCommand(RequireSquadLeaderCommandDef par)
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