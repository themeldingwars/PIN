using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
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

    public bool Execute(Context context)
    {
        if (Params.Permanent == 1)
        {
            Logger.Warning("{Command} {CommandId} has unhandled param Permanent", nameof(StatModifierCommand), Params.Id);
        }

        var character = context.Self as CharacterEntity ?? (context.Self as DeployableEntity)?.Owner;
        if (character != null)
        {
            context.Actives.Add(this, new StatModifierCommandActiveContext() { Register = context.Register });
        }
        else
        {
            Logger.Warning("{Command} {CommandId} does nothing because self is not a Character. Self is {sourceType}. If this is happening, we should investigate why.", nameof(StatModifierCommand), Params.Id, context.Self?.GetType().Name ?? "null");
        }

        return true;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        var modifierContext = (StatModifierCommandActiveContext)activeCommandContext;
        var character = context.Self as CharacterEntity ?? (context.Self as DeployableEntity)?.Owner;
        if (character != null)
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
        var character = context.Self as CharacterEntity ?? (context.Self as DeployableEntity)?.Owner;
        if (character != null)
        {
            character.RemoveStatModifier(Params.Id, (StatModifierIdentifier)Params.Stat);
        }
    }
}

public class StatModifierCommandActiveContext : ICommandActiveContext
{
    public float Register { get; set; }
}