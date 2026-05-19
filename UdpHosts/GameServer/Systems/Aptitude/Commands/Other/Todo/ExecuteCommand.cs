using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ExecuteCommand : Command, ICommand
{
    private ExecuteCommandDef Params;

    public ExecuteCommand(ExecuteCommandDef par)
: base(par)
    {
        Params = par;
    }

    // based on ff1189 should act on context.Targets
    public override void Execute(Context context, ref CommandResult result)
    {
    }
}