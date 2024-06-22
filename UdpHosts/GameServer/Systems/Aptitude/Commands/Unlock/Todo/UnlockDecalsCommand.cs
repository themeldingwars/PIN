using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockDecalsCommand : ICommand
{
    private UnlockDecalsCommandDef Params;

    public UnlockDecalsCommand(UnlockDecalsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}