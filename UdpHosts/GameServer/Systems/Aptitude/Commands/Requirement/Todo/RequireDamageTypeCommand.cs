using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireDamageTypeCommand : Command, ICommand
{
    private RequireDamageTypeCommandDef Params;

    public RequireDamageTypeCommand(RequireDamageTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}