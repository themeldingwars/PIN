using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UpdateSpawnTableCommand : Command, ICommand
{
    private UpdateSpawnTableCommandDef Params;

    public UpdateSpawnTableCommand(UpdateSpawnTableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}