using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetCharacterNPCsCommand : Command, ICommand
{
    private TargetCharacterNPCsCommandDef Params;

    public TargetCharacterNPCsCommand(TargetCharacterNPCsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity { IsPlayerControlled: true } player)
        {
            result.SetFail();
            return;
        }

        context.FormerTargets = new AptitudeTargets(context.Targets);

        /*
        foreach (var npc in player.OwnedNPCs)
        {
            context.Targets.Push(npc);
        }
        */

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}