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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}