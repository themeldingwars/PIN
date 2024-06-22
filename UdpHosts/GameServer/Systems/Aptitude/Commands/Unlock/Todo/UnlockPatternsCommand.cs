using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockPatternsCommand : ICommand
{
    private UnlockPatternsCommandDef Params;

    public UnlockPatternsCommand(UnlockPatternsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}