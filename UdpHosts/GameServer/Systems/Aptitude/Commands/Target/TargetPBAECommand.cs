using System;
using System.Linq;
using System.Numerics;
using GameServer.Data.SDB.Records.apt;
using GameServer.Entities;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class TargetPBAECommand : Command, ICommand
{
    private TargetPBAECommandDef Params;

    public TargetPBAECommand(TargetPBAECommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // Unused:
        // Params.IgnoreWalls
        // Params.ScaleOffset
        // Params.ScaleQuerySize
        // Params.AimPosOffset
        // Params.UseInitPos
        // Params.UseBodyPosition
        if (context.Targets.Count >= Params.MaxTargets)
        {
            Console.WriteLine($"TargetPBAECommand {Id} The context target count exceeds the MaxTargets BEFORE command executes, investigate if this happens");
        }

        float radius = AbilitySystem.RegistryOp(context.Register, Params.Radius, (Operand)Params.RadiusRegop);
        if (Params.UseWeaponRadius == 1)
        {
            Console.WriteLine($"TargetPBAECommand {Id} has UseWeaponRadius set to 1, investigate what to do");
        }

        if (Params.Filter == 1)
        {
            // 2202 commands have value 0, 5 commands have value 1
            // Probably remove from context.Targets all targets selected using other Params
            Console.WriteLine($"TargetPBAECommand {Id} has Filter set to 1, investigate what to do");
        }

        if (Params.IncludeInteractives == 1)
        {
            // Presumably targets with interactions can or can not be targeted?
            Console.WriteLine($"TargetPBAECommand {Id} has IncludeInteractives set to 1, investigate what to do");
        }

        // It seems that all 4 combos of UseInitPos and UseBodyPosition are possible (e.g. they can both be 0, both be 1 or be exclusive)
        // No idea what that implies...
        Vector3 origin = context.Self.Position;
        var matches = context.Shard.Entities
        .Where((pair) =>
        {
            if (pair.Value is BaseAptitudeEntity compatibleEntity)
            {
                if (Params.IncludeSelf == 0 && compatibleEntity == context.Self)
                {
                    // If IncludeSelf is 0, we shall not target self. Otherwise, it's a valid target, and based on our origin it'll almost certainly be included.
                    return false;
                }

                var distance = Vector3.DistanceSquared(origin, compatibleEntity.Position);
                if (distance <= radius)
                {
                    return true;
                }
            }
            
            return false;
        })
        .Select((pair) => pair.Value as BaseAptitudeEntity)
        .ToList();
        matches.ForEach(context.Targets.Push);

        if (context.Targets.Count >= Params.MaxTargets)
        {
            Console.WriteLine($"TargetPBAECommand {Id} The context target count exceeds the MaxTargets AFTER command executes, now what?");
        }

        if (context.Targets.Count < Params.MinTargets)
        {
            Console.WriteLine($"TargetPBAECommand {Id} The context target count is below the MinTargets AFTER command executes, returning false");
            return false;
        }

        return true;
    }
}