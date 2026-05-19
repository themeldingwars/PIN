using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireResourceCommand : Command, ICommand
{
    private RequireResourceCommandDef Params;

    public RequireResourceCommand(RequireResourceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}