using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.dbcharacter;
using GameServer.Enums.Visuals;

namespace GameServer.Data;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "Self explanatory")]
public enum LoadoutSlotType
{
    Primary = 1,
    Secondary = 2,
    AbilityHKM = 6,
    Ability1 = 7,
    Ability2 = 8,
    Ability3 = 9,
    Backpack = 11,
    GearTorso = 116,
    GearAuxWeapon = 122,
    GearMedicalSystem = 123,
    GearHead = 124,
    GearArms = 126,
    GearLegs = 127,
    GearReactor = 128,
    GearOS = 129,
    GearGadget1 = 130,
    GearGadget2 = 137
}

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "Self explanatory")]
public enum AbilitySlotType
{
    Ability1 = 0,
    Ability2 = 1,
    Ability3 = 2,
    AbilityHKM = 3,
    AbilityInteract = 4,
    AbilityAux = 5,
    AbilityMedical = 6,
    AbilitySIN = 13,
    AbilityCalldownVehicle = 16,
    AbilityCalldownGlider = 17
}

public class CharacterLoadout
{
    public static readonly LoadoutSlotType[] LoadoutAbilitySlots =
    {
        LoadoutSlotType.Ability1,
        LoadoutSlotType.Ability2,
        LoadoutSlotType.Ability3,
        LoadoutSlotType.AbilityHKM,
        LoadoutSlotType.GearAuxWeapon,
        LoadoutSlotType.GearMedicalSystem
    };

    public static readonly LoadoutSlotType[] LoadoutChassisSlots =
    {
        LoadoutSlotType.GearTorso,
        LoadoutSlotType.GearAuxWeapon,
        LoadoutSlotType.GearMedicalSystem,
        LoadoutSlotType.GearHead,
        LoadoutSlotType.GearArms,
        LoadoutSlotType.GearLegs,
        LoadoutSlotType.GearReactor,
        LoadoutSlotType.GearOS,
        LoadoutSlotType.GearGadget1,
        LoadoutSlotType.GearGadget2
    };

    public static readonly LoadoutSlotType[] LoadoutWeaponSlots =
    {
        LoadoutSlotType.Primary,
        LoadoutSlotType.Secondary,
    };

    public static readonly Dictionary<LoadoutSlotType, AbilitySlotType> LoadoutToAbilitySlotMap = new Dictionary<LoadoutSlotType, AbilitySlotType>()
    {
        { LoadoutSlotType.Ability1, AbilitySlotType.Ability1 },
        { LoadoutSlotType.Ability2, AbilitySlotType.Ability2 },
        { LoadoutSlotType.Ability3, AbilitySlotType.Ability3 },
        { LoadoutSlotType.AbilityHKM, AbilitySlotType.AbilityHKM },
        { LoadoutSlotType.GearAuxWeapon, AbilitySlotType.AbilityAux },
        { LoadoutSlotType.GearMedicalSystem, AbilitySlotType.AbilityMedical },
    };
    public static readonly Dictionary<AbilitySlotType, LoadoutSlotType> AbilityToLoadoutSlotMap = new Dictionary<AbilitySlotType, LoadoutSlotType>()
    {
        { AbilitySlotType.Ability1, LoadoutSlotType.Ability1 },
        { AbilitySlotType.Ability2, LoadoutSlotType.Ability2 },
        { AbilitySlotType.Ability3, LoadoutSlotType.Ability3 },
        { AbilitySlotType.AbilityHKM, LoadoutSlotType.AbilityHKM },
        { AbilitySlotType.AbilityAux, LoadoutSlotType.GearAuxWeapon },
        { AbilitySlotType.AbilityMedical, LoadoutSlotType.GearMedicalSystem },
    };

    public Dictionary<LoadoutSlotType, uint> SlottedItems = new Dictionary<LoadoutSlotType, uint>();

