using System;
using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.GRPC;
using GameServer.Protocol;
using GameServer.StaticDB;
using Serilog;
using GrpcInventoryResponse = GrpcGameServerAPIClient.CharacterInventoryResponse;
using GrpcLoadoutVisual = GrpcGameServerAPIClient.LoadoutVisual;
using LoadoutVisualType = AeroMessages.GSS.Character.LoadoutConfig_Visual.LoadoutVisualType;

namespace GameServer.Data;

/// <summary>
/// One module slot on an item: a slot index and the dbitems::RootItem sdb_id in it.
/// The GSS Item block only carries the sdb ids, so PIN keeps the indices to be able to persist an item
/// without losing them.
/// </summary>
/// <param name="SlotIndex">The module slot</param>
/// <param name="ModuleSdbId">The dbitems::RootItem sdb_id of the module</param>
public readonly record struct InventoryModule(uint SlotIndex, uint ModuleSdbId);

public class CharacterInventory
{
    public const uint PveConfigId = 0;
    public const uint PvpConfigId = 1;

    private static readonly ILogger _logger = Log.ForContext<CharacterEntity>();

    private readonly Dictionary<ulong, Item> _items; // By guid
    private readonly Dictionary<uint, Resource> _resources; // By typeid
    private readonly Dictionary<int, Loadout> _loadouts; // By loadoutid
    private readonly Dictionary<ulong, List<InventoryModule>> _itemModules; // By guid

    private readonly IShard _shard;
    private readonly INetworkClient _player;
    private readonly CharacterEntity _character;

    private bool _persistChanges;

    public CharacterInventory(IShard shard, INetworkClient player, CharacterEntity character)
    {
        _shard = shard;
        _player = player;
        _character = character;
        _items = [];
        _resources = [];
        _loadouts = [];
        _itemModules = [];
    }

    public bool EnablePartialUpdates { get; set; }
    public ulong CharacterGuid => _character.EntityId + (byte)GuidService.AdditionalTypes.Character;

    public static bool IsUsable(GrpcInventoryResponse response) => response is { Loadouts.Count: > 0 };

    public void LoadHardcodedInventory()
    {
        foreach (uint item in HardcodedCharacterData.FallbackInventoryItems)
        {
            CreateItem(item);
        }

        foreach ((uint resource, uint quantity) in HardcodedCharacterData.FallbackInventoryResources)
        {
            AddResource(resource, quantity);
        }

        foreach (var data in HardcodedCharacterData.TempHardcodedLoadouts)
        {
            HardcodedCharacterData.GenerateLoadoutAndItems(this, data);
        }

        foreach ((uint createId, uint chassisId) in HardcodedCharacterData.TempCharCreateLoadouts)
        {
            HardcodedCharacterData.GenerateCharCreateLoadoutAndItems(this, createId, chassisId);
        }
    }

    /// <summary>
    /// Build the inventory from the persisted state of the character.
    /// </summary>
    /// <param name="response">The persisted inventory, or null when it could not be fetched</param>
    /// <returns>True when the inventory was loaded; false when the caller should fall back to the hardcoded one</returns>
    public bool LoadRemote(GrpcInventoryResponse response)
    {
        if (!IsUsable(response))
        {
            return false;
        }

        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var remote in response.Items)
        {
            var modules = new List<InventoryModule>(remote.Modules.Count);
            foreach (var module in remote.Modules)
            {
                modules.Add(new InventoryModule(module.SlotIndex, module.ModuleSdbId));
            }

            _items[remote.Guid] = new Item()
            {
                ChangeType = 0,
                SdbId = remote.SdbId,
                GUID = remote.Guid,
                SubInventory = (byte)remote.SubInventory,
                TimestampEpoch = timestamp,
                DynamicFlags = (byte)remote.DynamicFlags,
                Durability = (ushort)remote.Durability,
                DurabilityPool = 0,
                Unk4 = 0,
                Unk5 = 0,
                Attributes = [],
                Quality = 0,
                Modules = [.. modules.Select(m => m.ModuleSdbId)],
            };

            _itemModules[remote.Guid] = modules;
        }

