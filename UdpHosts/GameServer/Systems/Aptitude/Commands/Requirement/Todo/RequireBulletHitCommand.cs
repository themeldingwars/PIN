using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireBulletHitCommand : Command, ICommand
{
    private RequireBulletHitCommandDef Params;

    public RequireBulletHitCommand(RequireBulletHitCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}