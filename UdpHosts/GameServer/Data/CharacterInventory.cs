using System;
using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB;
using GameServer.Entities.Character;
using GameServer.Enums;

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

        foreach(var loadout in HardcodedCharacterData.GetTempAvailableLoadouts())
        {
            _loadouts.Add(loadout.FrameLoadoutId, loadout);
        }
    }

    public void CreateItem(uint sdbId)
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
    
    public void SendFullInventory()
    {
        if (_items.Count > 255)
        {
            throw new NotImplementedException("Too many items in inventory, CharacterInventory.SendFullInventory has to be updated");
        }

        if (_resources.Count > 255)
        {
            throw new NotImplementedException("Too many resources in inventory, CharacterInventory.SendFullInventory has to be updated");
        }

        if (_loadouts.Count > 255)
        {
            throw new NotImplementedException("Too many loadouts in inventory, CharacterInventory.SendFullInventory has to be updated");
        }

        var update = new InventoryUpdate()
        {
            ClearExistingData = 1,
            ItemsPart1Length = (byte)_items.Count,
            ItemsPart1 = _items.Values.ToArray(),
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
}