using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireIsNPCCommand : ICommand
{
    private RequireIsNPCCommandDef Params;

    public RequireIsNPCCommand(RequireIsNPCCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}