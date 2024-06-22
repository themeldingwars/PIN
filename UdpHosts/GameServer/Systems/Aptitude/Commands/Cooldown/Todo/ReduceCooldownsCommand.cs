using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ReduceCooldownsCommand : ICommand
{
    private ReduceCooldownsCommandDef Params;

    public ReduceCooldownsCommand(ReduceCooldownsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}