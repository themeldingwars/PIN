using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class UiNamedVariableCommand : Command, ICommand
{
    private UiNamedVariableCommandDef Params;

    public UiNamedVariableCommand(UiNamedVariableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}