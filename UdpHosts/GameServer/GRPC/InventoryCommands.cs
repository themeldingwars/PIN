using System.Collections.Generic;
using AeroMessages.GSS.Character.Event;
using GameServer.Data;
using GrpcGameServerAPIClient;
using LoadoutConfigVisual = AeroMessages.GSS.Character.LoadoutConfig_Visual;

namespace GameServer.GRPC;

/// <summary>
///     Builds the rpc inventory commands. The proto message names collide with the Aero inventory types,
///     so the mapping lives here instead of in CharacterInventory.
/// </summary>
public static class InventoryCommands
{
    /// <summary>
    ///     Upserts one item and replaces its whole module list.
    /// </summary>
    /// <param name="characterGuid">The full character guid the item belongs to</param>
    /// <param name="item">The item as it is sent to the client</param>
    /// <param name="modules">The complete module list, not a delta. An empty list clears the item's modules.</param>
    /// <returns>The command to send</returns>
    public static Command SaveItem(ulong characterGuid, Item item, IReadOnlyList<InventoryModule> modules)
    {
        var save = new SaveInventoryItem()
        {
            CharacterGuid = characterGuid,
            ItemGuid = (long)item.GUID,
            SdbId = item.SdbId,
            SubInventory = item.SubInventory,
            DynamicFlags = item.DynamicFlags,
            Durability = item.Durability,
        };

        foreach (var module in modules)
        {
            save.Modules.Add(new ItemModule() { SlotIndex = module.SlotIndex, ModuleSdbId = module.ModuleSdbId });
        }

        return new Command() { SaveInventoryItem = save };
    }

    /// <summary>
    ///     Sets the absolute quantity of one resource.
    /// </summary>
    /// <param name="characterGuid">The full character guid the resource belongs to</param>
    /// <param name="resource">The resource as it is sent to the client</param>
    /// <returns>The command to send</returns>
    public static Command SaveResource(ulong characterGuid, Resource resource)
        => new()
        {
            SaveInventoryResource = new SaveInventoryResource()
            {
                CharacterGuid = characterGuid,
                SdbId = resource.SdbId,
                Quantity = resource.Quantity,
                SubInventory = resource.SubInventory,
            }
        };

    /// <summary>
    ///     Sets one loadout slot to an item.
    /// </summary>
    /// <param name="characterGuid">The full character guid that owns the loadout</param>
    /// <param name="loadoutId">The loadout to change</param>
    /// <param name="configId">The loadout config, 0 for PvE and 1 for PvP</param>
    /// <param name="slotIndex">The dbitems::LoadoutSlot id of the slot</param>
    /// <param name="itemGuid">The item to put in the slot</param>
    /// <returns>The command to send</returns>
    public static Command SaveLoadoutSlot(ulong characterGuid, int loadoutId, uint configId, byte slotIndex, ulong itemGuid)
        => new()
        {
            SaveLoadoutSlot = new SaveLoadoutSlot()
            {
                CharacterGuid = characterGuid,
                LoadoutId = loadoutId,
                ConfigId = configId,
                SlotIndex = slotIndex,
                ItemGuid = (long)itemGuid,
            }
        };

    /// <summary>
    ///     Clears one loadout slot.
    /// </summary>
    /// <param name="characterGuid">The full character guid that owns the loadout</param>
    /// <param name="loadoutId">The loadout to change</param>
    /// <param name="configId">The loadout config, 0 for PvE and 1 for PvP</param>
    /// <param name="slotIndex">The dbitems::LoadoutSlot id of the slot</param>
    /// <returns>The command to send</returns>
    public static Command DeleteLoadoutSlot(ulong characterGuid, int loadoutId, uint configId, byte slotIndex)
        => new()
        {
            DeleteLoadoutSlot = new DeleteLoadoutSlot()
            {
                CharacterGuid = characterGuid,
                LoadoutId = loadoutId,
                ConfigId = configId,
                SlotIndex = slotIndex,
            }
        };

