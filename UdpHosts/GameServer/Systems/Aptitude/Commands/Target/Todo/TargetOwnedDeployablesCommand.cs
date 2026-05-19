using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetOwnedDeployablesCommand : Command, ICommand
{
    private TargetOwnedDeployablesCommandDef Params;

    public TargetOwnedDeployablesCommand(TargetOwnedDeployablesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity character)
        {
            Logger.Warning("{Command} {CommandId} fails because Self is not a Character. If this is happening, we should investigate why.", nameof(TargetOwnedDeployablesCommand), Params.Id);
            result.SetFail();
            return;
        }

        context.FormerTargets = new AptitudeTargets(context.Targets);

        foreach (var d in character.OwnedDeployables)
        {
            context.Targets.Push(d);
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}