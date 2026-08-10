namespace GameServer.StaticDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.GSS.V66.Character;
using Data;
using Records.dbcharacter;
using Records.dbitems;
using Records.vcs;
using Serilog;

public class SDBUtils
{
    private static readonly ILogger _logger = Log.ForContext<SDBInterface>();

    public static Vector3 Vector3FromFauFau(FauFau.Util.CommmonDataTypes.Vector3 input)
    {
        return new Vector3(input.x, input.y, input.z);
    }

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
        var chassisInfo = chassisId != 0 ? SDBInterface.GetBattleframe(chassisId) : new Battleframe();

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
            Gradients = [.. gradients],
            Colors = colors,
            Palettes = [.. palettes],
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
            Abilities = [],
            DeathAbility = 0,
            MaxHitPoints = 100,
            DamageResponse = 0,
            StatusFxId = 0,
            Turrets = [],
            Deployables = [],
            HullSegment = null,
            DriverPoseFile = 0,
            PasengerPoseFile = 0,
            PassengerPoseOffset = Vector3.Zero,
            DriverPoseOffset = Vector3.Zero,
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
                    result.DriverPoseFile = driverComponent.DriverPoseFile;
                    result.DriverPoseOffset = Vector3FromFauFau(driverComponent.DriverPoseFileOffset);
                    break;

                case ComponentType.Passenger:
                    var passengerComponent = SDBInterface.GetPassengerComponentDef(componentId);
                    result.MaxPassengers = passengerComponent.MaxPassengers;
                    result.PassengerPosture = passengerComponent.Posture;
                    result.HasActivePassenger = passengerComponent.ActivePassenger == 1;
                    result.SkipOnePassenger = passengerComponent.LeadingZero == 1;
                    result.PasengerPoseFile = passengerComponent.PassengerPoseFile;
                    result.PassengerPoseOffset = Vector3FromFauFau(passengerComponent.PassengerPoseFileOffset);
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

                case ComponentType.HullSegment:
                    result.HullSegment = SDBInterface.GetHullSegmentComponentDef(componentId);
                    break;

