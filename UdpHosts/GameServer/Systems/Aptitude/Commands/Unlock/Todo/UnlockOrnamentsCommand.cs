using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockOrnamentsCommand : Command, ICommand
{
    private UnlockOrnamentsCommandDef Params;

    public UnlockOrnamentsCommand(UnlockOrnamentsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}