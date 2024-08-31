namespace GameServer.Data.SDB;

using System.Collections.Generic;
using FauFau.Formats;
using Records.dbcharacter;
using Records.dbitems;
using Records.dbviusalrecords;
using Records.apt;
using Records.aptfs;
using Records.vcs;
using Records;
using System;
using AeroMessages.GSS.V66.Character;
using System.Linq;

public class SDBUtils
{
    public static Dictionary<byte, CharCreateLoadoutSlots> GetDefaultLoadoutSlots(uint loadoutId)
    {
        var loadout = SDBInterface.GetCharCreateLoadout(loadoutId);
        if (loadout == null)
        {
            return null;
        }

        var defaultLoadoutSlots = SDBInterface.GetCharCreateLoadoutSlots(loadout.Id);
        if (defaultLoadoutSlots == null)
        {
            return null;
        }

        return defaultLoadoutSlots;
    }

    public static Dictionary<byte, CharCreateLoadoutSlots> GetChassisDefaultLoadoutSlots(uint chassisId)
    {
        var loadouts = SDBInterface.GetCharCreateLoadoutsByFrame(chassisId); // yolo
        CharCreateLoadout defaultLoadout;
        if (loadouts.Length == 0) 
        {
            return null;
        }
        else
        {
            defaultLoadout = loadouts.First();
        }

        if (defaultLoadout == null)
        {
            return null;
        }

        var defaultLoadoutSlots = SDBInterface.GetCharCreateLoadoutSlots(defaultLoadout.Id);
        if (defaultLoadoutSlots == null)
        {
            return null;
        }

        return defaultLoadoutSlots;
    }

    public static uint GetChassisDefaultBackpack(uint chassisId)
    {
        var loadouts = SDBInterface.GetCharCreateLoadoutsByFrame(chassisId); // yolo
        CharCreateLoadout defaultLoadout;
        if (loadouts.Length == 0) 
        {
            return 0;
        }
        else
        {
            defaultLoadout = loadouts.First();
        }

        if (defaultLoadout == null)
        {
            return 0;
        }

        var defaultLoadoutSlots = SDBInterface.GetCharCreateLoadoutSlots(defaultLoadout.Id);
        if (defaultLoadoutSlots == null)
        {
            return 0;
        }
        
        defaultLoadoutSlots.TryGetValue((byte)LoadoutSlotType.Backpack, out CharCreateLoadoutSlots defaultBackpackSlot);
        if (defaultBackpackSlot == null)
        {
            return 0;
        }

        return defaultBackpackSlot.DefaultPveModule;
    }

