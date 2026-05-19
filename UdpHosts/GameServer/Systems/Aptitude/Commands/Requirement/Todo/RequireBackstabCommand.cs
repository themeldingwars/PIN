using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireBackstabCommand : Command, ICommand
{
    private RequireBackstabCommandDef Params;

    public RequireBackstabCommand(RequireBackstabCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}