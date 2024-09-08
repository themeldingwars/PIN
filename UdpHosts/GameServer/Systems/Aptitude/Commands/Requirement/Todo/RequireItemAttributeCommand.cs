using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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