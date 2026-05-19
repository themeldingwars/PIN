using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireJumpedCommand : Command, ICommand
{
    private RequireJumpedCommandDef Params;

    public RequireJumpedCommand(RequireJumpedCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}