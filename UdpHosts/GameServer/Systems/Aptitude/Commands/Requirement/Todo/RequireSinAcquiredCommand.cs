using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireSinAcquiredCommand : Command, ICommand
{
    private RequireSinAcquiredCommandDef Params;

    public RequireSinAcquiredCommand(RequireSinAcquiredCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}