    public static ChassisWarpaintResult GetChassisWarpaint(uint chassisId, uint customFullbody, uint customArmor, uint customBodysuit, uint customGlow)
    {
        var chassisInfo = SDBInterface.GetBattleframe(chassisId);

        uint defaultFullbody = chassisInfo.DefaultFullbodyPaletteId;
        uint defaultArmor = chassisInfo.DefaultArmorPaletteId;
        uint defaultBodysuit = chassisInfo.DefaultBodysuitPaletteId;
        uint defaultGlow = chassisInfo.DefaultGlowPaletteId;

        uint fullbodyId = customFullbody != 0 ? customFullbody : defaultFullbody;
        uint armorId = customArmor != 0 ? customArmor : defaultArmor;
        uint bodysuitId = customBodysuit != 0 ? customBodysuit : defaultBodysuit;
        uint glowId = customGlow != 0 ? customGlow : defaultGlow;

        var fullbody = SDBInterface.GetWarpaintPalette(fullbodyId);
        var armor = SDBInterface.GetWarpaintPalette(armorId);
        var bodysuit = SDBInterface.GetWarpaintPalette(bodysuitId);
        var glow = SDBInterface.GetWarpaintPalette(glowId);
    
        var input = new[] { fullbody, armor, bodysuit, glow };
        
        var gradients = new List<uint>();
        var palettes = new List<VisualsPaletteBlock>();
        var colors = new uint[7]
        {
            // Temp? Not sure but there should probably be a base color
            4278190080,
            4278190080,
            4278190080,
            4278190080,
            4278190080,
            4278190080,
            4278190080
        };

        foreach (var data in input)
        {
            if (data == null)
            {
                continue;
            }
            
            // Add palette
            palettes.Add(new() { PaletteId = data.Id, PaletteType = (byte)data.TypeFlags });

            // Calc colors
            var paletteColors = new uint[7]
            {
                FColor.CombineLightDark(data.Color1Highlight, data.Color1Shadow),
                FColor.CombineLightDark(data.Color2Highlight, data.Color2Shadow),
                FColor.CombineLightDark(data.Color3Highlight, data.Color3Shadow),
                FColor.CombineLightDark(data.Color4Highlight, data.Color4Shadow),
                FColor.CombineLightDark(data.Color5Highlight, data.Color5Shadow),
                FColor.CombineLightDark(data.Color6Highlight, data.Color6Shadow),
                FColor.CombineLightDark(data.Color7Highlight, data.Color7Shadow),
            };

            // Fullbody
            if ((data.TypeFlags & (uint)Math.Pow(2, 4)) != 0)
            {
                colors[0] = paletteColors[0];
                colors[1] = paletteColors[1];
                colors[2] = paletteColors[2];
                colors[3] = paletteColors[3];
                colors[4] = paletteColors[4];
                colors[5] = paletteColors[5];
                colors[6] = paletteColors[6];
            }

            // Armor
            if ((data.TypeFlags & (uint)Math.Pow(2, 0)) != 0)
            {
                colors[0] = paletteColors[0];
                colors[1] = paletteColors[1];
                colors[2] = paletteColors[2];
            }

            // Bodysuit
            if ((data.TypeFlags & (uint)Math.Pow(2, 1)) != 0)
            {
                colors[3] = paletteColors[3];
                colors[4] = paletteColors[4];
            }

            // Glow
            if ((data.TypeFlags & (uint)Math.Pow(2, 3)) != 0)
            {
                colors[5] = paletteColors[5];
                colors[6] = paletteColors[6];
            }

            // Gradient
            if (data.TextureGradientId != 0)
            {
                gradients.Add(data.TextureGradientId);
            }
        }

        return new ChassisWarpaintResult
        {
            Gradients = gradients.ToArray(),
            Colors = colors,
            Palettes = palettes.ToArray(),
        };
    }

    public static VehicleInfoResult GetDetailedVehicleInfo(ushort vehicleId)
    {
        var vehicleInfo = SDBInterface.GetVehicleInfo(vehicleId);
        var vehicleClass = SDBInterface.GetVehicleClass(vehicleInfo.VehicleClass);
        var baseComponents = SDBInterface.GetBaseComponentDef(vehicleId);
        var result = new VehicleInfoResult()
        {
            VehicleId = vehicleId,
            FactionId = vehicleInfo.FactionId,
            Class = vehicleClass.Name,
            ScopeRange = 150,
            SpawnHeight = 1,
            SpawnAbility = 0,
            DespawnAbility = 0,
            HasDriverSeat = false,
            DriverPosture = 0,
            MaxPassengers = 0,
            PassengerPosture = 0,
            HasActivePassenger = false,
            SkipOnePassenger = false,
            Abilities = new List<AbilityComponentDef>(),
            DeathAbility = 0,
            MaxHitPoints = 100,
            DamageResponse = 0,
            StatusFxId = 0,
            Turrets = new List<TurretComponentDef>(),
            Deployables = new List<DeployableComponentDef>(),
        };

        foreach (var baseComponent in baseComponents.Values)
        {
            var componentId = baseComponent.Id;

            var componentType = (ComponentType)baseComponent.SdbGuid;
            switch (componentType)
            {
                case ComponentType.Scoping:
                    var scopingComponent = SDBInterface.GetScopingComponentDef(componentId);
                    result.ScopeRange = scopingComponent.ScopeRange;
                    result.SpawnHeight = scopingComponent.SpawnHeight;
                    result.SpawnAbility = scopingComponent.SpawnAbility;
                    result.DespawnAbility = scopingComponent.DespawnAbility;
                    break;

                case ComponentType.Driver:
                    var driverComponent = SDBInterface.GetDriverComponentDef(componentId);
                    result.HasDriverSeat = true;
                    result.DriverPosture = driverComponent.Posture;
                    break;

                case ComponentType.Passenger:
                    var passengerComponent = SDBInterface.GetPassengerComponentDef(componentId);
                    result.MaxPassengers = passengerComponent.MaxPassengers;
                    result.PassengerPosture = passengerComponent.Posture;
                    result.HasActivePassenger = passengerComponent.ActivePassenger == 1;
                    result.SkipOnePassenger = passengerComponent.LeadingZero == 1;
                    break;

                case ComponentType.Ability:
                    var abilityComponent = SDBInterface.GetAbilityComponentDef(componentId);
                    result.Abilities.Add(abilityComponent);
                    break;

                case ComponentType.Damage:
                    var damageComponent = SDBInterface.GetDamageComponentDef(componentId);
                    result.DeathAbility = damageComponent.DeathAbility;
                    result.MaxHitPoints = damageComponent.MaxHitPoints;
                    result.DamageResponse = damageComponent.DamageResponse;
                    break;

                case ComponentType.StatusEffect:
                    var statusEffectComponent = SDBInterface.GetStatusEffectComponentDef(componentId);
                    result.StatusFxId = statusEffectComponent.StatusFxId;
                    break;

                case ComponentType.Turret:
                    var turretComponent = SDBInterface.GetTurretComponentDef(componentId);
                    result.Turrets.Add(turretComponent);
                    break;

                case ComponentType.Deployable:
                    var deployableComponent = SDBInterface.GetDeployableComponentDef(componentId);
                    result.Deployables.Add(deployableComponent);
                    break;

                case ComponentType.SpawnPoint:
                    // TODO: Probably for allowing spawning into the vehicle
                    // var spawnPointComponent = SDBInterface.GetSpawnPointComponentDef(componentId);
                    break;

                default:
                    // Console.WriteLine($"Unhandled vehicle component, id: {componentId}, type: {componentType}");
                    break;
            }
        }

        return result;
    }

