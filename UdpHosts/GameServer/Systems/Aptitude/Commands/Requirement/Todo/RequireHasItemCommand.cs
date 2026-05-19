using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireHasItemCommand : Command, ICommand
{
    private RequireHasItemCommandDef Params;

    public RequireHasItemCommand(RequireHasItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}