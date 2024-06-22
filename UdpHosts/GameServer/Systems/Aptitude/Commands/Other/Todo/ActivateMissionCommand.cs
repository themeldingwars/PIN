using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ActivateMissionCommand : ICommand
{
    private ActivateMissionCommandDef Params;

    public ActivateMissionCommand(ActivateMissionCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}