    public CharacterLoadout(uint chassisId, uint loadoutId)
    {
        LoadoutID = loadoutId;
        VehicleID = 77087;
        GliderID = 81423;
        ChassisID = chassisId;
        BackpackID = SDBUtils.GetChassisDefaultBackpack(ChassisID);

        ChassisWarpaint = SDBUtils.GetChassisWarpaint(ChassisID, 0, 0, 0, 0);

        // Temp load some default equipment :)
        bool forceFallback = false;
        var defaultSlots = SDBUtils.GetChassisDefaultLoadoutSlots(ChassisID);
        if (defaultSlots != null && !forceFallback)
        {
            foreach (LoadoutSlotType slot in defaultSlots.Keys)
            {
                if (LoadoutAbilitySlots.Contains(slot) || LoadoutChassisSlots.Contains(slot) || LoadoutWeaponSlots.Contains(slot))
                {
                    CharCreateLoadoutSlots record = defaultSlots.GetValueOrDefault((byte)slot);
                    if (record.DefaultPveModule != 0)
                    {
                        SlottedItems.Add(slot, record.DefaultPveModule);
                    }
                    else if (record.DefaultPvpModule != 0)
                    {
                        SlottedItems.Add(slot, record.DefaultPvpModule);
                    }
                }
            }
        }
        else
        {
            SlottedItems.Add(LoadoutSlotType.Primary, 134616);
            SlottedItems.Add(LoadoutSlotType.Secondary, 114316);
            SlottedItems.Add(LoadoutSlotType.AbilityHKM, 113931);
            SlottedItems.Add(LoadoutSlotType.Ability1, 143330);
            SlottedItems.Add(LoadoutSlotType.Ability2, 136056);
            SlottedItems.Add(LoadoutSlotType.Ability3, 113552);
            SlottedItems.Add(LoadoutSlotType.GearTorso, 126575);
            SlottedItems.Add(LoadoutSlotType.GearAuxWeapon, 129458);
            SlottedItems.Add(LoadoutSlotType.GearMedicalSystem, 129056);
            SlottedItems.Add(LoadoutSlotType.GearHead, 125845);
            SlottedItems.Add(LoadoutSlotType.GearArms, 128036);
            SlottedItems.Add(LoadoutSlotType.GearLegs, 128766);
            SlottedItems.Add(LoadoutSlotType.GearReactor, 127306);
            SlottedItems.Add(LoadoutSlotType.GearOS, 129202);
            SlottedItems.Add(LoadoutSlotType.GearGadget1, 142078);
            SlottedItems.Add(LoadoutSlotType.GearGadget2, 130419);
        }
    }

    public uint LoadoutID { get; set; }
    public uint VehicleID { get; set; }
    public uint GliderID { get; set; }
    public uint ChassisID { get; set; }
    public uint BackpackID { get; set; }
    public ChassisWarpaintResult ChassisWarpaint { get; set; }

    public VisualsBlock GetChassisVisuals()
    {
        return new VisualsBlock
        {
            Decals = Array.Empty<VisualsDecalsBlock>(),
            Gradients = ChassisWarpaint.Gradients,
            Colors = ChassisWarpaint.Colors,
            Palettes = ChassisWarpaint.Palettes,
            Patterns = Array.Empty<VisualsPatternBlock>(),
            OrnamentGroupIds = Array.Empty<uint>(),
            CziMapAssetIds = Array.Empty<uint>(),
            MorphWeights = Array.Empty<HalfFloat>(),
            Overlays = Array.Empty<VisualsOverlayBlock>()
        };
    }

    public uint GetAbilityModuleIdBySlotIndex(byte slotIndex)
    {
        var slotType = (AbilitySlotType)slotIndex;
        var loadoutSlot = AbilityToLoadoutSlotMap.GetValueOrDefault(slotType);
        if (loadoutSlot == 0)
        {
            return 0;
        }

        return SlottedItems.GetValueOrDefault(loadoutSlot);
    }

    public SlottedModule[] GetBackpackModules()
    {
        return SlottedItems
        .Where(slotted => LoadoutAbilitySlots.Contains(slotted.Key))
        .Select((slotted) =>
        {
            return new SlottedModule
            {
                SdbId = slotted.Value,
                SlotIndex = (byte)LoadoutToAbilitySlotMap[slotted.Key],
                Flags = 0,
                Unk2 = 0,
            };
        })
        .ToArray();
    }

    public SlottedModule[] GetChassisModules()
    {
        return SlottedItems
        .Where(slotted => LoadoutChassisSlots.Contains(slotted.Key))
        .Select((slotted) =>
        {
            return new SlottedModule
            {
                SdbId = slotted.Value,
                SlotIndex = 0xff,
                Flags = 0,
                Unk2 = 0,
            };
        })
        .ToArray();
    }

    public AeroMessages.GSS.V66.StatsData[] GetItemAttributes()
    {
        var result = new Dictionary<ushort, float>()
        {
            // Base stats that should probably be collected elsewhere
            { 5, 75 }, // Jet Energy Recharge
            { 6, 100 }, // Health
            { 7, 3.75f }, // Health Regen
            { 12, 10 }, // Run Speed
            { 35, 500 }, // Jet Energy
            { 37, 1.75f }, // Jump Height
            { 1121, 150 }, // Jet Spring Cost
            { 1451, 100 }, // Power Rating
            { 1377, 130 }, // Sprint Speed
        };

        foreach (var pair in SlottedItems)
        {
            if (LoadoutAbilitySlots.Contains(pair.Key) || LoadoutChassisSlots.Contains(pair.Key))
            {
                var itemAttributes = SDBInterface.GetItemAttributeRange(pair.Value);
                foreach (var range in itemAttributes.Values)
                {
                    if (result.ContainsKey(range.AttributeId))
                    {
                        result[range.AttributeId] += range.Base;
                    }
                    else
                    {
                        result.Add(range.AttributeId, range.Base);
                    }
                }

            }
        }

        return result
        .Select((pair) => {
            return new StatsData()
            {
                Id = pair.Key,
                Value = pair.Value
            };
        })
        .ToArray();
    }
}