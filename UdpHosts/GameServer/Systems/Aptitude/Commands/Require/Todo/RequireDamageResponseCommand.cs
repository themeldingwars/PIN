using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireDamageResponseCommand : Command, ICommand
{
    private RequireDamageResponseCommandDef Params;

    public RequireDamageResponseCommand(RequireDamageResponseCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}