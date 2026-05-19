using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireInRangeCommand : Command, ICommand
{
    private RequireInRangeCommandDef Params;

    public RequireInRangeCommand(RequireInRangeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}