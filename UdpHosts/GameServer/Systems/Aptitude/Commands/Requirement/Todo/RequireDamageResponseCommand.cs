using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

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