                default:
                    _logger.Debug("Unhandled vehicle component, id: {componentId}, type: {componentType}", componentId, componentType);
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
            _logger.Error("GetDetailedWeaponInfo could not find weapon {weaponSdbId}", weaponSdbId);
            return null;
        }

        // Get main template
        WeaponTemplateResult main = GetDetailedWeaponTemplateInfo(weapon.WeaponTypeId, weaponSdbId);
        if (main == null)
        {
            _logger.Error("GetDetailedWeaponInfo could not find main template {WeaponTypeId} for {weaponSdbId}", weapon.WeaponTypeId, weaponSdbId);
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
            _logger.Error("GetDetailedWeaponInfo could not find template {weaponTypeId}", weaponTypeId);
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
            // TODO: Investigate attribute scaling for MsPerBurst / ms_per_burst_OverridenByRateOfFireAttribute.
            MsPerBurst = WeaponTemplateModifier(template.MsPerBurst, modifiers?.MsPerBurst, modifiers?.MsPerBurstMult),

            // TODO: Investigate attribute scaling for MsBurstDuration.
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
            MinSpreadFrac = WeaponTemplateModifier(template.MinSpreadFrac, modifiers?.MinSpreadFrac),
            MsRiseReturnDelay = WeaponTemplateModifier(template.MsRiseReturnDelay, modifiers?.MsRiseReturnDelay),
            RunSpreadRampMult = 1f,
            JumpSpreadRampMult = 1f,
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

        // Cascade module modifiers: WeaponSlot.DefaultAbility -> AbilityModule -> WeaponTemplateModifiers
        var weaponSlots = SDBInterface.GetWeaponSlots(weaponSdbId);
        if (weaponSlots != null && weaponSlots.Count > 0)
        {
            if (weaponSlots.Count > 1)
            {
                _logger.Warning("Weapon {WeaponId} has {Count} WeaponSlot entries, merging modifiers sequentially", weaponSdbId, weaponSlots.Count);
            }

            foreach (var weaponSlot in weaponSlots)
            {
                if (weaponSlot.DefaultAbility != 0)
                {
                    var abilityModule = SDBInterface.GetAbilityModule(weaponSlot.DefaultAbility);
                    if (abilityModule != null)
                    {
                        var moduleModifiers = SDBInterface.GetWeaponTemplateModifiers(weaponSlot.DefaultAbility);
                        if (moduleModifiers != null)
                        {
                            result.ScopeId = WeaponTemplateOverrider(result.ScopeId, moduleModifiers.DefaultScopeId);
                            result.UnderbarrelId = WeaponTemplateOverrider(result.UnderbarrelId, moduleModifiers.DefaultUnderbarrelId);
                            result.AmmoId = WeaponTemplateOverrider(result.AmmoId, moduleModifiers.DefaultAmmoId);
                            result.WeaponFlags = WeaponTemplateModifier(result.WeaponFlags, moduleModifiers.WeaponFlags);
                            result.FireType = WeaponTemplateModifier(result.FireType, moduleModifiers.FireType);
                            result.Range = WeaponTemplateModifier(result.Range, moduleModifiers.Range, moduleModifiers.RangeMult);
                            result.EquipEnterMs = WeaponTemplateModifier(result.EquipEnterMs, moduleModifiers.EquipEnterMs);
                            result.EquipExitMs = WeaponTemplateModifier(result.EquipExitMs, moduleModifiers.EquipExitMs);
                            result.MeleeAbility = WeaponTemplateOverrider(result.MeleeAbility, moduleModifiers.MeleeAbilityId);
                            result.AttackAbility = WeaponTemplateOverrider(result.AttackAbility, moduleModifiers.AttackAbilityId);
                            result.OverchargeAbility = WeaponTemplateOverrider(result.OverchargeAbility, moduleModifiers.OverchargeAbility);
                            result.BurstAbility = WeaponTemplateOverrider(result.BurstAbility, moduleModifiers.BurstAbilityId);
                            result.ReloadAbility = WeaponTemplateOverrider(result.ReloadAbility, moduleModifiers.ReloadAbility);
                            result.EmptyAbility = WeaponTemplateOverrider(result.EmptyAbility, moduleModifiers.ClipEmptyAbility);
                            result.BaseClipSize = WeaponTemplateModifier(result.BaseClipSize, moduleModifiers.BaseClipSize, moduleModifiers.BaseClipSizeMult);
                            result.MaxAmmo = WeaponTemplateModifier(result.MaxAmmo, moduleModifiers.MaxAmmo, moduleModifiers.MaxAmmoMult);
                            result.AmmoPerBurst = WeaponTemplateModifier(result.AmmoPerBurst, moduleModifiers.AmmoPerBurst);
                            result.MinAmmoPerBurst = WeaponTemplateModifier(result.MinAmmoPerBurst, moduleModifiers.MinAmmoPerBurst);
                            result.RoundsPerBurst = WeaponTemplateModifier(result.RoundsPerBurst, moduleModifiers.RoundsPerBurst, moduleModifiers.RoundsPerBurstMult);
                            result.MinRoundsPerBurst = WeaponTemplateModifier(result.MinRoundsPerBurst, moduleModifiers.MinRoundsPerBurst, moduleModifiers.MinRoundsPerBurstMult);
                            result.RoundReload = WeaponTemplateModifier(result.RoundReload, moduleModifiers.RoundReload);
                            result.ClipRegenMs = WeaponTemplateModifier(result.ClipRegenMs, moduleModifiers.ClipRegenMs, moduleModifiers.ClipRegenMsMult);
                            result.ReloadTime = WeaponTemplateModifier(result.ReloadTime, moduleModifiers.ReloadTime, moduleModifiers.ReloadTimeMult);
                            result.ReloadPenalty = WeaponTemplateModifier(result.ReloadPenalty, moduleModifiers.ReloadPenalty, moduleModifiers.ReloadPenaltyMult);
                            result.MaxTargets = WeaponTemplateModifier(result.MaxTargets, moduleModifiers.MaxTargets);
                            result.BurstBonusPerTarget = WeaponTemplateModifier(result.BurstBonusPerTarget, moduleModifiers.BurstbonusPerTarget);
                            result.TargetingRange = WeaponTemplateModifier(result.TargetingRange, moduleModifiers.TargetingRange, moduleModifiers.TargetingRangeMult);
                            result.MsPerBurst = WeaponTemplateModifier(result.MsPerBurst, moduleModifiers.MsPerBurst, moduleModifiers.MsPerBurstMult);
                            result.MsBurstDuration = WeaponTemplateModifier(result.MsBurstDuration, moduleModifiers.MsBurstDuration);
                            result.MsChargeUp = WeaponTemplateModifier(result.MsChargeUp, moduleModifiers.MsChargeup, moduleModifiers.MsChargeupMult);
                            result.MsChargeUpMax = WeaponTemplateModifier(result.MsChargeUpMax, moduleModifiers.MsChargeupMax, moduleModifiers.MsChargeupMaxMult);
                            result.MsChargeUpMin = WeaponTemplateModifier(result.MsChargeUpMin, moduleModifiers.MsChargeupMin, moduleModifiers.MsChargeupMinMult);
                            result.MsOverchargeDelay = WeaponTemplateModifier(result.MsOverchargeDelay, moduleModifiers.MsOverchargeDelay);
                            result.MinDamage = WeaponTemplateModifier(result.MinDamage, moduleModifiers.MinDamage, moduleModifiers.MinDamageMult);
                            result.DamagePerRound = WeaponTemplateModifier(result.DamagePerRound, moduleModifiers.DamagePerRound, moduleModifiers.DamagePerRoundMult);
                            result.HeadshotMult = WeaponTemplateModifier(result.HeadshotMult, moduleModifiers.HeadshotMult, moduleModifiers.HeadshotMultMult);
                            result.MinSpread = WeaponTemplateModifier(result.MinSpread, moduleModifiers.MinSpread, moduleModifiers.MinSpreadMult);
                            result.MaxSpread = WeaponTemplateModifier(result.MaxSpread, moduleModifiers.MaxSpread, moduleModifiers.MaxSpreadMult);
                            result.StartingSpread = WeaponTemplateModifier(result.StartingSpread, moduleModifiers.StartingSpread, moduleModifiers.StartingSpreadMult);
                            result.SpreadPerBurst = WeaponTemplateModifier(result.SpreadPerBurst, moduleModifiers.SpreadPerBurst, moduleModifiers.SpreadPerBurstMult);
                            result.SpreadRampExponent = WeaponTemplateModifier(result.SpreadRampExponent, moduleModifiers.SpreadRampExponent);
                            result.SpreadRampTime = WeaponTemplateModifier(result.SpreadRampTime, moduleModifiers.SpreadRampTime);
                            result.RunMinSpread = WeaponTemplateModifier(result.RunMinSpread, moduleModifiers.RunMinspreadAdd);
                            result.JumpMinSpread = WeaponTemplateModifier(result.JumpMinSpread, moduleModifiers.JumpMinspreadAdd);
                            result.MinSpreadFrac = WeaponTemplateModifier(result.MinSpreadFrac, moduleModifiers.MinSpreadFrac);
                            result.MsRiseReturnDelay = WeaponTemplateModifier(result.MsRiseReturnDelay, moduleModifiers.MsRiseReturnDelay);
                            result.RunSpreadRampMult = 1f;
                            result.JumpSpreadRampMult = 1f;
                            result.MsSpreadReturnDelay = WeaponTemplateModifier(result.MsSpreadReturnDelay, moduleModifiers.MsSpreadReturnDelay);
                            result.MsSpreadReturn = WeaponTemplateModifier(result.MsSpreadReturn, moduleModifiers.MsSpreadReturn);
                            result.NoSpreadChance = WeaponTemplateModifier(result.NoSpreadChance, moduleModifiers.NoSpreadChance, moduleModifiers.NoSpreadChanceMult);
                            result.Agility = WeaponTemplateModifier(result.Agility, moduleModifiers.Agility);
                            result.MsAgilityReturn = WeaponTemplateModifier(result.MsAgilityReturn, moduleModifiers.MsAgilityReturn);
                            result.MsAgilityReturnDelay = WeaponTemplateModifier(result.MsAgilityReturnDelay, moduleModifiers.MsAgilityReturnDelay);
                            result.MsReturn = WeaponTemplateModifier(result.MsReturn, moduleModifiers.MsReturn);
                        }
                    }
                }
            }
        }

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

    // Applies item modifiers as value = mult*value + add.
    private static sbyte WeaponTemplateModifier(sbyte baseValue, sbyte? modifierValue, float? multiplierValue = 1)
    {
        return (sbyte)((baseValue * (multiplierValue ?? 1)) + (modifierValue ?? 0));
    }

    private static byte WeaponTemplateModifier(byte baseValue, sbyte? modifierValue, float? multiplierValue = 1)
    {
        return (byte)((baseValue * (multiplierValue ?? 1)) + (modifierValue ?? 0));
    }

    private static uint WeaponTemplateModifier(uint baseValue, int? modifierValue, float? multiplierValue = 1)
    {
        return (uint)((baseValue * (multiplierValue ?? 1)) + (modifierValue ?? 0));
    }

    private static int WeaponTemplateModifier(int baseValue, int? modifierValue, float? multiplierValue = 1)
    {
        return (int)((baseValue * (multiplierValue ?? 1)) + (modifierValue ?? 0));
    }

    private static ushort WeaponTemplateModifier(ushort baseValue, short? modifierValue, float? multiplierValue = 1)
    {
        return (ushort)((baseValue * (multiplierValue ?? 1)) + (modifierValue ?? 0));
    }

    private static float WeaponTemplateModifier(float baseValue, float? modifierValue, float? multiplierValue = 1)
    {
        return (float)((baseValue * (multiplierValue ?? 1)) + (modifierValue ?? 0));
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
    public float MinSpreadFrac;
    public uint MsRiseReturnDelay;
    public float RunSpreadRampMult;
    public float JumpSpreadRampMult;
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
    public HullSegmentDef HullSegment;
    public uint DriverPoseFile;
    public uint PasengerPoseFile;
    public Vector3 PassengerPoseOffset;
    public Vector3 DriverPoseOffset;
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