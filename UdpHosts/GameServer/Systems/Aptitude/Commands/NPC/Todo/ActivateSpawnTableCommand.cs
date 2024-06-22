using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ActivateSpawnTableCommand : ICommand
{
    private ActivateSpawnTableCommandDef Params;

    public ActivateSpawnTableCommand(ActivateSpawnTableCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}