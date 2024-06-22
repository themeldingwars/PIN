using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CreateSpawnPointCommand : ICommand
{
    private CreateSpawnPointCommandDef Params;

    public CreateSpawnPointCommand(CreateSpawnPointCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}