    public static WeaponInfoResult GetDetailedWeaponInfo(uint weaponSdbId)
    {
        // Get weapon
        Weapons weapon = SDBInterface.GetWeapon(weaponSdbId);
        if (weapon == null)
        {
            Console.WriteLine($"GetDetailedWeaponInfo could not find weapon {weaponSdbId}");
            return null;
        }

        // Get main template
        WeaponTemplateResult main = GetDetailedWeaponTemplateInfo(weapon.WeaponTypeId, weaponSdbId);
        if (main == null)
        {
            Console.WriteLine($"GetDetailedWeaponInfo could not find main template {weapon.WeaponTypeId} for {weaponSdbId}");
            return null;
        }

        // Get main scope and underbarrel
        uint scopeStatusFx = 0;
        WeaponUnderbarrel mainUnderbarrel = null;
        if (main.ScopeId != 0)
        {
            var scope = SDBInterface.GetWeaponScope(main.ScopeId);
            scopeStatusFx = scope.Statusfx;
        }
        
        if (main.UnderbarrelId != 0)
        {
            mainUnderbarrel = SDBInterface.GetWeaponUnderbarrel(main.UnderbarrelId);
        }

        WeaponTemplateResult alt = null;
        if (mainUnderbarrel != null && mainUnderbarrel.WeaponTypeId != 0)
        {
            alt = GetDetailedWeaponTemplateInfo(mainUnderbarrel.WeaponTypeId, main.UnderbarrelId, true);
        }

        return new WeaponInfoResult() { Main = main, Alt = alt, ScopeStatusFx = scopeStatusFx };
    }

