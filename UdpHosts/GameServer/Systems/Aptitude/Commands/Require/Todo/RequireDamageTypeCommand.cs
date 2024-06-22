using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class RequireDamageTypeCommand : ICommand
{
    private RequireDamageTypeCommandDef Params;

    public RequireDamageTypeCommand(RequireDamageTypeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}