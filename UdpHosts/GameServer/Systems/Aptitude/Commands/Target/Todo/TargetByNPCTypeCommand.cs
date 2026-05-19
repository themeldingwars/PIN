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

        context.FormerTargets = context.Targets;
        context.Targets = new AptitudeTargets();

        foreach (var target in context.FormerTargets)
        {
            if (target is not CharacterEntity { IsPlayerControlled: false } npc)
            {
                continue;
            }

            if (npc.Character_ObserverView.NPCTypeProp == Params.Type)
            {
                context.Targets.Push(npc);
            }
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}