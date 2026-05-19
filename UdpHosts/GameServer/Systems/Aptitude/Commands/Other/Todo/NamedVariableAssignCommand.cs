using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Other;

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