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

    public bool Execute(Context context)
    {
        return true;
    }
}