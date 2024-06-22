using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireTookDamageCommand : ICommand
{
    private RequireTookDamageCommandDef Params;

    public RequireTookDamageCommand(RequireTookDamageCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}