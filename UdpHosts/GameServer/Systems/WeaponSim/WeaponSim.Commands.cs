using System.Linq;
using AeroMessages.GSS.Character;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Systems.Aptitude;

namespace GameServer.Systems.WeaponSim;

/// <summary>
///     Writes the character's fields when a command arrives, the sim follows when it reaches the command's time.
/// </summary>
public partial class WeaponSim
{
    public void SelectWeapon(CharacterEntity entity, byte index, byte previousIndex, uint time)
    {
        if (index != entity.WeaponIndex.Index)
        {
            Holster(entity, entity.WeaponIndex.Index, time);
        }

        entity.SetWeaponIndex(new WeaponIndexData
        {
            Index = index,
            PreviousIndex = previousIndex,
            PlaySwapAnimation = 0,
            Time = time,
        });
    }

    /// <summary>
    ///     Like the client, a weapon without a zoom ignores the scope, and only an alt mode flagged as the scope mode follows
    ///     it.
    /// </summary>
    public void UseScope(CharacterEntity entity, bool inScope, uint time)
    {
        byte slot = entity.WeaponIndex.Index;
        if (entity.GetWeaponDetails(slot, 0) is not { HasZoom: true } main)
        {
            return;
        }

        if (inScope)
        {
            ApplyScopeEffect(entity, main.ScopeStatusFx, time);
        }
        else
        {
            RemoveScopeEffect(entity);
        }

        if (entity.HasAltMode(slot) && entity.GetWeaponDetails(slot, 1).Weapon.Flags.AltIsScopeMode)
        {
            SelectFireMode(entity, slot, (byte)(inScope ? 1 : 0), time);
        }
    }

    /// <summary>
    ///     Takes the weapon slot, 1 or 2, and the client's mode index, which counts modulo the weapon's modes.
    /// </summary>
    public void SelectFireMode(CharacterEntity entity, byte slot, byte mode, uint time)
    {
        mode = entity.HasAltMode(slot) ? (byte)(mode % 2) : (byte)0;
        if (slot is < 1 or > 2 || entity.GetFireMode(slot) == mode)
        {
            return;
        }

        entity.SetFireMode((byte)(slot - 1), new FireModeData { Mode = mode, Time = time });
        EnqueueFireEvent(entity, new WeaponFireEvent
        {
            Time = time,
            Type = WeaponFireEventType.SelectFireMode,
            Slot = slot,
            Value = mode,
        });
    }

    /// <summary>
    ///     Like the client when it applies a different loadout, not on a respawn.
    /// </summary>
    public void ResetFireModes(CharacterEntity entity, uint time)
    {
        RemoveScopeEffect(entity);
        entity.SetFireMode(0, new FireModeData { Mode = 0, Time = time });
        entity.SetFireMode(1, new FireModeData { Mode = 0, Time = time });
    }

    /// <summary>
    ///     Leaves the fire mode alone, like the live servers at a death.
    /// </summary>
    public void RemoveScopeEffect(CharacterEntity entity)
    {
        if (!_weaponSimState.TryGetValue(entity.EntityId, out var weapons) || weapons.ScopeEffect == 0)
        {
            return;
        }

        _shard.Abilities.DoRemoveEffect(entity, weapons.ScopeEffect);
        weapons.ScopeEffect = 0;
    }

    // The holstered weapon leaves its scope and a hold type alt like in the client, a spin-up or grenade alt stays selected
    private void Holster(CharacterEntity entity, byte slot, uint time)
    {
        if (entity.GetWeaponDetails(slot, 0) is not { IsEmpty: false } main)
        {
            return;
        }

        RemoveScopeEffect(entity);
        bool holdAlt = entity.HasAltMode(slot) && entity.GetWeaponDetails(slot, 1).Weapon.Flags.Mode == WeaponFlags.BurstMode.Hold;
        if (main.HasZoom || holdAlt)
        {
            SelectFireMode(entity, slot, 0, time);
        }
    }

    // Melee abilities check the effect's tag to scope out first
    private void ApplyScopeEffect(CharacterEntity entity, uint effectId, uint time)
    {
        // A melee ability or a cleanse can have removed it in the meantime
        if (effectId == 0 || entity.GetActiveEffects().Any(effect => effect?.Effect.Id == effectId))
        {
            return;
        }

        RemoveScopeEffect(entity);
        GetOrCreateWeapons(entity).ScopeEffect = effectId;
        _shard.Abilities.DoApplyEffect(effectId, entity, new Context(_shard, entity) { CurrentTime = time, InitTime = time });
    }
}