    public static WeaponTemplateResult GetDetailedWeaponTemplateInfo(uint weaponTypeId, uint weaponSdbId, bool isUnderbarrel = false)
    {
        // Template must exist
        var template = SDBInterface.GetWeaponTemplate(weaponTypeId);
        if (template == null)
        {
            Console.WriteLine($"GetDetailedWeaponInfo could not find template {weaponTypeId}");
            return null;
        }

        // Modifiers are optional
        var modifiers = SDBInterface.GetWeaponTemplateModifiers(weaponSdbId);

        WeaponTemplateResult result = new WeaponTemplateResult()
        {
            // Debug
            DebugName = $"{(isUnderbarrel ? "Underbarrel" : "Main")} {weaponSdbId} (Type {weaponTypeId} - {template.Name.TrimEnd('\0')})",
            
            // Components
            ScopeId = WeaponTemplateOverrider(template.DefaultScopeId, modifiers?.DefaultScopeId),
            UnderbarrelId = WeaponTemplateOverrider(template.DefaultUnderbarrelId, modifiers?.DefaultUnderbarrelId),
            AmmoId = WeaponTemplateOverrider(template.DefaultAmmoId, modifiers?.DefaultAmmoId),

            // Properties
            WeaponFlags = WeaponTemplateModifier(template.WeaponFlags, modifiers?.WeaponFlags),
            FireType = WeaponTemplateModifier(template.FireType, modifiers?.FireType),
            Range = WeaponTemplateModifier(template.Range, modifiers?.Range, modifiers?.RangeMult),
            EquipEnterMs = WeaponTemplateModifier(template.EquipEnterMs, modifiers?.EquipEnterMs),
            EquipExitMs = WeaponTemplateModifier(template.EquipExitMs, modifiers?.EquipExitMs),
            SlotIndex = WeaponTemplateOverrider(template.SlotIndex, (byte?)modifiers?.SlotIndex), // Not sure why the modifier table has this as sbyte, but 0 and no negative values are seen, so assuming we should not subtract and simply cast to byte.

            // Abilities
            MeleeAbility = WeaponTemplateOverrider(template.MeleeAbilityId, modifiers?.MeleeAbilityId),
            AttackAbility = WeaponTemplateOverrider(template.AttackAbilityId, modifiers?.AttackAbilityId),
            OverchargeAbility = WeaponTemplateOverrider(template.OverchargeAbility, modifiers?.OverchargeAbility),
            BurstAbility = WeaponTemplateOverrider(template.BurstAbilityId, modifiers?.BurstAbilityId),
            ReloadAbility = WeaponTemplateOverrider(template.ReloadAbility, modifiers?.ReloadAbility),
            EmptyAbility = WeaponTemplateOverrider(template.ClipEmptyAbility, modifiers?.ClipEmptyAbility),

            // Ammo, Clip, Reload
            BaseClipSize = WeaponTemplateModifier(template.BaseClipSize, modifiers?.BaseClipSize, modifiers?.BaseClipSizeMult),
            MaxAmmo = WeaponTemplateModifier(template.MaxAmmo, modifiers?.MaxAmmo, modifiers?.MaxAmmoMult),
            AmmoPerBurst = WeaponTemplateModifier(template.AmmoPerBurst, modifiers?.AmmoPerBurst),
            MinAmmoPerBurst = WeaponTemplateModifier(template.MinAmmoPerBurst, modifiers?.MinAmmoPerBurst),
            RoundsPerBurst = WeaponTemplateModifier(template.RoundsPerBurst, modifiers?.RoundsPerBurst, modifiers?.RoundsPerBurstMult),
            MinRoundsPerBurst = WeaponTemplateModifier(template.MinRoundsPerBurst, modifiers?.MinRoundsPerBurst, modifiers?.MinRoundsPerBurstMult),
            RoundReload = WeaponTemplateModifier(template.RoundReload, modifiers?.RoundReload),
            ClipRegenMs = WeaponTemplateModifier(template.ClipRegenMs, modifiers?.ClipRegenMs, modifiers?.ClipRegenMsMult),
            ReloadTime = WeaponTemplateModifier(template.ReloadTime, modifiers?.ReloadTime, modifiers?.ReloadTimeMult),
            ReloadPenalty = WeaponTemplateModifier(template.ReloadPenalty, modifiers?.ReloadPenalty, modifiers?.ReloadPenaltyMult),

            // Targets
            MaxTargets = WeaponTemplateModifier(template.MaxTargets, modifiers?.MaxTargets),
            BurstBonusPerTarget = WeaponTemplateModifier(template.BurstbonusPerTarget, modifiers?.BurstbonusPerTarget), 
            TargetingRange = WeaponTemplateModifier(template.TargetingRange, modifiers?.TargetingRange, modifiers?.TargetingRangeMult),
            
            // Burst
            MsPerBurst = WeaponTemplateModifier(template.MsPerBurst, modifiers?.MsPerBurst, modifiers?.MsPerBurstMult),
            MsBurstDuration = WeaponTemplateModifier(template.MsBurstDuration, modifiers?.MsBurstDuration),

            // Chargeup
            MsChargeUp = WeaponTemplateModifier(template.MsChargeup, modifiers?.MsChargeup, modifiers?.MsChargeupMult),
            MsChargeUpMax = WeaponTemplateModifier(template.MsChargeupMax, modifiers?.MsChargeupMax, modifiers?.MsChargeupMaxMult),
            MsChargeUpMin = WeaponTemplateModifier(template.MsChargeupMin, modifiers?.MsChargeupMin, modifiers?.MsChargeupMinMult),

            // Overcharge
            MsOverchargeDelay = WeaponTemplateModifier(template.MsOverchargeDelay, modifiers?.MsOverchargeDelay),

            // Damage
            MinDamage = WeaponTemplateModifier(template.MinDamage, modifiers?.MinDamage, modifiers?.MinDamageMult),
            DamagePerRound = WeaponTemplateModifier(template.DamagePerRound, modifiers?.DamagePerRound, modifiers?.DamagePerRoundMult),
            HeadshotMult = WeaponTemplateModifier(template.HeadshotMult, modifiers?.HeadshotMult, modifiers?.HeadshotMultMult),

            // Spread
            MinSpread = WeaponTemplateModifier(template.MinSpread, modifiers?.MinSpread, modifiers?.MinSpreadMult), // min_spread_frac
            MaxSpread = WeaponTemplateModifier(template.MaxSpread, modifiers?.MaxSpread, modifiers?.MaxSpreadMult), // max_spread_frac
            StartingSpread = WeaponTemplateModifier(template.StartingSpread, modifiers?.StartingSpread, modifiers?.StartingSpreadMult),
            SpreadPerBurst = WeaponTemplateModifier(template.SpreadPerBurst, modifiers?.SpreadPerBurst, modifiers?.SpreadPerBurstMult),
            SpreadRampExponent = WeaponTemplateModifier(template.SpreadRampExponent, modifiers?.SpreadRampExponent),
            SpreadRampTime = WeaponTemplateModifier(template.SpreadRampTime, modifiers?.SpreadRampTime),
            RunMinSpread = WeaponTemplateModifier(template.RunMinspreadAdd, modifiers?.RunMinspreadAdd),
            JumpMinSpread = WeaponTemplateModifier(template.JumpMinspreadAdd, modifiers?.JumpMinspreadAdd),
            MsSpreadReturnDelay = WeaponTemplateModifier(template.MsSpreadReturnDelay, modifiers?.MsSpreadReturnDelay),
            MsSpreadReturn = WeaponTemplateModifier(template.MsSpreadReturn, modifiers?.MsSpreadReturn),
            NoSpreadChance = WeaponTemplateModifier(template.NoSpreadChance, modifiers?.NoSpreadChance, modifiers?.NoSpreadChanceMult),

            // "Agility"
            Agility = WeaponTemplateModifier(template.Agility, modifiers?.Agility),
            MsAgilityReturn = WeaponTemplateModifier(template.MsAgilityReturn, modifiers?.MsAgilityReturn),
            MsAgilityReturnDelay = WeaponTemplateModifier(template.MsAgilityReturnDelay, modifiers?.MsAgilityReturnDelay),

            // ?
            MsReturn = WeaponTemplateModifier(template.MsReturn, modifiers?.MsReturn),
        };

        return result;
    }

