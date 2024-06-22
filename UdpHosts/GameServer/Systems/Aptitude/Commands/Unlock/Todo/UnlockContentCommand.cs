using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockContentCommand : ICommand
{
    private UnlockContentCommandDef Params;

    public UnlockContentCommand(UnlockContentCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}