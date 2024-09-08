using System;
using System.Collections.Generic;
using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
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
            Console.WriteLine($"StatModifierCommand {Params.Id} has unhandled param Permanent");
        }

        if (context.Self.GetType() == typeof(CharacterEntity))
        {
            context.Actives.Add(this, new StatModifierCommandActiveContext()
            {
                Register = context.Register
            });
        }
        else
        {
            Console.WriteLine($"StatModifierCommand {Id} does nothing because self is not a Character. Self is {context.Self.GetType().Name}. If this is happening, we should investigate why.");
        }

        return true;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        var modifierContext = (StatModifierCommandActiveContext)activeCommandContext;
        if (context.Self.GetType() == typeof(CharacterEntity))
        {
            var character = context.Self as CharacterEntity;

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
        if (context.Self.GetType() == typeof(CharacterEntity))
        {
            var character = context.Self as CharacterEntity;
            character.RemoveStatModifier(Params.Id, (StatModifierIdentifier)Params.Stat);
        }
    }
}

public class StatModifierCommandActiveContext : ICommandActiveContext
{
    public float Register { get; set; }
}