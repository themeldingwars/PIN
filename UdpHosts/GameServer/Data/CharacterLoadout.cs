using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AeroMessages.GSS.V66.Character;
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

    public static readonly Dictionary<LoadoutSlotType, AbilitySlotType> LoadoutToAbilitySlotMap = new Dictionary<LoadoutSlotType, AbilitySlotType>()
    {
        { LoadoutSlotType.Ability1, AbilitySlotType.Ability1 },
        { LoadoutSlotType.Ability2, AbilitySlotType.Ability2 },
        { LoadoutSlotType.Ability3, AbilitySlotType.Ability3 },
        { LoadoutSlotType.AbilityHKM, AbilitySlotType.AbilityHKM },
        { LoadoutSlotType.GearAuxWeapon, AbilitySlotType.AbilityAux },
        { LoadoutSlotType.GearMedicalSystem, AbilitySlotType.AbilityMedical },
    };

    public Dictionary<LoadoutSlotType, uint> SlottedItems;

    public CharacterLoadout()
    {
        VehicleID = 77087;
        GliderID = 81423;
        ChassisID = 76331;
        BackpackID = 78041;

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

    public uint VehicleID { get; set; }
    public uint GliderID { get; set; }
    public uint ChassisID { get; set; }
    public uint BackpackID { get; set; }

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
}