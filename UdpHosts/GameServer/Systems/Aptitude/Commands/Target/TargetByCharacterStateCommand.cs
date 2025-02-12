using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
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
            if (target is CharacterEntity character)
            {
                var characterState = character.CharacterState.State;

                if (Params.Respawning == 1 && characterState == CharacterStatus.Respawning)
                {
                    newTargets.Push(character);
                }
                else if (Params.Incapacitated == 1 && characterState == CharacterStatus.Incapacitated)
                {
                    newTargets.Push(character);
                }
                else if (Params.Traumatized == 1 && characterState == CharacterStatus.Traumatized)
                {
                    newTargets.Push(character);
                }
                else if (Params.Ghost == 1 && characterState == CharacterStatus.Ghost)
                {
                    newTargets.Push(character);
                }
                else if (Params.Living == 1 && characterState == CharacterStatus.Living)
                {
                    newTargets.Push(character);
                }
                else if (Params.Dead == 1 && characterState == CharacterStatus.Dead)
                {
                    newTargets.Push(character);
                }
                else if (Params.Spawning == 1 && characterState == CharacterStatus.Spawning)
                {
                    newTargets.Push(character);
                }
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            return false;
        }

        return true;
    }
}