    /// <summary>
    ///     Replaces the whole visual set of one loadout config: palettes, patterns, decals, glider and vehicle.
    ///     The client re-sends the entire set whenever the player changes one thing, so this replaces the set
    ///     rather than editing one entry, and an empty set clears the config.
    /// </summary>
    /// <param name="characterGuid">The full character guid that owns the loadout</param>
    /// <param name="loadoutId">The loadout whose config changed</param>
    /// <param name="configId">The loadout config, 0 for PvE and 1 for PvP</param>
    /// <param name="visuals">The whole set the config holds now, in the order it has to come back</param>
    /// <returns>The command to send</returns>
    public static Command SaveLoadoutVisuals(ulong characterGuid, int loadoutId, uint configId, IReadOnlyList<LoadoutConfigVisual> visuals)
    {
        var save = new SaveLoadoutVisuals()
        {
            CharacterGuid = characterGuid,
            LoadoutId = loadoutId,
            ConfigId = configId,
        };

        // Order is part of the data: palettes are applied in list order and a later one overwrites an earlier one
        // in the same region, so the set goes out in the order PIN holds it and has to come back the same way.
        foreach (var visual in visuals)
        {
            var entry = new LoadoutVisual
            {
                VisualType = (uint)visual.VisualType,
                ItemSdbId = visual.ItemSdbId,
                Data1 = visual.Data1,
                Data2 = visual.Data2,
            };

            if (visual.Transform != null)
            {
                foreach (var value in visual.Transform)
                {
                    entry.Transform.Add(value);
                }
            }

            save.Visuals.Add(entry);
        }

        return new Command() { SaveLoadoutVisuals = save };
    }

    public static LoadoutConfigVisual ToLoadoutVisual(LoadoutVisual visual)
        => new()
        {
            VisualType = (LoadoutConfigVisual.LoadoutVisualType)visual.VisualType,
            ItemSdbId = visual.ItemSdbId,
            Data1 = visual.Data1,
            Data2 = visual.Data2,
            Transform = [.. visual.Transform],
        };

    public static Command SaveCurrentBattleframe(ulong characterGuid, uint chassisId)
        => new()
        {
            SaveCurrentBattleframe = new SaveCurrentBattleframe()
            {
                CharacterGuid = characterGuid,
                ChassisId = (int)chassisId,
            }
        };

    /// <summary>
    ///     Grants one unlock. Idempotent.
    /// </summary>
    /// <param name="characterGuid">The full character guid</param>
    /// <param name="unlock">The unlock to grant</param>
    /// <returns>The command to send</returns>
    public static Command SaveUnlock(ulong characterGuid, UnlockKey unlock)
        => new()
        {
            SaveCharacterUnlock = new SaveCharacterUnlock()
            {
                CharacterGuid = characterGuid,
                UnlockType = unlock.UnlockType,
                UnlockId = unlock.UnlockId,
                FrameId = unlock.FrameId,
            }
        };

    /// <summary>
    ///     Revokes one unlock.
    /// </summary>
    /// <param name="characterGuid">The full character guid</param>
    /// <param name="unlock">The unlock to revoke</param>
    /// <returns>The command to send</returns>
    public static Command DeleteUnlock(ulong characterGuid, UnlockKey unlock)
        => new()
        {
            DeleteCharacterUnlock = new DeleteCharacterUnlock()
            {
                CharacterGuid = characterGuid,
                UnlockType = unlock.UnlockType,
                UnlockId = unlock.UnlockId,
                FrameId = unlock.FrameId,
            }
        };

    /// <summary>
    ///     Sets the base level and xp of one battleframe.
    /// </summary>
    /// <param name="characterGuid">The full character guid</param>
    /// <param name="frame">The frame to save, keyed by its chassis sdb id</param>
    /// <returns>The command to send</returns>
    public static Command SaveBattleframeXp(ulong characterGuid, BattleframeProgression frame)
        => new()
        {
            SaveBattleframeXp = new SaveBattleframeXp()
            {
                CharacterGuid = characterGuid,
                BattleframeSdbId = frame.ChassisId,
                Level = frame.Level,
                CurrentXp = frame.CurrentXp,
                LifetimeXp = frame.LifetimeXp,
            }
        };

