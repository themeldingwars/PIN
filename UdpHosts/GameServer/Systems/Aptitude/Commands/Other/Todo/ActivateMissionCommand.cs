using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ActivateMissionCommand : Command, ICommand
{
    private ActivateMissionCommandDef Params;

    public ActivateMissionCommand(ActivateMissionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}