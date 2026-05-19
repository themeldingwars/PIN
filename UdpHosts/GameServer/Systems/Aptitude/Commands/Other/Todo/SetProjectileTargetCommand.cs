using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SetProjectileTargetCommand : Command, ICommand
{
    private SetProjectileTargetCommandDef Params;

    public SetProjectileTargetCommand(SetProjectileTargetCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}