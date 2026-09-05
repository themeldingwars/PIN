using GameServer.Entities.Character;
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
            result.SetFail(StatusCode.Status1);
            return;
        }

        if (Params.Permanent == 1)
        {
            Logger.Warning("{Command} {CommandId} has unhandled param Permanent", nameof(StatModifierCommand), Params.Id);
        }

        if (context.Self is CharacterEntity)
        {
            context.Actives.Add(this, new StatModifierCommandActiveContext() { Register = context.Register });
        }
        else
        {
            Logger.Warning("{Command} {CommandId} does nothing because self is not a Character. Self is {sourceType}. If this is happening, we should investigate why.", nameof(StatModifierCommand), Params.Id, context.Self.GetType().Name);
        }

        result.SetPass();
        return;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        var modifierContext = (StatModifierCommandActiveContext)activeCommandContext;
        var stat = (AptitudeStat)Params.Stat;
        float value = AbilitySystem.RegistryOp(modifierContext.Register, Params.Value, (Operand)Params.ValueRegop);

        var mod = BuildModifier(Params.Op, stat, value);
        if (mod == null)
        {
            Logger.Warning("{Command} {CommandId} has unsupported Op {Op}", nameof(StatModifierCommand), Params.Id, Params.Op);
            return;
        }

        context.StatChangelist[stat] = mod;

        if (context.Self is CharacterEntity character)
        {
            character.RefreshStatModifier(stat);
        }
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        var stat = (AptitudeStat)Params.Stat;
        if (context.StatChangelist.Remove(stat) && context.Self is CharacterEntity character)
        {
            character.RefreshStatModifier(stat);
        }
    }

    private static ActiveStatModifier BuildModifier(byte op, AptitudeStat stat, float value)
    {
        const float PercentScale = 0.01f;
        return (int)op switch
        {
            0 => new ActiveStatModifier { Stat = stat, Multi = 1.0f, Add = value },                             // add
            1 => new ActiveStatModifier { Stat = stat, Multi = value * PercentScale, Add = 0.0f },              // percent (value/100)
            2 => new ActiveStatModifier { Stat = stat, Multi = value, Add = 0.0f },                             // multi
            3 => new ActiveStatModifier { Stat = stat, Multi = 1.0f, Add = value, Cap = value, HasCap = true }, // floor (max)
            4 => new ActiveStatModifier { Stat = stat, Multi = value, Add = 0.0f, Cap = value, HasCap = true }, // special (min after multi)
            _ => null,
        };
    }
}

public class StatModifierCommandActiveContext : ICommandActiveContext
{
    public float Register { get; set; }
}