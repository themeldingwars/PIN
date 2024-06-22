using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireItemAttributeCommand : ICommand
{
    private RequireItemAttributeCommandDef Params;

    public RequireItemAttributeCommand(RequireItemAttributeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}