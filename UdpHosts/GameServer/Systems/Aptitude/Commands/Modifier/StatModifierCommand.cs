using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class StatModifierCommand : Command, ICommand
{
    private StatModifierCommandDef Params;

    public StatModifierCommand(StatModifierCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
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

        return true;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        var modifierContext = (StatModifierCommandActiveContext)activeCommandContext;
        if (context.Self is CharacterEntity character)
        {
            float value = AbilitySystem.RegistryOp(modifierContext.Register, Params.Value, (Operand)Params.ValueRegop);

            var mod = new CharacterEntity.ActiveStatModifier()
            {
                Op = Params.Op,
                Stat = (StatModifierIdentifier)Params.Stat,
                Value = value,
            };
            character.AddStatModifier(Params.Id, mod);
        }
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        if (context.Self is CharacterEntity character)
        {
            character.RemoveStatModifier(Params.Id, (StatModifierIdentifier)Params.Stat);
        }
    }
}

public class StatModifierCommandActiveContext : ICommandActiveContext
{
    public float Register { get; set; }
}