    private static uint WeaponTemplateOverrider(uint baseValue, uint? overrideValue)
    {
        if (overrideValue != null)
        {
            return (uint)overrideValue;
        }
        else
        {
            return baseValue;
        }
    }

    private static ushort WeaponTemplateOverrider(ushort baseValue, ushort? overrideValue)
    {
        if (overrideValue != null)
        {
            return (ushort)overrideValue;
        }
        else
        {
            return baseValue;
        }
    }

    private static byte WeaponTemplateOverrider(byte baseValue, byte? overrideValue)
    {
        if (overrideValue != null)
        {
            return (byte)overrideValue;
        }
        else
        {
            return baseValue;
        }
    }

    private static sbyte WeaponTemplateModifier(sbyte baseValue, sbyte? modifierValue, float? multiplierValue = 1)
    {
        return (sbyte)((baseValue + (modifierValue ?? 0)) * multiplierValue ?? 1);
    }

    private static byte WeaponTemplateModifier(byte baseValue, sbyte? modifierValue, float? multiplierValue = 1)
    {
        return (byte)((baseValue + (modifierValue ?? 0)) * multiplierValue ?? 1);
    }

    private static uint WeaponTemplateModifier(uint baseValue, int? modifierValue, float? multiplierValue = 1)
    {
        return (uint)((baseValue + (modifierValue ?? 0)) * multiplierValue ?? 1);
    }

    private static int WeaponTemplateModifier(int baseValue, int? modifierValue, float? multiplierValue = 1)
    {
        return (int)((baseValue + (modifierValue ?? 0)) * multiplierValue ?? 1);
    }

    private static ushort WeaponTemplateModifier(ushort baseValue, short? modifierValue, float? multiplierValue = 1)
    {
        return (ushort)((baseValue + (modifierValue ?? 0)) * multiplierValue ?? 1);
    }

    private static float WeaponTemplateModifier(float baseValue, float? modifierValue, float? multiplierValue = 1)
    {
        return (float)((baseValue + (modifierValue ?? 0)) * multiplierValue ?? 1);
    }
}

