using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireAimModeCommand : ICommand
{
    private RequireAimModeCommandDef Params;

    public RequireAimModeCommand(RequireAimModeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}