using System;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class StatRequirementCommand : Command, ICommand
{
    private const float _equalTolerance = 0.001f;

    private StatRequirementCommandDef Params;

    public StatRequirementCommand(StatRequirementCommandDef par)
: base(par)
    {
        Params = par;
    }

    // Passes when Stat1 of Self compares to a threshold as the flags allow. The threshold is Value, or Value percent of Stat2
    // when that is set (0 means none). Lessthan, Greaterthan and Equalto each allow one outcome.
    // Assumption: the client's loader divides Value by 100 and inverts the flags, its code alone doesn't fit the SDB's rows.
    public override void Execute(Context context, ref CommandResult result)
    {
        // Assumption: a stat the server doesn't know passes, so the rest of the chain still runs
        if (AptitudeStats.Read(context.Self, Params.Stat1) is not { } value)
        {
            Logger.Debug("{Command} {CommandId}: stat {Stat} isn't known for {Self}, passing", nameof(StatRequirementCommand), Params.Id, Params.Stat1, context.Self);
            result.SetPass();
            return;
        }

        // The client combines Value with the register before it scales it, like the other commands' regops
        var threshold = AbilitySystem.RegistryOp(context.Register, Params.Value, (Operand)Params.ValueRegop);
        if (Params.Stat2 != 0)
        {
            if (AptitudeStats.Read(context.Self, Params.Stat2) is not { } of)
            {
                Logger.Debug("{Command} {CommandId}: stat {Stat} isn't known for {Self}, passing", nameof(StatRequirementCommand), Params.Id, Params.Stat2, context.Self);
                result.SetPass();
                return;
            }

            threshold = threshold * of / 100f;
        }

        var passes = MathF.Abs(value - threshold) <= _equalTolerance
                         ? Params.Equalto == 1
                         : value < threshold
                             ? Params.Lessthan == 1
                             : Params.Greaterthan == 1;

        if (passes)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }
    }

    public override void Reset(Context context)
    {
    }
}
