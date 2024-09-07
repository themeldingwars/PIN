using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ActivateMissionCommand : Command, ICommand
{
    private ActivateMissionCommandDef Params;

    public ActivateMissionCommand(ActivateMissionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}