using GameServer.Entities;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Modifier;

public class StatModifierCommand : Command, ICommand
{
    private StatModifierCommandDef Params;

    public StatModifierCommand(StatModifierCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.Stat > 0x39)
        {
            result.SetFail(StatusCode.Error);
            return;
        }

        if (Params.Permanent == 1)
        {
            Logger.Warning("{Command} {CommandId} has unhandled param Permanent", nameof(StatModifierCommand), Params.Id);
        }

        var stat = (AptitudeStat)Params.Stat;
        float value = AbilitySystem.RegistryOp(context.Register, Params.Value, (Operand)Params.ValueRegop);
        var mod = BuildModifier(Params.Op, stat, value);
        if (mod == null)
        {
            Logger.Warning("{Command} {CommandId} has unsupported Op {Op}", nameof(StatModifierCommand), Params.Id, Params.Op);
            result.SetFail(StatusCode.Error);
            return;
        }

        mod.SourceId = Params.Id;
        context.StatChangelist[stat] = mod;
        (context.Self as BaseAptitudeEntity)?.RefreshStatModifier(stat);

        result.SetPass();
    }

    public override void Reset(Context context)
    {
        if (Params.Stat > 0x39)
        {
            return;
        }

        var stat = (AptitudeStat)Params.Stat;
        if (context.StatChangelist.TryGetValue(stat, out var mod) && mod.SourceId == Params.Id)
        {
            context.StatChangelist.Remove(stat);
            (context.Self as BaseAptitudeEntity)?.RefreshStatModifier(stat);
        }
    }

    private static ActiveStatModifier BuildModifier(byte op, AptitudeStat stat, float value)
    {
        const float PercentScale = 0.01f;
        return (int)op switch
        {
            1 => new ActiveStatModifier { Stat = stat, Multi = 1.0f, Add = value },                             // add
            2 => new ActiveStatModifier { Stat = stat, Multi = value * PercentScale, Add = 0.0f },              // percent (value/100)
            3 => new ActiveStatModifier { Stat = stat, Multi = value, Add = 0.0f },                             // multi
            4 => new ActiveStatModifier { Stat = stat, Multi = 1.0f, Add = value, Cap = value, HasCap = true }, // floor (max)
            5 => new ActiveStatModifier { Stat = stat, Multi = value, Add = 0.0f, Cap = value, HasCap = true }, // special (min after multi)
            _ => null,
        };
    }
}