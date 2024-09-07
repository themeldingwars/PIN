using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ReduceCooldownsCommand : Command, ICommand
{
    private ReduceCooldownsCommandDef Params;

    public ReduceCooldownsCommand(ReduceCooldownsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}