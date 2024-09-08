using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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