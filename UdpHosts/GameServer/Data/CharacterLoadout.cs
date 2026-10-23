using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.GSS;
using AeroMessages.GSS.Character;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;
using Serilog;
using LoadoutVisualType = AeroMessages.GSS.Character.LoadoutConfig_Visual.LoadoutVisualType;

namespace GameServer.Data;

public enum LoadoutSlotType : byte
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
    GearGadget2 = 137,
    Vehicle = 157,
    Glider = 158,
}

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
    // The Usage field of a pattern or decal carries which chassis region it paints, counted from zero:
    // 0 armor, 1 bodysuit, 2 weapon. 255 says there is no region, which is what the retail wire carries for an
    // empty pattern slot and for every decal, a decal being placed by its own transform rather than by region.
    public const byte NoRegionUsage = 255;

    public static readonly LoadoutSlotType[] LoadoutAbilitySlots =
    [
        LoadoutSlotType.Ability1,
        LoadoutSlotType.Ability2,
        LoadoutSlotType.Ability3,
        LoadoutSlotType.AbilityHKM,
        LoadoutSlotType.GearAuxWeapon,
        LoadoutSlotType.GearMedicalSystem
    ];

    public static readonly LoadoutSlotType[] LoadoutChassisSlots =
    [
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
    ];

    public static readonly LoadoutSlotType[] LoadoutWeaponSlots =
    [
        LoadoutSlotType.Primary,
        LoadoutSlotType.Secondary,
    ];

    public static readonly Dictionary<LoadoutSlotType, AbilitySlotType> LoadoutToAbilitySlotMap = new()
    {
        { LoadoutSlotType.Ability1, AbilitySlotType.Ability1 },
        { LoadoutSlotType.Ability2, AbilitySlotType.Ability2 },
        { LoadoutSlotType.Ability3, AbilitySlotType.Ability3 },
        { LoadoutSlotType.AbilityHKM, AbilitySlotType.AbilityHKM },
        { LoadoutSlotType.GearAuxWeapon, AbilitySlotType.AbilityAux },
        { LoadoutSlotType.GearMedicalSystem, AbilitySlotType.AbilityMedical },
    };
    public static readonly Dictionary<AbilitySlotType, LoadoutSlotType> AbilityToLoadoutSlotMap = new()
    {
        { AbilitySlotType.Ability1, LoadoutSlotType.Ability1 },
        { AbilitySlotType.Ability2, LoadoutSlotType.Ability2 },
        { AbilitySlotType.Ability3, LoadoutSlotType.Ability3 },
        { AbilitySlotType.AbilityHKM, LoadoutSlotType.AbilityHKM },
        { AbilitySlotType.AbilityAux, LoadoutSlotType.GearAuxWeapon },
        { AbilitySlotType.AbilityMedical, LoadoutSlotType.GearMedicalSystem },
        { AbilitySlotType.AbilityCalldownVehicle, LoadoutSlotType.Vehicle },
        { AbilitySlotType.AbilityCalldownGlider, LoadoutSlotType.Glider }
    };

    public Dictionary<LoadoutSlotType, uint> SlottedItems = [];

    // Module and Character Scalars is an assumption that has not been completely confirmed. I'm not sure what the exact rule for separation between the two categories is.
    public Dictionary<ushort, float> ItemAttributes = [];
    public Dictionary<ushort, float> ItemModuleScalars = [];
    public Dictionary<ushort, float> ItemCharacterScalars = [];

    private static readonly Dictionary<string, LoadoutSlotType> _slotNames = new(StringComparer.OrdinalIgnoreCase)
    {
        { "primary", LoadoutSlotType.Primary },
        { "primary_weapon", LoadoutSlotType.Primary },
        { "secondary", LoadoutSlotType.Secondary },
        { "secondary_weapon", LoadoutSlotType.Secondary },
        { "aux_weapon", LoadoutSlotType.GearAuxWeapon },
        { "auxweapon", LoadoutSlotType.GearAuxWeapon },
        { "medical", LoadoutSlotType.GearMedicalSystem },
        { "medical_system", LoadoutSlotType.GearMedicalSystem },
        { "torso", LoadoutSlotType.GearTorso },
        { "head", LoadoutSlotType.GearHead },
        { "arms", LoadoutSlotType.GearArms },
        { "legs", LoadoutSlotType.GearLegs },
        { "reactor", LoadoutSlotType.GearReactor },
        { "os", LoadoutSlotType.GearOS },
        { "gadget1", LoadoutSlotType.GearGadget1 },
        { "gadget2", LoadoutSlotType.GearGadget2 },
        { "backpack", LoadoutSlotType.Backpack },
        { "ability1", LoadoutSlotType.Ability1 },
        { "ability2", LoadoutSlotType.Ability2 },
        { "ability3", LoadoutSlotType.Ability3 },
        { "abilityhkm", LoadoutSlotType.AbilityHKM },
        { "ability_hkm", LoadoutSlotType.AbilityHKM },
        { "vehicle", LoadoutSlotType.Vehicle },
        { "glider", LoadoutSlotType.Glider },
    };

    private static readonly Dictionary<ushort, float> _fallbackAttributes = new()
    {
        // We fill these in if we somehow don't have them to ensure a playable experience (0 run speed = zzz)
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

    private static readonly Dictionary<ushort, float> _fallbackModuleScalars = [];

    private static readonly Dictionary<ushort, float> _fallbackCharacterScalars = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterLoadout"/> class.
    /// </summary>
    public CharacterLoadout()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterLoadout"/> class.
    /// </summary>
    /// <param name="refData">Data to setup the loadout with</param>
    public CharacterLoadout(LoadoutReferenceData refData)
    {
        InitFromLoadoutReferenceData(refData);
    }

    public int LoadoutID { get; set; }
    public uint VehicleID { get; set; }
    public uint GliderID { get; set; }
    public uint ChassisID { get; set; }
    public uint BackpackID { get; set; }
    public uint ChassisChangeTime { get; set; }
    public ChassisWarpaintResult ChassisWarpaint { get; set; }
    public WarpaintPatternData ChassisWarpaintPatterns { get; set; }
    public WarpaintTatooData ChassisWarpaintTatoos { get; set; }

    public float GearHealthAttribute { get; set; }

    /// <summary>
    /// Parses a friendly slot name (e.g. "primary_weapon") into a <see cref="LoadoutSlotType"/>.
    /// </summary>
    /// <param name="name">The friendly slot name to parse.</param>
    /// <param name="slot">Receives the parsed slot type when the name is recognized.</param>
    /// <returns>True when the name maps to a known slot; false otherwise.</returns>
    public static bool TryParseSlot(string name, out LoadoutSlotType slot)
    {
        return _slotNames.TryGetValue(name, out slot);
    }

    /// <summary>
    /// Gets the item type accepted by a loadout slot, or null if the slot does not accept items.
    /// </summary>
    /// <param name="slot">The loadout slot to query.</param>
    /// <returns>The item type the slot accepts, or null when the slot does not accept items.</returns>
    public static ItemType? GetAcceptedItemType(LoadoutSlotType slot)
    {
        if (LoadoutWeaponSlots.Contains(slot))
        {
            return ItemType.Weapon;
        }

        if (LoadoutAbilitySlots.Contains(slot) || slot is LoadoutSlotType.Vehicle or LoadoutSlotType.Glider)
        {
            return ItemType.AbilityModule;
        }

        if (slot == LoadoutSlotType.Backpack)
        {
            return ItemType.Backpack;
        }

        if (LoadoutChassisSlots.Contains(slot))
        {
            return ItemType.FrameModule;
        }

        return null;
    }

    public VisualsBlock GetChassisVisuals()
    {
        return new VisualsBlock
        {
            Decals = ChassisWarpaintTatoos.ToAero(),
            Gradients = ChassisWarpaint.Gradients,
            Colors = ChassisWarpaint.Colors,
            Palettes = ChassisWarpaint.Palettes,
            Patterns = ChassisWarpaintPatterns.ToAero(),
            OrnamentGroupIds = [],
            CziMapAssetIds = [],
            MorphWeights = [],
            Overlays = []
        };
    }

    /// <summary>
    ///     Rebuild the chassis warpaint, patterns and decals from the visuals slotted on the loadout, the way the
    ///     monster visuals path builds them from the Monster row.
    ///     An empty list leaves the battleframe on its dbitems::Battleframe defaults.
    /// </summary>
    /// <param name="visuals">The visuals slotted on the loadout config</param>
    public void ApplyVisuals(IReadOnlyList<LoadoutConfig_Visual> visuals)
    {
        var fullbody = 0u;
        var armor = 0u;
        var bodysuit = 0u;
        var glow = 0u;
        var patterns = new List<WarpaintPattern>();
        var tatoos = new List<WarpaintTatoo>();

        foreach (var visual in visuals)
        {
            switch (visual.VisualType)
            {
                case LoadoutVisualType.Palette:
                    var palette = SDBInterface.GetWarpaintPalette(visual.ItemSdbId);
                    if (palette == null)
                    {
                        Log.Warning("Loadout {LoadoutId} slots palette item {SdbId}, which has no dbvisualrecords::WarpaintPalette row", LoadoutID, visual.ItemSdbId);
                        break;
                    }

                    // A fullbody palette paints all seven colors, so it takes every palette slot. Leaving one to the
                    // battleframe default would let that default overwrite part of the pick: GetChassisWarpaint
                    // blends fullbody first and the narrower palettes after it.
                    if ((palette.TypeFlags & SDBUtils.FullbodyPaletteBit) != 0)
                    {
                        fullbody = palette.Id;
                        armor = palette.Id;
                        bodysuit = palette.Id;
                        glow = palette.Id;
                        break;
                    }

                    var slotted = false;
                    if ((palette.TypeFlags & SDBUtils.ArmorPaletteBit) != 0)
                    {
                        armor = palette.Id;
                        slotted = true;
                    }

                    if ((palette.TypeFlags & SDBUtils.BodysuitPaletteBit) != 0)
                    {
                        bodysuit = palette.Id;
                        slotted = true;
                    }

                    if ((palette.TypeFlags & SDBUtils.GlowPaletteBit) != 0)
                    {
                        glow = palette.Id;
                        slotted = true;
                    }

                    if (!slotted)
                    {
                        Log.Warning("WarpaintPalette {PaletteId} on loadout {LoadoutId} has no armor, bodysuit or glow region flag ({TypeFlags})", palette.Id, LoadoutID, palette.TypeFlags);
                    }

                    break;

                case LoadoutVisualType.Pattern:
                    var region = ReadPatternRegion(visual.Data1);
                    if (region == NoRegionUsage)
                    {
                        Log.Debug("Loadout {LoadoutId} slots pattern {SdbId} on region {Region}, which is not a chassis region PIN knows", LoadoutID, visual.ItemSdbId, visual.Data1);
                    }

                    var transform = ReadPatternTransform(visual.Transform);

                    if (Math.Max(Math.Max(transform.Y, transform.Z), transform.W) > WarpaintPatternData.WireMaxValue)
                    {
                        Log.Warning("Loadout {LoadoutId} pattern {SdbId} carries transform {Transform}, and the visuals block holds scale and offsets as fixed point topping out at {Max}", LoadoutID, visual.ItemSdbId, transform, WarpaintPatternData.WireMaxValue);
                    }

                    patterns.Add(new WarpaintPattern
                    {
                        Id = visual.ItemSdbId,
                        Transform = transform,
                        Usage = region,
                    });
                    break;

                case LoadoutVisualType.Decal:
                    tatoos.Add(new WarpaintTatoo
                    {
                        Id = visual.ItemSdbId,
                        Color = visual.Data2,
                        Transform = ReadVisualTransform(visual.Transform, 3),
                        Usage = NoRegionUsage,
                    });
                    break;
            }
        }

        ChassisWarpaint = SDBUtils.GetChassisWarpaint(ChassisID, fullbody, armor, bodysuit, glow);
        ChassisWarpaintPatterns = new WarpaintPatternData { Entries = [.. patterns] };
        ChassisWarpaintTatoos = new WarpaintTatooData { Entries = [.. tatoos] };
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
        return [.. SlottedItems
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
        })];
    }

    public SlottedModule[] GetChassisModules()
    {
        return [.. SlottedItems
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
        })];
    }

    public StatsData[] GetItemAttributes()
    {
        return [.. ItemAttributes
        .Select((pair) =>
        {
            return new StatsData()
            {
                Id = pair.Key,
                Value = pair.Value
            };
        })];
    }

    public StatsData[] GetPrimaryWeaponAttributes()
    {
        var result = new Dictionary<ushort, float>();
        var itemId = SlottedItems.GetValueOrDefault(LoadoutSlotType.Primary);
        if (itemId != 0)
        {
            MergeItemAttributeRange(result, itemId);
        }

        return [.. result
        .Select((pair) =>
        {
            return new StatsData()
            {
                Id = pair.Key,
                Value = pair.Value
            };
        })];
    }

    public StatsData[] GetSecondaryWeaponAttributes()
    {
        var result = new Dictionary<ushort, float>();
        var itemId = SlottedItems.GetValueOrDefault(LoadoutSlotType.Secondary);
        if (itemId != 0)
        {
            MergeItemAttributeRange(result, itemId);
        }

        return [.. result
        .Select((pair) =>
        {
            return new StatsData()
            {
                Id = pair.Key,
                Value = pair.Value
            };
        })];
    }

    public StatsData[] GetItemModuleScalars()
    {
        return [.. ItemModuleScalars
        .Select((pair) =>
        {
            return new StatsData()
            {
                Id = pair.Key,
                Value = pair.Value
            };
        })];
    }

    public StatsData[] GetItemCharacterScalars()
    {
        return [.. ItemCharacterScalars
        .Select((pair) =>
        {
            return new StatsData()
            {
                Id = pair.Key,
                Value = pair.Value
            };
        })];
    }

    private static byte ReadPatternRegion(uint data1) => data1 is >= 1 and <= 3 ? (byte)(data1 - 1) : NoRegionUsage;

    /// <summary>
    ///     Read the four dials a pattern carries as a vector of real numbers: rotation in degrees, scale, the
    ///     horizontal offset and the vertical offset.
    /// <param name="values">The four floats the client sent</param>
    /// <returns>The rotation in degrees, the scale and the offsets as one vector</returns>
    private static Vector4 ReadPatternTransform(float[] values) => ReadVisualTransform(values, 1)[0];

    /// <summary>
    ///     Read the placement a decal visual carries as the three vectors the visuals block wants. A decal's twelve
    ///     floats are a row major 3x3 rotation and scale followed by the translation.
    /// </summary>
    /// <param name="values">The floats the client sent</param>
    /// <param name="vectorCount">How many vectors the visual type carries</param>
    /// <returns>The vectors, zero filled where the input ran out</returns>
    private static Vector4[] ReadVisualTransform(float[] values, int vectorCount)
    {
        var vectors = new Vector4[vectorCount];
        if (values == null)
        {
            return vectors;
        }

        for (var vector = 0; vector < vectorCount; vector++)
        {
            var offset = vector * 4;
            vectors[vector] = new Vector4(
                offset < values.Length ? values[offset] : 0f,
                offset + 1 < values.Length ? values[offset + 1] : 0f,
                offset + 2 < values.Length ? values[offset + 2] : 0f,
                offset + 3 < values.Length ? values[offset + 3] : 0f);
        }

        return vectors;
    }

    private static void MergeItemAttributeRange(Dictionary<ushort, float> result, uint itemId)
    {
        var itemAttributes = SDBInterface.GetItemAttributeRange(itemId);
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

        var weaponSlots = SDBInterface.GetWeaponSlots(itemId);
        if (weaponSlots != null && weaponSlots.Count > 0)
        {
            if (weaponSlots.Count > 1)
            {
                Log.Warning("Weapon {WeaponId} has {Count} WeaponSlot entries, merging attributes sequentially", itemId, weaponSlots.Count);
            }

            foreach (var weaponSlot in weaponSlots)
            {
                if (weaponSlot.DefaultAbility != 0)
                {
                    var abilityModule = SDBInterface.GetAbilityModule(weaponSlot.DefaultAbility);
                    if (abilityModule != null)
                    {
                        var moduleAttributes = SDBInterface.GetItemAttributeRange(weaponSlot.DefaultAbility);
                        foreach (var range in moduleAttributes.Values)
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
            }
        }
    }

    private void InitFromLoadoutReferenceData(LoadoutReferenceData refData)
    {
        VehicleID = 77087;
        GliderID = 81423;
        LoadoutID = refData.LoadoutId;
        ChassisID = refData.ChassisId;
        BackpackID = SDBUtils.GetChassisDefaultBackpack(ChassisID);
        ApplyVisuals(refData.Visuals ?? []);

        // Assume PvE
        foreach (var (slot, type) in refData.SlottedItemsPvE)
        {
            SlottedItems.Add(slot, type);
        }

        CalculateItemAttributes();
    }

    private void ApplyItemStats(uint itemTypeId, Dictionary<ushort, float> totalAttributes, Dictionary<ushort, float> totalModuleScalars, Dictionary<ushort, float> totalCharacterScalars)
    {
        var itemAttributes = SDBInterface.GetItemAttributeRange(itemTypeId);
        foreach (var range in itemAttributes.Values)
        {
            if (totalAttributes.ContainsKey(range.AttributeId))
            {
                totalAttributes[range.AttributeId] += range.Base;
            }
            else
            {
                totalAttributes.Add(range.AttributeId, range.Base);
            }
        }

        var itemModuleScalars = SDBInterface.GetItemModuleScalars(itemTypeId);
        foreach (var (attributeCategoryId, scalars) in itemModuleScalars)
        {
            var attributeCategory = SDBInterface.GetAttributeCategory(attributeCategoryId);
            if (!totalCharacterScalars.ContainsKey(attributeCategoryId))
            {
                totalCharacterScalars.Add(attributeCategoryId, 0.0f);
            }

            if (attributeCategory.IsScalar == 1)
            {
                totalCharacterScalars[attributeCategoryId] += totalCharacterScalars[attributeCategoryId] / 100 * attributeCategory.ModuleEffectiveness;
            }
            else
            {
                totalCharacterScalars[attributeCategoryId] += scalars.Value - 1;
            }
        }

        var itemCharacterScalars = SDBInterface.GetItemCharacterScalars(itemTypeId);
        foreach (var (attributeCategoryId, scalars) in itemCharacterScalars)
        {
            var attributeCategory = SDBInterface.GetAttributeCategory(attributeCategoryId);
            if (!totalModuleScalars.ContainsKey(attributeCategoryId))
            {
                totalModuleScalars.Add(attributeCategoryId, 0.0f);
            }

            if (attributeCategory.IsScalar == 1)
            {
                totalModuleScalars[attributeCategoryId] += totalModuleScalars[attributeCategoryId] / 100 * attributeCategory.ModuleEffectiveness;
            }
            else
            {
                totalModuleScalars[attributeCategoryId] += scalars.Value - 1;
            }
        }
    }

    private void CalculateItemAttributes()
    {
        var attributes = new Dictionary<ushort, float>()
        {
            { 12, 2.5f }, // Temporary base Run Speed boost because it's so slow otherwise
        };
        var moduleScalars = new Dictionary<ushort, float>()
        {
        };
        var characterScalars = new Dictionary<ushort, float>()
        {
        };

        ApplyItemStats(ChassisID, attributes, moduleScalars, characterScalars);
        foreach (var pair in SlottedItems)
        {
            if (LoadoutAbilitySlots.Contains(pair.Key) || LoadoutChassisSlots.Contains(pair.Key))
            {
                ApplyItemStats(pair.Value, attributes, moduleScalars, characterScalars);
            }
        }

        foreach (var pair in _fallbackAttributes)
        {
            if (!attributes.ContainsKey(pair.Key))
            {
                attributes.Add(pair.Key, pair.Value);
            }
        }

        foreach (var pair in _fallbackModuleScalars)
        {
            if (!moduleScalars.ContainsKey(pair.Key))
            {
                moduleScalars.Add(pair.Key, pair.Value);
            }
        }

        foreach (var pair in _fallbackCharacterScalars)
        {
            if (!characterScalars.ContainsKey(pair.Key))
            {
                characterScalars.Add(pair.Key, pair.Value);
            }
        }

        ItemAttributes = attributes;
        ItemModuleScalars = moduleScalars;
        ItemCharacterScalars = characterScalars;
        GearHealthAttribute = attributes.GetValueOrDefault((ushort)ItemAttributeId.Health);
    }
}