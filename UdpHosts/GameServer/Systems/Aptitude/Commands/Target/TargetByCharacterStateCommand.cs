using GameServer.Data.SDB.Records.aptfs;
using static AeroMessages.GSS.V66.Character.CharacterStateData;

namespace GameServer.Aptitude;

public class TargetByCharacterStateCommand : Command, ICommand
{
    private TargetByCharacterStateCommandDef Params;

    public TargetByCharacterStateCommand(TargetByCharacterStateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();
        foreach (IAptitudeTarget target in previousTargets)
        {
            if (target.GetType() == typeof(Entities.Character.CharacterEntity))
            {
                var character = target as Entities.Character.CharacterEntity;

                var characterState = character.CharacterState.State;

                if (Params.Respawning == 1 && characterState == CharacterStatus.Respawning)
                {
                    newTargets.Push(target);
                }
                else if (Params.Incapacitated == 1 && characterState == CharacterStatus.Incapacitated)
                {
                    newTargets.Push(target);
                }
                else if (Params.Traumatized == 1 && characterState == CharacterStatus.Traumatized)
                {
                    newTargets.Push(target);
                }
                else if (Params.Ghost == 1 && characterState == CharacterStatus.Ghost)
                {
                    newTargets.Push(target);
                }
                else if (Params.Living == 1 && characterState == CharacterStatus.Living)
                {
                    newTargets.Push(target);
                }
                else if (Params.Dead == 1 && characterState == CharacterStatus.Dead)
                {
                    newTargets.Push(target);
                }
                else if (Params.Spawning == 1 && characterState == CharacterStatus.Spawning)
                {
                    newTargets.Push(target);
                }
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}