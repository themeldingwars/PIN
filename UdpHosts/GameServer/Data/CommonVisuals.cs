using GameServer.Enums.Visuals;
using GameServer.Packets.GSS.Character.BaseController;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Data;

public class CommonVisuals
{
    public CommonVisuals()
    {
        Decals = new List<Decal>();
        DecalGradients = new List<DecalGradient>();
        Colors = new List<uint>();
        Palettes = new List<Palette>();
        Patterns = new List<Pattern>();
        OrnamentGroups = new List<uint>();

        // Below are likely Morph Weights, Overlays and Pattern Gradients?
        ItemsUnk1 = new List<uint>();
        ItemsUnk2 = new List<uint>();
        ItemsUnk3 = new List<uint>();
    }

    public IList<Decal> Decals { get; protected set; }
    public IList<DecalGradient> DecalGradients { get; protected set; }
    public IList<uint> Colors { get; protected set; }
    public IList<Palette> Palettes { get; protected set; }
    public IList<Pattern> Patterns { get; protected set; }
    public IList<uint> OrnamentGroups { get; protected set; }

    // Below are likely Morph Weights, Overlays and Pattern Gradients?
    public IList<uint> ItemsUnk1 { get; protected set; }
    public IList<uint> ItemsUnk2 { get; protected set; }
    public IList<uint> ItemsUnk3 { get; protected set; }

    public static implicit operator KeyFrame.Visuals(CommonVisuals p)
    {
        return new KeyFrame.Visuals
               {
                   Decals = p.Decals.Select(x => (KeyFrame.Decal)x).ToList(),
                   DecalGradients = p.DecalGradients.Select(x => (KeyFrame.DecalGradient)x).ToList(),
                   Colors = p.Colors,
                   Palettes = p.Palettes.Select(x => (KeyFrame.Palette)x).ToList(),
                   Patterns = p.Patterns.Select(x => (KeyFrame.Pattern)x).ToList(),
                   OrnamentGroups = p.OrnamentGroups,
                   ItemsUnk1 = p.ItemsUnk1,
                   ItemsUnk2 = p.ItemsUnk2,
                   ItemsUnk3 = p.ItemsUnk3
               };
    }


    public class Palette
    {
        public PaletteType Type;
        public uint ID;

        public static implicit operator KeyFrame.Palette(Palette p)
        {
            return new KeyFrame.Palette { Type = (byte)p.Type, ID = p.ID };
        }
    }

    public class Pattern
    {
        public uint ID;
        public IList<Half> Transform = new List<Half>();
        public byte Usage;

        public static implicit operator KeyFrame.Pattern(Pattern p)
        {
            return new KeyFrame.Pattern { ID = p.ID, Transform = p.Transform, Usage = p.Usage };
        }
    }

    public class Decal
    {
        public uint ID;
        public uint Color;
        public IList<Half> Transform = new List<Half>();
        public byte Usage;

        public static implicit operator KeyFrame.Decal(Decal p)
        {
            return new KeyFrame.Decal { ID = p.ID, Color = p.Color, Transform = p.Transform, Usage = p.Usage };
        }
    }

    public class DecalGradient
    {
        public byte Unknown;

        public static implicit operator KeyFrame.DecalGradient(DecalGradient p)
        {
            return new KeyFrame.DecalGradient { Unk = p.Unknown };
        }
    }
}