using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockContentCommand : Command, ICommand
{
    private UnlockContentCommandDef Params;

    public UnlockContentCommand(UnlockContentCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}