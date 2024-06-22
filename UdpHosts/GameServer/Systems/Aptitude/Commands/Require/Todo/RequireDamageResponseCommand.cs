using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireDamageResponseCommand : ICommand
{
    private RequireDamageResponseCommandDef Params;

    public RequireDamageResponseCommand(RequireDamageResponseCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}