using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class NamedVariableAssignCommand : ICommand
{
    private NamedVariableAssignCommandDef Params;

    public NamedVariableAssignCommand(NamedVariableAssignCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}