using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class TargetByHealthCommand : Command, ICommand
{
    private TargetByHealthCommandDef Params;

    public TargetByHealthCommand(TargetByHealthCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();

        var healthPct = AbilitySystem.RegistryOp(context.Register, Params.HealthPct, (Operand)Params.HealthRegop);

        foreach (IAptitudeTarget target in previousTargets)
        {
            if (target is CharacterEntity character)
            {
                var currentHealthPct = (character.Character_BaseController.CurrentHealthProp /
                                        character.Character_BaseController.MaxHealthProp.Value) * 100;

                if (currentHealthPct >= healthPct)
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

        return true;
    }
}