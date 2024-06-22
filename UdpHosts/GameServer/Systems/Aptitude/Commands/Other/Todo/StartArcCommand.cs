using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class StartArcCommand : ICommand
{
    private StartArcCommandDef Params;

    public StartArcCommand(StartArcCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}