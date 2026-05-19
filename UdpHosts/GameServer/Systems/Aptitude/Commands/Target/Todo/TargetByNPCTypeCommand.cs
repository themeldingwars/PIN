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

    public bool Execute(Context context)
    {
        if (Params.Type == 0)
        {
            return true;
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

        return true;
    }
}