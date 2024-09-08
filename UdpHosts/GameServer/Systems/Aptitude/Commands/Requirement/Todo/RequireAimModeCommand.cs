using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireAimModeCommand : Command, ICommand
{
    private RequireAimModeCommandDef Params;

    public RequireAimModeCommand(RequireAimModeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}