    /// <summary>
    ///     Sets the elite level, elite xp and elite point balance of one battleframe.
    /// </summary>
    /// <param name="characterGuid">The full character guid</param>
    /// <param name="frame">The frame to save, keyed by its chassis sdb id</param>
    /// <returns>The command to send</returns>
    public static Command SaveBattleframeEliteXp(ulong characterGuid, BattleframeProgression frame)
        => new()
        {
            SaveBattleframeEliteXp = new SaveBattleframeEliteXp()
            {
                CharacterGuid = characterGuid,
                BattleframeSdbId = frame.ChassisId,
                EliteLevel = frame.EliteLevel,
                CurrentEliteXp = frame.CurrentEliteXp,
                LifetimeEliteXp = frame.LifetimeEliteXp,
                ElitePoints = frame.ElitePoints,
            }
        };

    /// <summary>
    ///     Records one selected upgrade on one battleframe. Upserts (battleframe, upgrade id) and sets Count
    ///     absolutely, so sending Count = 3 twice leaves 3 and not 6.
    /// </summary>
    /// <param name="characterGuid">The full character guid</param>
    /// <param name="chassisId">The dbitems::RootItem sdb_id of the chassis</param>
    /// <param name="upgrade">The upgrade and how many times it was taken</param>
    /// <returns>The command to send</returns>
    public static Command SaveBattleframeUpgrade(ulong characterGuid, uint chassisId, BattleframeUpgrade upgrade)
        => new()
        {
            SaveBattleframeUpgrade = new SaveBattleframeUpgrade()
            {
                CharacterGuid = characterGuid,
                BattleframeSdbId = chassisId,
                UpgradeId = upgrade.UpgradeId,
                Count = upgrade.Count,
            }
        };

    /// <summary>
    ///     Deletes every selected upgrade on one battleframe. There is no single-upgrade delete.
    /// </summary>
    /// <param name="characterGuid">The full character guid</param>
    /// <param name="chassisId">The dbitems::RootItem sdb_id of the chassis</param>
    /// <returns>The command to send</returns>
    public static Command ClearBattleframeUpgrades(ulong characterGuid, uint chassisId)
        => new()
        {
            ClearBattleframeUpgrades = new ClearBattleframeUpgrades()
            {
                CharacterGuid = characterGuid,
                BattleframeSdbId = chassisId,
            }
        };

    /// <summary>
    ///     Replaces the whole rolled set of upgrades on one battleframe in one transaction. An empty list clears
    ///     it, which is how a roll is consumed once the character has picked from it.
    /// </summary>
    /// <param name="characterGuid">The full character guid</param>
    /// <param name="chassisId">The dbitems::RootItem sdb_id of the chassis</param>
    /// <param name="upgrades">The complete rolled set, not a delta</param>
    /// <returns>The command to send</returns>
    public static Command SaveBattleframeAvailableUpgrades(ulong characterGuid, uint chassisId, IReadOnlyList<BattleframeAvailableUpgrade> upgrades)
    {
        var save = new SaveBattleframeAvailableUpgrades()
        {
            CharacterGuid = characterGuid,
            BattleframeSdbId = chassisId,
        };

        foreach (var upgrade in upgrades)
        {
            var entry = new BattleframeAvailableUpgradeEntry()
            {
                UpgradeIndex = upgrade.UpgradeIndex,
                UpgradeId = upgrade.UpgradeId,
                Category = upgrade.Category,
                StatValue = upgrade.StatValue,
                LocalizationId = upgrade.LocalizationId,
            };

            if (upgrade.Count.HasValue)
            {
                entry.Count = upgrade.Count.Value;
            }

            if (upgrade.ItemSdbId.HasValue)
            {
                entry.ItemSdbId = upgrade.ItemSdbId.Value;
            }

            save.Upgrades.Add(entry);
        }

        return new Command() { SaveBattleframeAvailableUpgrades = save };
    }
}