        foreach (var remote in response.Resources)
        {
            _resources[remote.SdbId] = new Resource()
            {
                SdbId = remote.SdbId,
                TextKey = string.Empty,
                Quantity = remote.Quantity,
                SubInventory = (byte)remote.SubInventory,
                TimestampEpoch = 0,
            };
        }

        var slotsByLoadout = response.Slots.GroupBy(s => s.LoadoutId).ToDictionary(g => g.Key, g => g.ToList());
        var perksByLoadout = response.LoadoutPerks.GroupBy(p => p.LoadoutId).ToDictionary(g => g.Key, g => g.ToList());
        var visualsByConfig = response.Visuals
            .GroupBy(visual => (visual.LoadoutId, visual.ConfigId))
            .ToDictionary(group => group.Key, group => group.SelectMany(entry => entry.Visuals).ToList());

        foreach (var remote in response.Loadouts)
        {
            var pveItems = new List<LoadoutConfig_Item>();
            var pvpItems = new List<LoadoutConfig_Item>();

            if (slotsByLoadout.TryGetValue(remote.LoadoutId, out var slots))
            {
                foreach (var slot in slots)
                {
                    var itemGuid = (ulong)slot.ItemGuid;
                    if (!_items.ContainsKey(itemGuid))
                    {
                        _logger.Warning(
                            "Loadout {LoadoutId} slots item {ItemGuid:X16}, which is not in the inventory of {CharacterId:X16}",
                            remote.LoadoutId,
                            itemGuid,
                            _character.EntityId);
                        continue;
                    }

                    if (slot.SlotIndex > byte.MaxValue)
                    {
                        _logger.Warning(
                            "Loadout {LoadoutId} uses slot index {SlotIndex}, which does not fit the GSS loadout block",
                            remote.LoadoutId,
                            slot.SlotIndex);
                        continue;
                    }

                    var entry = new LoadoutConfig_Item() { ItemGUID = itemGuid, SlotIndex = (byte)slot.SlotIndex };
                    if (slot.ConfigId == PvpConfigId)
                    {
                        pvpItems.Add(entry);
                    }
                    else
                    {
                        pveItems.Add(entry);
                    }
                }
            }

            uint[] perks = perksByLoadout.TryGetValue(remote.LoadoutId, out var perkEntries)
                               ? [.. perkEntries.Select(p => p.PerkItemSdbId)]
                               : [];

            var pveVisuals = ReadStoredVisuals(visualsByConfig, remote.LoadoutId, PveConfigId);
            var pvpVisuals = ReadStoredVisuals(visualsByConfig, remote.LoadoutId, PvpConfigId);

            _loadouts[remote.LoadoutId] = new Loadout()
            {
                PveLoadoutId = remote.LoadoutId,
                PvpLoadoutId = 0,
                LoadoutName = remote.LoadoutName,
                LoadoutType = remote.LoadoutType,
                ChassisID = (uint)remote.ChassisId,
                LoadoutConfigs =
                [
                    new LoadoutConfig()
                    {
                        ConfigID = PveConfigId,
                        ConfigName = "pve",
                        Items = [.. pveItems],
                        Visuals = pveVisuals,
                        Perks = [.. perks],
                        Unk1 = 0,
                        PerkBandwidth = 0,
                        PerkRespecLockRemainingSeconds = 0,
                        HaveExtraData = 0,
                    },
                    new LoadoutConfig()
                    {
                        ConfigID = PvpConfigId,
                        ConfigName = "pvp",
                        Items = [.. pvpItems],
                        Visuals = pvpVisuals,
                        Perks = [.. perks],
                        Unk1 = 0,
                        PerkBandwidth = 0,
                        PerkRespecLockRemainingSeconds = 0,
                        HaveExtraData = 0,
                    },
                ],
            };
        }

        _persistChanges = true;

