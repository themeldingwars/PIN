using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UpdateSpawnTableCommand : ICommand
{
    private UpdateSpawnTableCommandDef Params;

    public UpdateSpawnTableCommand(UpdateSpawnTableCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}