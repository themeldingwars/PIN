using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireIsNPCCommand : Command, ICommand
{
    private RequireIsNPCCommandDef Params;

    public RequireIsNPCCommand(RequireIsNPCCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}