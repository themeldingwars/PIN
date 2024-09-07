using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class NamedVariableAssignCommand : Command, ICommand
{
    private NamedVariableAssignCommandDef Params;

    public NamedVariableAssignCommand(NamedVariableAssignCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}