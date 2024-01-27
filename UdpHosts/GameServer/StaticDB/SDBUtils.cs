namespace GameServer.Data.SDB;

using System.Collections.Generic;
using FauFau.Formats;
using Records.dbitems;
using Records.dbviusalrecords;
using Records.apt;
using Records.aptfs;
using Records;
using System;
using AeroMessages.GSS.V66.Character;
using System.Linq;
using global::GameServer.Data.SDB.Records.dbcharacter;

public class SDBUtils
{
    public static uint GetChassisDefaultBackpack(uint chassisId)
    {
        var defaultLoadout = SDBInterface.GetCharCreateLoadoutsByFrame(chassisId)
        .First(); // yolo
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
    public static uint   CombineLightDark(uint light, uint dark) => ARGB8888ToRGB565(dark) | (uint) (ARGB8888ToRGB565(light) << 16);
    public static uint   ExtractLight(uint     combined) => RGB565ToARGB8888((ushort) (combined >> 16));
    public static uint   ExtractDark(uint      combined) => RGB565ToARGB8888((ushort) combined);
    public static ushort ARGB8888ToRGB565(uint argb)     => (ushort) (((((byte) (argb >> 16) >> 3) & 0x1f) << 11) | ((((byte) (argb >> 8) >> 2) & 0x3f) << 5) | (((byte) argb >> 3) & 0x1f));

    public static uint RGB565ToARGB8888(ushort rgb)
    {
        int r = (rgb            >> 11) * 255 + 16;
        int g = ((rgb & 0x07E0) >> 5)  * 255 + 32;
        int b = (rgb & 0x001F)         * 255 + 16;
        return (uint) (0xFF000000 | (byte) ((r / 32 + r) / 32) << 16 | (byte) ((g / 64 + g) / 64) << 8 | (byte) ((b / 32 + b) / 32));
    }
}