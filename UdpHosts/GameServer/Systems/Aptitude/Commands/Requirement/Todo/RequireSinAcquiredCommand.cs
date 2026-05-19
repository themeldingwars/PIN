using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireSinAcquiredCommand : Command, ICommand
{
    private RequireSinAcquiredCommandDef Params;

    public RequireSinAcquiredCommand(RequireSinAcquiredCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}