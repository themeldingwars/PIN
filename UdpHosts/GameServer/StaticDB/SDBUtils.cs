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
            ScopeRange = 300,
            SpawnHeight = 1,
            SpawnAbility = 0,
            DespawnAbility = 0,
            HasDriverSeat = false,
            DriverPosture = 0,
            MaxPassengers = 0,
            PassengerPosture = 0,
            HasActivePassenger = false,
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
            // We don't know how to identify the type, so we look each one up in every table of interest.
            // Not sure if the sdb_guid somehow hints at where to find it
            var componentId = baseComponent.Id;
            var scopingComponent = SDBInterface.GetScopingComponentDef(componentId);
            var driverComponent = SDBInterface.GetDriverComponentDef(componentId);
            var passengerComponent = SDBInterface.GetPassengerComponentDef(componentId);
            var abilityComponent = SDBInterface.GetAbilityComponentDef(componentId);
            var damageComponent = SDBInterface.GetDamageComponentDef(componentId);
            var statusEffectComponent = SDBInterface.GetStatusEffectComponentDef(componentId);
            var turretComponent = SDBInterface.GetTurretComponentDef(componentId);
            var deployableComponent = SDBInterface.GetDeployableComponentDef(componentId);
            var spawnPointComponent = SDBInterface.GetSpawnPointComponentDef(componentId);

            if (scopingComponent != null)
            {
                result.ScopeRange = scopingComponent.ScopeRange;
                result.SpawnHeight = scopingComponent.SpawnHeight;
                result.SpawnAbility = scopingComponent.SpawnAbility;
                result.DespawnAbility = scopingComponent.DespawnAbility;
            }

            if (driverComponent != null)
            {
                result.HasDriverSeat = true;
                result.DriverPosture = driverComponent.Posture;
            }

            if (passengerComponent != null)
            {
                result.MaxPassengers = passengerComponent.MaxPassengers;
                result.PassengerPosture = passengerComponent.Posture;
                result.HasActivePassenger = passengerComponent.ActivePassenger == 1;
            }

            if (abilityComponent != null)
            {
                result.Abilities.Add(abilityComponent);
            }

            if (damageComponent != null)
            {
                result.DeathAbility = damageComponent.DeathAbility;
                result.MaxHitPoints = damageComponent.MaxHitPoints;
                result.DamageResponse = damageComponent.DamageResponse;
            }

            if (statusEffectComponent != null)
            {
                result.StatusFxId = statusEffectComponent.StatusFxId;
            }

            if (turretComponent != null)
            {
                result.Turrets.Add(turretComponent);
            }

            if (deployableComponent != null)
            {
                result.Deployables.Add(deployableComponent);
            }

            if (spawnPointComponent != null)
            {
                // TODO: Probably for allowing spawning into the vehicle
            }
        }

        return result;
    }
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