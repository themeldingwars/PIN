using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class StartArcCommand : Command, ICommand
{
    private StartArcCommandDef Params;

    public StartArcCommand(StartArcCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}