        _logger.Information(
            "Loaded {Items} items, {Resources} resources, {Loadouts} loadouts, {Perks} perks and {VisualConfigs} painted configs over GRPC for {CharacterId:X16}",
            response.Items.Count,
            response.Resources.Count,
            response.Loadouts.Count,
            response.LoadoutPerks.Count,
            response.Visuals.Count,
            _character.EntityId);

        return true;
    }

    public int GetLoadoutIdForChassis(uint chassisId)
    {
        foreach (var (loadoutId, loadout) in _loadouts)
        {
            if (loadout.ChassisID == chassisId)
            {
                return loadoutId;
            }
        }

        return 0;
    }

    /// <summary>
    /// Any loadout in the inventory, for when the character's current battleframe has no loadout of its own.
    /// </summary>
    /// <returns>The lowest loadout id, or 0 when the inventory has no loadouts</returns>
    public int GetAnyLoadoutId()
        => _loadouts.Count > 0 ? _loadouts.Keys.Min() : 0;

    /// <summary>
    /// Get a loadout from the inventory by loadoutId
    /// </summary>
    /// <param name="loadoutId">The id of the loadout to get</param>
    /// <returns>The loadout data as LoadoutReferenceData or null if the loadoutId was invalid</returns>
    public LoadoutReferenceData GetLoadoutReferenceData(int loadoutId)
    {
        if (!_loadouts.TryGetValue(loadoutId, out var loadout))
        {
            return null;
        }

        var refData = new LoadoutReferenceData()
                      {
                          LoadoutId = loadoutId,
                          ChassisId = loadout.ChassisID
                      };

        var pveConfig = loadout.LoadoutConfigs[0];
        var pvpConfig = loadout.LoadoutConfigs[1];
        foreach (var itemRef in pveConfig.Items)
        {
            var item = _items[itemRef.ItemGUID];
            refData.SlottedItemsPvE.Add((LoadoutSlotType)itemRef.SlotIndex, item.SdbId);
        }

        foreach (var itemRef in pvpConfig.Items)
        {
            var item = _items[itemRef.ItemGUID];
            refData.SlottedItemsPvP.Add((LoadoutSlotType)itemRef.SlotIndex, item.SdbId);
        }

        // A character is equipped from the PvE config, so we use those visuals.
        refData.Visuals = [.. pveConfig.Visuals];

        return refData;
    }

    public ulong CreateItem(uint sdbId)
        => CreateItem(sdbId, GetInventoryTypeByItemTypeId(sdbId));

    /// <summary>
    /// Create an item in an explicit sub-inventory. The static db only ever resolves Bag, Cache
    /// and Gear, so anything else (TempResources, Mail) has to be asked for by the caller.
    /// </summary>
    /// <param name="sdbId">The dbitems::RootItem sdb_id of the item to create</param>
    /// <param name="subInventory">The sub-inventory to create the item in</param>
    /// <returns>The guid of the created item</returns>
    public ulong CreateItem(uint sdbId, InventoryType subInventory)
    {
        ulong guid = _shard.GetNextGuid((byte)GuidService.AdditionalTypes.Item);
        Item item = new Item()
        {
            SdbId = sdbId,
            GUID = guid,
            SubInventory = (byte)subInventory,
            Durability = 1000,
            DynamicFlags = 0,
            TimestampEpoch = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Modules = [],
            ChangeType = 0,
            DurabilityPool = 0,
            Unk4 = 0,
            Unk5 = 0,
            Attributes = [],
            Quality = 0,
        };

        _items.Add(guid, item);
        _itemModules[guid] = [];
        SendItemUpdate(guid);
        PersistItem(guid);
        return guid;
    }

    public void AddResource(uint sdbId, uint quantity)
        => AddResource(sdbId, quantity, GetInventoryTypeByItemTypeId(sdbId));

    /// <summary>
    /// Add a resource to an explicit sub-inventory, see <see cref="CreateItem(uint, InventoryType)"/>.
    /// </summary>
    /// <param name="sdbId">The dbitems::RootItem sdb_id of the resource</param>
    /// <param name="quantity">How much to add</param>
    /// <param name="subInventory">The sub-inventory to hold the resource</param>
    public void AddResource(uint sdbId, uint quantity, InventoryType subInventory)
    {
        if (!_resources.ContainsKey(sdbId))
        {
            Resource resource = new Resource()
            {
                Quantity = 0,
                SdbId = sdbId,
                SubInventory = (byte)subInventory,
                TextKey = string.Empty,
                TimestampEpoch = 0,
            };

            _resources.Add(sdbId, resource);
        }

        var res = _resources[sdbId];
        res.Quantity += quantity;
        _resources[sdbId] = res;
        SendResourceUpdate(sdbId);
        PersistResource(sdbId);
    }

    public bool ConsumeResource(uint sdbId, uint cost)
    {
        if (!_resources.TryGetValue(sdbId, out var res))
        {
            return false;
        }

        if (res.Quantity < cost)
        {
            return false;
        }
        else
        {
            res.Quantity -= cost;

            // A resource that runs out stays at 0
            _resources[sdbId] = res;

            SendResourceUpdate(sdbId);
            PersistResource(sdbId);
            return true;
        }
    }

    public uint GetResourceQuantity(uint sdbId)
    {
        return _resources.TryGetValue(sdbId, out var value) ? value.Quantity : 0;
    }

    public void AddLoadout(Loadout loadout)
    {
        _loadouts.Add(loadout.PveLoadoutId, loadout);
    }

    public void ApplyBattleframeBandwidth(CharacterBattleframes battleframes)
    {
        foreach (var loadout in _loadouts.Values)
        {
            var bandwidth = battleframes.Get(loadout.ChassisID)?.Bandwidth ?? 0;
            for (var config = 0; config < loadout.LoadoutConfigs.Length; config++)
            {
                loadout.LoadoutConfigs[config].PerkBandwidth = bandwidth;
            }
        }
    }

    public void SendFullInventory()
    {
        var update = new InventoryUpdate()
        {
            ClearExistingData = 1,
            Items = [.. _items.Values],
            Resources = [.. _resources.Values],
            Loadouts = [.. _loadouts.Values],
            UpdateMailInventory = 1,
            MailItems = [],
            MailResources = []
        };

        _player.NetChannels[ChannelType.ReliableGss].SendMessage(update, _character.EntityId);
    }

    public void SendItemUpdate(ulong guid)
    {
        if (!EnablePartialUpdates)
        {
            return;
        }

        var item = _items[guid];
        var update = new InventoryUpdate()
        {
            ClearExistingData = 0,
            Items = [item],
            Resources = [],
            Loadouts = [],
            UpdateMailInventory = 1,
            MailItems = [],
            MailResources = []
        };

        _player.NetChannels[ChannelType.ReliableGss].SendMessage(update, _character.EntityId);
    }

    public void SendResourceUpdate(uint sdbId)
    {
        if (!EnablePartialUpdates)
        {
            return;
        }

        var resource = _resources[sdbId];
        var update = new InventoryUpdate()
        {
            ClearExistingData = 0,
            Items = [],
            Resources =
            [
                resource
            ],
            Loadouts = [],
            UpdateMailInventory = 1,
            MailItems = [],
            MailResources = []
        };

        _player.NetChannels[ChannelType.ReliableGss].SendMessage(update, _character.EntityId);
    }

    public void SendEquipmentChanges(ulong oldItemGuid, ulong newItemGuid)
    {
        if (!EnablePartialUpdates)
        {
            return;
        }

        var itemChanges = new Item[]
        {
        };

        if (oldItemGuid != 0)
        {
            var oldItem = _items[oldItemGuid];
            itemChanges = [.. itemChanges, oldItem];
        }

        if (newItemGuid != 0)
        {
            var newItem = _items[newItemGuid];
            itemChanges = [.. itemChanges, newItem];
        }

        var update = new InventoryUpdate()
        {
            ClearExistingData = 0,
            Items = itemChanges,
            Resources = [],
            Loadouts = [.. _loadouts.Values],
            UpdateMailInventory = 1,
            MailItems = [],
            MailResources = []
        };

        _player.NetChannels[ChannelType.ReliableGss].SendMessage(update, _character.EntityId);
    }

    /// <summary>
    ///     Puts an item into one of a loadout's slots, updating the item's equipped flag, the character's current
    ///     loadout, the client's inventory and the db.
    /// </summary>
    /// <param name="loadoutId">The loadout to slot the item into.</param>
    /// <param name="slot">The loadout slot to fill.</param>
    /// <param name="guid">The guid of the item to slot, 0 to empty the slot.</param>
    public void EquipItemByGUID(int loadoutId, LoadoutSlotType slot, ulong guid)
    {
        if (!Enum.IsDefined(slot))
        {
            _logger.Warning(
                "Not equipping item {ItemGuid} into loadout {LoadoutId} slot {Slot}",
                guid,
                loadoutId,
                (byte)slot);

            return;
        }

        ulong changedOldItemGUID = 0;
        ulong changedNewItemGUID = guid;

        // Unequip old Item (if any)
        if (_loadouts[loadoutId].LoadoutConfigs[0].Items.Any((e) => e.SlotIndex == (byte)slot))
        {
            // Set Item to unequipped
            var oldItemGUID = _loadouts[loadoutId].LoadoutConfigs[0].Items.First((e) => e.SlotIndex == (byte)slot).ItemGUID;
            changedOldItemGUID = oldItemGUID;
            var oldItem = _items[oldItemGUID];
            oldItem.DynamicFlags = (byte)(oldItem.DynamicFlags ^ (byte)ItemDynamicFlags.IsEquipped);
            _items[oldItemGUID] = oldItem;

            // Update CurrentLoadout
            _character.CurrentLoadout.SlottedItems[slot] = 0;

            // Update LoadoutConfigs
            _loadouts[loadoutId].LoadoutConfigs[0].Items = [.. _loadouts[loadoutId].LoadoutConfigs[0].Items.Where(e => e.SlotIndex != (byte)slot)];
        }

        // Equip new item (if any)
        if (guid != 0)
        {
            // Update Item to Equipped
            var item = _items[guid];
            item.DynamicFlags = (byte)(item.DynamicFlags | (byte)ItemDynamicFlags.IsEquipped);
            _items[guid] = item;

            // Update CurrentLoadout
            _character.CurrentLoadout.SlottedItems[slot] = item.SdbId;

            // Update LoadoutConfig
            _loadouts[loadoutId].LoadoutConfigs[0].Items = [.. _loadouts[loadoutId].LoadoutConfigs[0].Items, new LoadoutConfig_Item() { ItemGUID = guid, SlotIndex = (byte)slot }];
        }

        // Update StaticInfo when visuals are changed
        var equippedSdbId = (guid != 0) ? _items[guid].SdbId : 0;
        switch (slot)
        {
            case LoadoutSlotType.Glider:
                _character.SetStaticInfo(_character.StaticInfo with { LoadoutGlider = equippedSdbId });
                break;
            case LoadoutSlotType.Vehicle:
                _character.SetStaticInfo(_character.StaticInfo with { LoadoutVehicle = equippedSdbId });
                break;
        }

        SendEquipmentChanges(changedOldItemGUID, changedNewItemGUID);
        PersistEquipmentChange(loadoutId, slot, changedOldItemGUID, guid);
    }

    public void EquipVisualBySdbId(int loadoutId, LoadoutVisualType visual, LoadoutSlotType slot, uint sdb_id)
    {
        // Unequip old item (if any)
        if (_loadouts[loadoutId].LoadoutConfigs[0].Visuals.Any(i => i.VisualType == visual))
        {
            // Update Visuals
            _loadouts[loadoutId].LoadoutConfigs[0].Visuals = [.. _loadouts[loadoutId].LoadoutConfigs[0].Visuals.Where(e => e.VisualType != visual)];
        }

        // Equip new item (if any)
        if (sdb_id != 0)
        {
            // Update Visuals
            _loadouts[loadoutId].LoadoutConfigs[0].Visuals =
            [
                .. _loadouts[loadoutId].LoadoutConfigs[0].Visuals,
                new LoadoutConfig_Visual() { ItemSdbId = sdb_id, VisualType = visual, Data1 = 0, Data2 = 0, Transform = [] },
            ];
            _ = _items.First(e => e.Value.SdbId == sdb_id).Value;
        }

        var equippedGUID = (sdb_id != 0) ? _items.First(e => e.Value.SdbId == sdb_id).Value.GUID : 0;

        PersistLoadoutVisuals(loadoutId, PveConfigId, _loadouts[loadoutId].LoadoutConfigs[0].Visuals);

        EquipItemByGUID(loadoutId, slot, equippedGUID);
    }

    public bool SetLoadoutVisuals(int loadoutId, uint configId, IReadOnlyList<LoadoutConfig_Visual> visuals, out LoadoutConfig_Visual[] applied)
    {
        applied = [];

        if (!_loadouts.TryGetValue(loadoutId, out var loadout))
        {
            _logger.Warning(
                "Visual change for loadout {LoadoutId}, which {CharacterId:X16} does not have",
                loadoutId,
                _character.EntityId);
            return false;
        }

        var configIndex = GetLoadoutConfigIndex(loadout, configId);
        if (configIndex < 0)
        {
            _logger.Warning("Loadout {LoadoutId} has no loadout config {ConfigId}", loadoutId, configId);
            return false;
        }

        var configs = loadout.LoadoutConfigs;

        var bySlot = new Dictionary<(LoadoutVisualType VisualType, uint Region), LoadoutConfig_Visual>();
        foreach (var visual in configs[configIndex].Visuals)
        {
            bySlot[(visual.VisualType, visual.Data1)] = visual;
        }

        foreach (var visual in visuals)
        {
            if (!Enum.IsDefined(visual.VisualType))
            {
                _logger.Warning(
                    "Loadout {LoadoutId} config {ConfigId} sent visual type {VisualType}, which PIN does not know",
                    loadoutId,
                    configId,
                    visual.VisualType);
                continue;
            }

            if (visual.ItemSdbId == 0)
            {
                bySlot.Remove((visual.VisualType, visual.Data1));
            }
            else if (HasNonFiniteTransform(visual.Transform))
            {
                _logger.Warning(
                    "Loadout {LoadoutId} config {ConfigId} sent visual {VisualType} {ItemSdbId} with a transform that is not a number, dropped",
                    loadoutId,
                    configId,
                    visual.VisualType,
                    visual.ItemSdbId);
            }
            else
            {
                bySlot[(visual.VisualType, visual.Data1)] = visual;
            }
        }

        applied = [.. bySlot.OrderBy(entry => entry.Key).Select(entry => entry.Value)];

        // A client that re-sends what the loadout already carries gets a response but no update and no persist.
        if (SameVisuals(configs[configIndex].Visuals, applied))
        {
            return false;
        }

        configs[configIndex].Visuals = applied;

        SendLoadoutChanges();
        PersistLoadoutVisuals(loadoutId, configId, applied);

        return true;
    }

    public void PersistCurrentBattleframe(uint chassisId)
    {
        if (!_persistChanges)
        {
            return;
        }

        _ = GRPCService.SendCommandAsync(InventoryCommands.SaveCurrentBattleframe(CharacterGuid, chassisId));
    }

    private static int GetLoadoutConfigIndex(Loadout loadout, uint configId)
    {
        for (var index = 0; index < loadout.LoadoutConfigs.Length; index++)
        {
            if (loadout.LoadoutConfigs[index].ConfigID == configId)
            {
                return index;
            }
        }

        return -1;
    }

    private static bool SameVisuals(LoadoutConfig_Visual[] before, LoadoutConfig_Visual[] after)
    {
        if (before.Length != after.Length)
        {
            return false;
        }

        for (var index = 0; index < before.Length; index++)
        {
            var a = before[index];
            var b = after[index];

            if (a.VisualType != b.VisualType
                || a.ItemSdbId != b.ItemSdbId
                || a.Data1 != b.Data1
                || a.Data2 != b.Data2
                || !(a.Transform ?? []).SequenceEqual(b.Transform ?? []))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Whether a visual carries a transform value that is not a number.
    /// </summary>
    /// <param name="transform">The floats the client sent, if any</param>
    /// <returns>True when any value is NaN or infinite</returns>
    private static bool HasNonFiniteTransform(float[] transform)
    {
        if (transform == null)
        {
            return false;
        }

        return transform.Any(value => float.IsNaN(value) || float.IsInfinity(value));
    }

    private LoadoutConfig_Visual[] ReadStoredVisuals(
        IReadOnlyDictionary<(int LoadoutId, uint ConfigId), List<GrpcLoadoutVisual>> visualsByConfig,
        int loadoutId,
        uint configId)
    {
        if (!visualsByConfig.TryGetValue((loadoutId, configId), out var stored))
        {
            return [];
        }

        var visuals = new List<LoadoutConfig_Visual>();
        foreach (var entry in stored)
        {
            var visual = InventoryCommands.ToLoadoutVisual(entry);
            if (!Enum.IsDefined(visual.VisualType))
            {
                _logger.Warning(
                    "RIN reports visual type {VisualType} on loadout {LoadoutId} config {ConfigId} of {CharacterId:X16}, which PIN does not know",
                    visual.VisualType,
                    loadoutId,
                    configId,
                    _character.EntityId);
                continue;
            }

            visuals.Add(visual);
        }

        return [.. visuals];
    }

    /// <summary>
    /// Persist an item and its modules. The save command replaces the whole module list, so the slot indices
    /// the item was loaded with have to go back out with it.
    /// </summary>
    /// <param name="guid">The item to persist</param>
    private void PersistItem(ulong guid)
    {
        if (!_persistChanges || !_items.TryGetValue(guid, out var item))
        {
            return;
        }

        _ = GRPCService.SendCommandAsync(InventoryCommands.SaveItem(CharacterGuid, item, _itemModules.GetValueOrDefault(guid, [])));
    }

    private void PersistResource(uint sdbId)
    {
        if (!_persistChanges || !_resources.TryGetValue(sdbId, out var resource))
        {
            return;
        }

        _ = GRPCService.SendCommandAsync(InventoryCommands.SaveResource(CharacterGuid, resource));
    }

    /// <summary>
    /// Persist an equipment change: the slot assignment plus the IsEquipped flag on both items.
    /// </summary>
    /// <param name="loadoutId">The loadout that changed</param>
    /// <param name="slot">The slot that changed</param>
    /// <param name="oldGuid">The item that was taken out of the slot, 0 when the slot was empty</param>
    /// <param name="newGuid">The item that went into the slot, 0 when the slot was cleared</param>
    private void PersistEquipmentChange(int loadoutId, LoadoutSlotType slot, ulong oldGuid, ulong newGuid)
    {
        if (!_persistChanges || !_loadouts.ContainsKey(loadoutId))
        {
            return;
        }

        if (newGuid != 0)
        {
            // Swapping the item in a slot is an upsert on (loadout_id, config_id, slot_index). PIN only ever edits
            // the PvE config, so that is the one that goes back.
            _ = GRPCService.SendCommandAsync(InventoryCommands.SaveLoadoutSlot(CharacterGuid, loadoutId, PveConfigId, (byte)slot, newGuid));
        }
        else
        {
            // Unequipping. The item itself stays in the inventory, it just stops being slotted.
            _ = GRPCService.SendCommandAsync(InventoryCommands.DeleteLoadoutSlot(CharacterGuid, loadoutId, PveConfigId, (byte)slot));
        }

        // Both items changed their IsEquipped flag, and SaveInventoryItem is the only command that stores it
        if (oldGuid != 0)
        {
            PersistItem(oldGuid);
        }

        if (newGuid != 0)
        {
            PersistItem(newGuid);
        }
    }

    private void SendLoadoutChanges()
    {
        if (!EnablePartialUpdates)
        {
            return;
        }

        var update = new InventoryUpdate()
        {
            ClearExistingData = 0,
            Items = [],
            Resources = [],
            Loadouts = [.. _loadouts.Values],
            UpdateMailInventory = 1,
            MailItems = [],
            MailResources = []
        };

        _player.NetChannels[ChannelType.ReliableGss].SendMessage(update, _character.EntityId);
    }

    private void PersistLoadoutVisuals(int loadoutId, uint configId, IReadOnlyList<LoadoutConfig_Visual> visuals)
    {
        if (!_persistChanges)
        {
            return;
        }

        _ = GRPCService.SendCommandAsync(InventoryCommands.SaveLoadoutVisuals(CharacterGuid, loadoutId, configId, visuals));

        _logger.Debug(
            "Sent loadout {LoadoutId} config {ConfigId} of {CharacterId:X16} with {VisualCount} visuals [{Visuals}] to RIN",
            loadoutId,
            configId,
            _character.EntityId,
            visuals.Count,
            string.Join(", ", visuals.Select(visual => $"{visual.VisualType}={visual.ItemSdbId}/{visual.Data1}/{visual.Data2}")));
    }

    /// <summary>
    /// Resolve the sub-inventory an item belongs in.
    /// The dbitems::RootItem.flags Cached bit (0x40) forces Cache. Everything else is decided by the item type.
    /// </summary>
    /// <param name="sdbId">The dbitems::RootItem sdb_id to look up</param>
    /// <returns>The sub-inventory to send for the item</returns>
    private InventoryType GetInventoryTypeByItemTypeId(uint sdbId)
    {
        var itemInfo = SDBInterface.GetRootItem(sdbId);
        if (itemInfo != null)
        {
            if (((ItemFlags)itemInfo.Flags).HasFlag(ItemFlags.Cached))
            {
                return InventoryType.Cache;
            }

            return GetInventoryTypeByItemType(itemInfo.Type);
        }
        else
        {
            return InventoryType.Bag;
        }
    }

    /// <summary>
    /// Sub-inventory for items that are not flagged Cached. Weapon and module types live in
    /// Gear, everything that is carried around lives in Bag.
    /// </summary>
    /// <param name="itemType">The dbitems::RootItem type of the item</param>
    /// <returns>The sub-inventory to send for the item</returns>
    private InventoryType GetInventoryTypeByItemType(byte itemType)
    {
        var result = InventoryType.Bag;
        switch ((ItemType)itemType)
        {
            // Gear, confirmed against retail InventoryUpdate captures.
            case ItemType.Weapon:
            case ItemType.Chassis:
            case ItemType.AbilityModule:
            case ItemType.FrameModule:

            // Same equipment families, not present in the captures.
            case ItemType.Backpack:
            case ItemType.FireModule:
            case ItemType.ScopeModule:
            case ItemType.WeaponModule:
            case ItemType.PerkModule:
            case ItemType.GearUpgrade:
                result = InventoryType.Gear;
                break;

            // Bag, confirmed against retail InventoryUpdate captures.
            case ItemType.Basic:
            case ItemType.ItemModule:
            case ItemType.PaletteModule:
            case ItemType.CraftingStation:
            case ItemType.ResourceItem:
            case ItemType.LockBoxKey:
            case ItemType.TinkerTools:

            // Carried crafting material and recipes, not present in the captures.
            case ItemType.Blueprint:
            case ItemType.CraftingComponent:
            case ItemType.CraftingSubcomponent:
            case ItemType.FabricationRecipe:
            case ItemType.Powerup:

            // Consumables are flagged Cached apart from a handful of db rows.
            case ItemType.Consumable:
                result = InventoryType.Bag;
                break;
            default:
                _logger.Warning("Unknown InventoryType for ItemType {ItemType}, defaulting to {Result}", (ItemType)itemType, result);
                break;
        }

        return result;
    }
}