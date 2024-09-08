using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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