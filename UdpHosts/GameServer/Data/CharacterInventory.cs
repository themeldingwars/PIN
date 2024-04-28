using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.dbcharacter;
using GameServer.Entities.Character;
using GameServer.Enums;
using LoadoutVisualType = AeroMessages.GSS.V66.Character.LoadoutConfig_Visual.LoadoutVisualType;

namespace GameServer.Data;

public class CharacterInventory
{
    public bool EnablePartialUpdates = false;

    private Dictionary<ulong, Item> _items; // By guid
    private Dictionary<uint, Resource> _resources; // By typeid
    private Dictionary<uint, Loadout> _loadouts; // By loadoutid

    private IShard _shard;
    private INetworkClient _player;
    private CharacterEntity _character;

    public CharacterInventory(IShard shard, INetworkClient player, CharacterEntity character)
    {
        _shard = shard;
        _player = player;
        _character = character;
        _items = new();
        _resources = new();
        _loadouts = new();  
    }

    public void LoadHardcodedInventory()
    {
        foreach(uint item in HardcodedCharacterData.FallbackInventoryItems)
        {
            CreateItem(item);
        }

        foreach ((uint resource, uint quantity) in HardcodedCharacterData.FallbackInventoryResources)
        {
            AddResource(resource, quantity);
        }

        foreach(var data in HardcodedCharacterData.TempHardcodedLoadouts)
        {
            HardcodedCharacterData.GenerateLoadoutAndItems(this, data);
        }

        foreach((uint createId, uint chassisId) in HardcodedCharacterData.TempCharCreateLoadouts)
        {
            HardcodedCharacterData.GenerateCharCreateLoadoutAndItems(this, createId, chassisId);
        }
    }

    public uint GetLoadoutIdForChassis(uint chassisId)
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
    /// Get a loadout from the inventory by loadoutId
    /// </summary>
    /// <param name="loadoutId">The id of the loadout to get</param>
    /// <returns>The loadout data as LoadoutReferenceData or null if the loadoutId was invalid</returns>
    public LoadoutReferenceData GetLoadoutReferenceData(uint loadoutId)
    {
        if (!_loadouts.ContainsKey(loadoutId))
        {
            return null;
        }

        var loadout = _loadouts[loadoutId];

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

        return refData;
    }

    public ulong CreateItem(uint sdbId)
    {
        ulong guid = _shard.GetNextGuid((byte)GuidService.AdditionalTypes.Item);
        Item item = new Item()
        {
            SdbId = sdbId,
            GUID = guid,
            SubInventory = GetInventoryTypeByItemTypeId(sdbId),
            Durability = 1000,
            DynamicFlags = 0,
            TimestampEpoch = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Modules = Array.Empty<uint>(),
            Unk1 = 0,
            Unk3 = 0,
            Unk4 = 0,
            Unk5 = 0,
            Unk6 = Array.Empty<ItemUnkData>(),
            Unk7 = 0,
        };

        _items.Add(guid, item);
        SendItemUpdate(guid);
        return guid;
    }

    public void AddResource(uint sdbId, uint quantity)
    {
        if (!_resources.ContainsKey(sdbId))
        {
            Resource resource = new Resource()
            {
                Quantity = 0,
                SdbId = sdbId,
                SubInventory = GetInventoryTypeByItemTypeId(sdbId),
                TextKey = string.Empty,
                Unk2 = 0,
            };

            _resources.Add(sdbId, resource);
        }

        var res = _resources[sdbId];
        res.Quantity += quantity;
        _resources[sdbId] = res;
        SendResourceUpdate(sdbId);
    }

    public bool ConsumeResource(uint sdbId, uint cost)
    {
        if (!_resources.ContainsKey(sdbId))
        {
            return false;
        }

        var res = _resources[sdbId];
        if (res.Quantity < cost)
        {
            return false;
        }
        else
        {
            res.Quantity -= cost;
            
            if (res.Quantity > 0)
            {
                _resources[sdbId] = res;
            }
            else
            {
                _resources.Remove(sdbId);
            }
            
            SendResourceUpdate(sdbId);
            return true;
        }
    }

    public void AddLoadout(Loadout loadout)
    {
        _loadouts.Add(loadout.FrameLoadoutId, loadout);
    }