public class WeaponInfoResult
{
    public WeaponTemplateResult Main;
    public WeaponTemplateResult Alt;
    public uint ScopeStatusFx;
}

public class WeaponTemplateResult
{
    // There's a few more props that we aren't bothering with atm
    // This includes the stuff that is presumably client side like animations and first person offsets.
    // We also ignore everything related to Slide, Rise, Jitter. We assume we can live with the client side work here.
    // Not sure about agility...

    // Debug
    public string DebugName;
    
    // Components
    public uint ScopeId;
    public uint UnderbarrelId;
    public ushort AmmoId;

    // Properties
    public uint WeaponFlags;
    public byte FireType;
    public byte SlotIndex;
    public float Range;
    public uint EquipEnterMs;
    public uint EquipExitMs;
    
    // Abilities
    public uint MeleeAbility;
    public uint AttackAbility;
    public uint OverchargeAbility;
    public uint BurstAbility;
    public uint ReloadAbility;
    public uint EmptyAbility;

    // Ammo, Clip, Reload
    public ushort BaseClipSize;
    public ushort MaxAmmo;
    public sbyte AmmoPerBurst;
    public sbyte MinAmmoPerBurst;
    public byte RoundsPerBurst;
    public byte MinRoundsPerBurst;
    public byte RoundReload;
    public uint ClipRegenMs;
    public uint ReloadTime;
    public uint ReloadPenalty;

    // Targets
    public byte MaxTargets;
    public byte BurstBonusPerTarget;
    public float TargetingRange;

    // Burst
    public uint MsPerBurst;
    public uint MsBurstDuration;

    // Chargeup
    public uint MsChargeUp;
    public uint MsChargeUpMax;
    public uint MsChargeUpMin;

    // Overcharge
    public uint MsOverchargeDelay;

    // Damage
    public int MinDamage;
    public int DamagePerRound;
    public float HeadshotMult;

    // Spread
    public float MinSpread;
    public float MaxSpread;
    public float StartingSpread;
    public float SpreadPerBurst;
    public float SpreadRampExponent;
    public uint SpreadRampTime;
    public float RunMinSpread;
    public float JumpMinSpread;
    public uint MsSpreadReturnDelay;
    public uint MsSpreadReturn;
    public float NoSpreadChance;

    // "Agility"
    public float Agility;
    public uint MsAgilityReturn;
    public uint MsAgilityReturnDelay;

    // ?
    public uint MsReturn;
}

public class VehicleInfoResult
{
    public ushort VehicleId;
    public uint FactionId;
    public string Class;
    public float ScopeRange;
    public float SpawnHeight;
    public uint SpawnAbility;
    public uint DespawnAbility;
    public bool HasDriverSeat;
    public byte DriverPosture;
    public uint MaxPassengers;
    public byte PassengerPosture;
    public bool HasActivePassenger;
    public bool SkipOnePassenger;
    public List<AbilityComponentDef> Abilities;
    public uint DeathAbility;
    public float MaxHitPoints;
    public uint DamageResponse;
    public uint StatusFxId;
    public List<TurretComponentDef> Turrets;
    public List<DeployableComponentDef> Deployables;
}

public class ChassisWarpaintResult
{
    public uint[] Gradients;
    public uint[] Colors;
    public VisualsPaletteBlock[] Palettes;
}

// Yoink it from RIN
public class FColor
{
    public static uint   CombineLightDark(uint light, uint dark) => ARGB8888ToRGB565(dark) | (uint)(ARGB8888ToRGB565(light) << 16);
    public static uint   ExtractLight(uint     combined) => RGB565ToARGB8888((ushort)(combined >> 16));
    public static uint   ExtractDark(uint      combined) => RGB565ToARGB8888((ushort)combined);
    public static ushort ARGB8888ToRGB565(uint argb)     => (ushort)(((((byte)(argb >> 16) >> 3) & 0x1f) << 11) | ((((byte)(argb >> 8) >> 2) & 0x3f) << 5) | (((byte)argb >> 3) & 0x1f));

    public static uint RGB565ToARGB8888(ushort rgb)
    {
        int r = ((rgb >> 11) * 255) + 16;
        int g = (((rgb & 0x07E0) >> 5) * 255) + 32;
        int b = ((rgb & 0x001F) * 255) + 16;
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
        return (uint)(0xFF000000 | (byte)(((r / 32) + r) / 32) << 16 | (byte)(((g / 64) + g) / 64) << 8 | (byte)(((b / 32) + b) / 32));
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
    }
}