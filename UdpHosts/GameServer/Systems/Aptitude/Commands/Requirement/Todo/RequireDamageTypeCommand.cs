using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

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