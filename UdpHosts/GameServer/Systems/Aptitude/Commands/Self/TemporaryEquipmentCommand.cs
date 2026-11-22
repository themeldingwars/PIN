using GameServer.Data;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Self;

public class TemporaryEquipmentCommand : Command, ICommand
{
    private readonly TemporaryEquipmentCommandDef Params;

    public TemporaryEquipmentCommand(TemporaryEquipmentCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params == null || (Params.PrimaryItemId == 0 && Params.SecondaryItemId == 0))
        {
            Logger.Warning("{Command} {CommandId} has no equipment configured, skipping", nameof(TemporaryEquipmentCommand), Id);
            result.SetPass();
            return;
        }

        if (context.Self is CharacterEntity { IsNpc: false, CurrentLoadout: { } original } character)
        {
            var active = new TemporaryEquipmentState(original, original.WithWeapons(Params.PrimaryItemId, Params.SecondaryItemId));
            context.CommandState[Id] = active;
            ApplyKeepingVitals(context, character, active.Temporary);
            Logger.Information("{Command} {CommandId} equipped {Character} with primary {Primary} and secondary {Secondary}", nameof(TemporaryEquipmentCommand), Id, character, Params.PrimaryItemId, Params.SecondaryItemId);
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
        if (!context.CommandState.Remove(Id, out var state) || state is not TemporaryEquipmentState active || context.Self is not CharacterEntity character)
        {
            return;
        }

        if (!ReferenceEquals(character.CurrentLoadout, active.Temporary))
        {
            Logger.Information("{Command} {CommandId} found another loadout on {Character}, leaving it", nameof(TemporaryEquipmentCommand), Id, character);
            return;
        }

        ApplyKeepingVitals(context, character, active.Original);
        Logger.Information("{Command} {CommandId} restored the loadout of {Character}", nameof(TemporaryEquipmentCommand), Id, character);
    }

    private static void ApplyKeepingVitals(Context context, CharacterEntity character, CharacterLoadout loadout)
    {
        var health = character.CurrentHealth;
        var charge = context.Shard.SuperCharge.GetCharge(character);
        character.ApplyLoadout(loadout);
        character.SetCurrentHealth(health);
        if (charge > 0)
        {
            context.Shard.SuperCharge.SetCharge(character, charge);
        }
    }
}

public record TemporaryEquipmentState(CharacterLoadout Original, CharacterLoadout Temporary);
