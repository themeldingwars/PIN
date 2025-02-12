using System;
using System.Numerics;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class TargetFilterByRangeCommand : Command, ICommand
{
    private TargetFilterByRangeCommandDef Params;

    public TargetFilterByRangeCommand(TargetFilterByRangeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.FormerTargets = context.Targets;
        context.Targets = new AptitudeTargets();

        var range = AbilitySystem.RegistryOp(context.Register, Params.Range, (Operand)Params.RangeRegop);
        var sourcePosition = context.Self.Position;

        foreach (IAptitudeTarget target in context.FormerTargets)
        {
            if (target is CharacterEntity character)
            {
                if (Vector3.Distance(sourcePosition, target.Position) <= range)
                {
                    context.Targets.Push(character);
                }
            }
            else
            {
                Console.WriteLine($"[TargetFilterByRange] Target is not CharacterEntity: {target}");
            }
        }

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            return false;
        }

        return true;
    }
}