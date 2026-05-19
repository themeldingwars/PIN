using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireItemAttributeCommand : Command, ICommand
{
    private RequireItemAttributeCommandDef Params;

    public RequireItemAttributeCommand(RequireItemAttributeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}