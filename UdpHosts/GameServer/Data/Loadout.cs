using System;
using System.Collections.Generic;
using GameServer.Enums.Visuals;

namespace GameServer.Data;

public class Loadout
{
    public Loadout()
    {
        VehicleID = 77087;
        GliderID = 81423;
        ChassisID = 76331;
        BackpackID = 78041;
        PrimaryWeaponID = 134616;
        SecondaryWeaponID = 114316;

        ChassisModules = new List<uint>();
        ChassisModules.Add(126575);
        ChassisModules.Add(125845);
        ChassisModules.Add(128036);
        ChassisModules.Add(128766);
        ChassisModules.Add(127306);
        ChassisModules.Add(129202);
        ChassisModules.Add(130419);
        ChassisModules.Add(142078);
        ChassisModules.Add(85817);
        ChassisModules.Add(85818);
        ChassisModules.Add(85956);
        ChassisModules.Add(85976);
        ChassisModules.Add(86067);
        ChassisModules.Add(86137);
        ChassisModules.Add(86139);
        ChassisModules.Add(118819);
        ChassisModules.Add(124247);
        ChassisModules.Add(140713);

        BackpackModules = new List<AbilityModule>();
        BackpackModules.Add(AbilityModule.Load(101940, 0));
        // BackpackModules.Add(AbilityModule.Load(143330, 0));
        BackpackModules.Add(AbilityModule.Load(136056, 1));
        BackpackModules.Add(AbilityModule.Load(113552, 2));
        BackpackModules.Add(AbilityModule.Load(113931, 3));
        BackpackModules.Add(AbilityModule.Load(129458, 5));
        BackpackModules.Add(AbilityModule.Load(129056, 6));

        // PrimaryWeaponModules = new List<WeaponModule>();
        // PrimaryWeaponModules.Add(new WeaponModule(118581));
        PrimaryWeaponVisuals = new CommonVisuals();
        PrimaryWeaponVisuals.Colors.Add(0x21040000u);
        PrimaryWeaponVisuals.Colors.Add(0x4a4910a2u);
        PrimaryWeaponVisuals.Colors.Add(0x916510a2u);

        PrimaryWeaponVisuals.Palettes.Add(new CommonVisuals.Palette { ID = 117006, Type = PaletteType.WeaponA });

        PrimaryWeaponVisuals.Patterns.Add(new CommonVisuals.Pattern { ID = 10192, Usage = 2, Transform = new[] { (Half)(ushort)12743u, (Half)(ushort)2753u, (Half)(ushort)2042u, (Half)(ushort)3676u } });

        PrimaryWeaponVisuals.OrnamentGroups.Add(10283);
        PrimaryWeaponVisuals.OrnamentGroups.Add(10339);

        // SecondaryWeaponModules = new List<WeaponModule>();
        SecondaryWeaponVisuals = new CommonVisuals();

        SecondaryWeaponVisuals.Colors.Add(0x96070000u);
        SecondaryWeaponVisuals.Colors.Add(0x44ac0000u);
        SecondaryWeaponVisuals.Colors.Add(0x00000000u);

        SecondaryWeaponVisuals.Palettes.Add(new CommonVisuals.Palette { ID = 106479, Type = PaletteType.WeaponB });

        SecondaryWeaponVisuals.OrnamentGroups.Add(10283);
    }

    public uint GUID { get; set; }

    public uint VehicleID { get; set; }
    public uint GliderID { get; set; }
    public uint ChassisID { get; set; }
    public uint BackpackID { get; set; }
    public uint PrimaryWeaponID { get; set; }
    public uint SecondaryWeaponID { get; set; }

    public IList<uint> ChassisModules { get; protected set; }

    public IList<AbilityModule> BackpackModules { get; protected set; }

    // public IList<WeaponModule> PrimaryWeaponModules { get; protected set; }
    public CommonVisuals PrimaryWeaponVisuals { get; protected set; }

    // public IList<WeaponModule> SecondaryWeaponModules { get; protected set; }
    public CommonVisuals SecondaryWeaponVisuals { get; protected set; }

    public static Loadout Load(uint guid)
    {
        return new Loadout { GUID = guid };
    }
}