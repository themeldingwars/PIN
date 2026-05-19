using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequirementServerCommand : Command, ICommand
{
    private RequirementServerCommandDef Params;

    public RequirementServerCommand(RequirementServerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // TODO: Investigate Params.Local, Params.LocalInit
        if (Params.Server == 1)
        {
            result.SetPass();
            return;
        }
        else
        {
            Logger.Warning("{Command} {CommandId} returns false", nameof(RequirementServerCommand), Params.Id);
            result.SetFail();
        }
    }

    public override void Reset(Context context)
    {
        return;
    }
}