    public void SendFullInventory()
    {
        if (_items.Count > ((255 * 3) - 1))
        {
            throw new NotImplementedException("Too many items in inventory, CharacterInventory.SendFullInventory has to be updated");
        }

        if (_resources.Count > 254)
        {
            throw new NotImplementedException("Too many resources in inventory, CharacterInventory.SendFullInventory has to be updated");
        }

        if (_loadouts.Count > 254)
        {
            throw new NotImplementedException("Too many loadouts in inventory, CharacterInventory.SendFullInventory has to be updated");
        }

        var update = new InventoryUpdate()
        {
            ClearExistingData = 1,
            ItemsPart1Length = 0,
            ItemsPart1 = Array.Empty<Item>(),
            ItemsPart2Length = 0,
            ItemsPart2 = Array.Empty<Item>(),
            ItemsPart3Length = 0,
            ItemsPart3 = Array.Empty<Item>(),
            Resources = _resources.Values.ToArray(),
            Loadouts = _loadouts.Values.ToArray(),
            Unk = 1,
            SecondItems = Array.Empty<Item>(),
            SecondResources = Array.Empty<Resource>()
        };

        if (_items.Count >= 255)
        {
            var tmp = _items.Values.ToArray();
            update.ItemsPart1Length = 255;
            update.ItemsPart1Full = tmp[0..255];
            if (_items.Count >= 510)
            {
                update.ItemsPart2Length = 255;
                update.ItemsPart2Full = tmp[255..510];

                update.ItemsPart3Length = (byte)tmp[510..^0].Length;
                update.ItemsPart3 = tmp[510..^0];
            }
            else
            {
                update.ItemsPart2Length = (byte)tmp[255..^0].Length;
                update.ItemsPart2 = tmp[255..^0];
            }
        }
        else
        {
            update.ItemsPart1Length = (byte)_items.Count;
            update.ItemsPart1 = _items.Values.ToArray();
        }

        _player.NetChannels[ChannelType.ReliableGss].SendIAero(update, _character.EntityId);
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
            ItemsPart1Length = 1,
            ItemsPart1 = 
            [
                item
            ],
            ItemsPart2Length = 0,
            ItemsPart2 = Array.Empty<Item>(),
            ItemsPart3Length = 0,
            ItemsPart3 = Array.Empty<Item>(),
            Resources = Array.Empty<Resource>(),
            Loadouts = Array.Empty<Loadout>(),
            Unk = 1,
            SecondItems = Array.Empty<Item>(),
            SecondResources = Array.Empty<Resource>()
        };

