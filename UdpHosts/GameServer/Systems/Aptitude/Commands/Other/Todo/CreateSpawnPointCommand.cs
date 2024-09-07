using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CreateSpawnPointCommand : Command, ICommand
{
    private CreateSpawnPointCommandDef Params;

    public CreateSpawnPointCommand(CreateSpawnPointCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}