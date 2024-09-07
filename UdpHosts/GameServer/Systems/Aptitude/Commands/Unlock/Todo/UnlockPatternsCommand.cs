using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockPatternsCommand : Command, ICommand
{
    private UnlockPatternsCommandDef Params;

    public UnlockPatternsCommand(UnlockPatternsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}