        _player.NetChannels[ChannelType.ReliableGss].SendIAero(update, _character.EntityId);
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
            ItemsPart1Length = 0,
            ItemsPart1 = Array.Empty<Item>(),
            ItemsPart2Length = 0,
            ItemsPart2 = Array.Empty<Item>(),
            ItemsPart3Length = 0,
            ItemsPart3 = Array.Empty<Item>(),
            Resources =
            [
                resource
            ],
            Loadouts = Array.Empty<Loadout>(),
            Unk = 1,
            SecondItems = Array.Empty<Item>(),
            SecondResources = Array.Empty<Resource>()
        };

        _player.NetChannels[ChannelType.ReliableGss].SendIAero(update, _character.EntityId);
    }

    private byte GetInventoryTypeByItemTypeId(uint sdbId)
    {
        var itemInfo = SDBInterface.GetRootItem(sdbId);
        if (itemInfo != null)
        {
            return GetInventoryTypeByItemType(itemInfo.Type);
        }
        else
        {
            return (byte)InventoryType.Bag;
        }        
    }

    private byte GetInventoryTypeByItemType(byte itemType)
    {
        var result = InventoryType.Bag;
        switch ((ItemType)itemType)
        {
            case ItemType.TinkerTools:
                result = InventoryType.Bag;
                break;
            case ItemType.ItemModule:
                result = InventoryType.Bag;
                break;
            case ItemType.PaletteModule:
                result = InventoryType.Bag;
                break;
            case ItemType.CraftingStation:
                result = InventoryType.Bag;
                break;
            case ItemType.ResourceItem:
                result = InventoryType.Bag;
                break;
            case ItemType.LockBoxKey:
                result = InventoryType.Bag;
                break;
            case ItemType.Basic: // NOTE: There are some basic type items and resources that go into cache
                result = InventoryType.Bag;
                break;
            case ItemType.Consumable:
                result = InventoryType.Cache;
                break;
            case ItemType.AbilityModule:
                result = InventoryType.Gear;
                break;
            case ItemType.FrameModule:
                result = InventoryType.Gear;
                break;
            case ItemType.Weapon:
                result = InventoryType.Gear;
                break;
            case ItemType.Chassis:
                result = InventoryType.Gear;
                break;
            default:
                Console.WriteLine($"Unknown InventoryType for ItemType {(ItemType)itemType}, defaulting to {result}");
                break;
        }

        return (byte)result;
    }

    public void EquipItemByGUID(int loadoutId, LoadoutSlotType slot, ulong guid)
    {
        //var currentItem = _character.CurrentLoadout.SlottedItems[slot]; // SdbId
        
        if (guid != 0)
        {
            var item = _items[guid];
            _character.CurrentLoadout.SlottedItems[slot] = item.SdbId;
        }
        else
        {
            _character.CurrentLoadout.SlottedItems[slot] = 0;
        }
        
        var loadoutItems = _loadouts[(uint)loadoutId].LoadoutConfigs[0].Items; //PvE Loadout

        if (loadoutItems.Any(i => i.SlotIndex == (byte)slot))
        {
            for (int i = 0; i < loadoutItems.Length; i++)
            {
                if (loadoutItems[i].SlotIndex == (byte) slot)
                {
                    loadoutItems[i].ItemGUID = guid;
                    break;
                }
            }
        }
        else
        {
            _loadouts[(uint)loadoutId].LoadoutConfigs[0].Items = loadoutItems.Append(new LoadoutConfig_Item() { ItemGUID = guid, SlotIndex = (byte)slot }).ToArray();
        }

        switch (slot)
        {
            case LoadoutSlotType.Glider:
                _character.StaticInfo = _character.StaticInfo with { LoadoutGlider = _items[guid].SdbId };
                break;
            case LoadoutSlotType.Vehicle:
                _character.StaticInfo = _character.StaticInfo with { LoadoutVehicle = _items[guid].SdbId };
                break;
        }

        // Ideally, the game would remove/hide the item from your inventory, but when I do that (and recreate the removed item)
        // the targeted slot becomes empty.
        //
        // _items.Remove(guid);
        // var itemToStore = CreateItem(currentItem);
        
        SendFullInventory(); 
    }
    
    public void EquipVisualBySdbId(uint loadoutId, LoadoutVisualType visual, LoadoutSlotType slot, uint sdb_id)
    {
        //var currentItem = _character.CurrentLoadout.SlottedItems[slot]; // SdbId
        var item = _items.First(e => e.Value.SdbId == sdb_id).Value;//CreateItem(sdb_id);
        
        var loadoutVisuals = _loadouts[(uint)loadoutId].LoadoutConfigs[0].Visuals; //PvE Loadout

        if (loadoutVisuals.Any(i => i.VisualType == visual))
        {
            for (int i = 0; i < loadoutVisuals.Length; i++)
            {
                if (loadoutVisuals[i].VisualType == visual)
                {
                    loadoutVisuals[i].ItemSdbId = sdb_id;
                    break;
                }
            }
        }
        else
        {
            _loadouts[(uint)loadoutId].LoadoutConfigs[0].Visuals = loadoutVisuals.Append(new LoadoutConfig_Visual() { ItemSdbId = sdb_id, VisualType = visual }).ToArray();
        }
        EquipItemByGUID((int) loadoutId, slot, item.GUID);

        switch (slot)
        {
            case LoadoutSlotType.Glider:
                Console.WriteLine("Setting Glider");
                _character.SetStaticInfo(_character.StaticInfo with
                                         {
                                             LoadoutGlider = sdb_id,
                                         });
                break;
            case LoadoutSlotType.Vehicle:
                _character.SetStaticInfo(_character.StaticInfo with
                                         {
                                             LoadoutVehicle = sdb_id,
                                         });
                break;
        }
        // Ideally, the game would remove/hide the item from your inventory, but when I do that (and recreate the removed item)
        // the targeted slot becomes empty.
        //
        // _items.Remove(guid);
        // var itemToStore = CreateItem(currentItem);

        //SendFullInventory(); 
    }
}