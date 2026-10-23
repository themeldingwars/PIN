using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetByNPCTypeCommand : Command, ICommand
{
    private TargetByNPCTypeCommandDef Params;

    public TargetByNPCTypeCommand(TargetByNPCTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.Type == 0)
        {
            result.SetPass();
            return;
        }

        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();

        foreach (IAptitudeTarget target in previousTargets)
        {
            if (target is CharacterEntity { IsPlayerControlled: false } npc && npc.Character_ObserverView.NPCTypeProp == Params.Type)
            {
                newTargets.Push(npc);
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}
