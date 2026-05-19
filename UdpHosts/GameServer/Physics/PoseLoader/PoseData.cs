#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using GameServer.Physics.IniLoader;
using Serilog;
using static GameServer.Physics.PoseLoader.PoseUtil;

namespace GameServer.Physics.PoseLoader;

public class PoseData
{
    public required string Name;
    public required Dictionary<string, ShapeDef> Shapes;

    public enum ShapeType
    {
        Sphere,
        Cylinder,
        Capsule,
        Triangle,
        Box,
        HKX,
    }

    public static PoseData LoadFromIni(IniData ini, ILogger logger)
    {
        /*
        foreach (var (section, data) in ini.Sections)
        {
            logger.Debug("Section {section}", section);
        }
        */

        var file = ini.Sections.GetValueOrDefault("File") ?? throw new InvalidDataException(".pose file did not contain a File section");
        if (file.GetValueOrDefault("Version") != "1" || file.GetValueOrDefault("Type") != "Pose" || file.GetValueOrDefault("Name") == null)
        {
            foreach (var (field, value) in file)
            {
                logger.Debug("File field {field} - {value}", field, value);
            }

            throw new InvalidDataException(".pose file invalid File section");
        }

        var result = new PoseData
        {
            Name = file.GetValueOrDefault("Name") ?? string.Empty,
            Shapes = MapShapes(ini)
        };

        return result;
    }

    public static Dictionary<string, ShapeDef> MapShapes(IniData ini)
    {
        var shapes = new Dictionary<string, ShapeDef>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in ini.Sections)
        {
            if (!section.Key.StartsWith("Shape-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string name = section.Key["Shape-".Length..]; // Remove "Shape-" prefix
            var values = section.Value;
            var type = Enum.TryParse<ShapeType>(values.GetValueOrDefault("Type")?.Trim('"'), true, out var typeValue)
                    ? typeValue
                    : throw new InvalidOperationException($"Unknown shape type: {typeValue}");
            var flags = ParseFlags(values.GetValueOrDefault("Flags") ?? string.Empty);
            var material = TryParseInt(values.GetValueOrDefault("Material")) ?? 0;
            var damageMod = values.TryGetValue("DamageMod", out var dmgStr) ? TryParseFloat(dmgStr) ?? 0 : -1.0f;
            var origin = ParseVector3(values.GetValueOrDefault("Origin") ?? "<0 0 0>");
            var rotation = values.TryGetValue("Rotation", out var rotStr) ? ParseRotation(rotStr) : Quaternion.Identity;
            var hitTagType = values.TryGetValue("HitTagType", out var hitStr) ? hitStr : string.Empty;

            ShapeDef shape = type switch
            {
                ShapeType.Sphere => new SphereShapeDef
                {
                    Name = name,
                    Flags = flags,
                    Material = material,
                    DamageMod = damageMod,
                    HitTagType = hitTagType,
                    Origin = origin,
                    Rotation = rotation,
                    Radius = TryParseFloat(values.GetValueOrDefault("Radius")) ?? throw new InvalidOperationException("Sphere requires Radius"),
                },

                ShapeType.Cylinder => new CylinderShapeDef
                {
                    Name = name,
                    Flags = flags,
                    Material = material,
                    DamageMod = damageMod,
                    HitTagType = hitTagType,
                    Origin = origin,
                    Rotation = rotation,
                    Height = TryParseFloat(values.GetValueOrDefault("Height")) ?? throw new InvalidOperationException("Cylinder requires Height"),
                    Radius = TryParseFloat(values.GetValueOrDefault("Radius")) ?? throw new InvalidOperationException("Cylinder requires Radius"),
                },

                ShapeType.Capsule => new CapsuleShapeDef
                {
                    Name = name,
                    Flags = flags,
                    Material = material,
                    DamageMod = damageMod,
                    HitTagType = hitTagType,
                    Origin = origin,
                    Rotation = rotation,
                    Height = TryParseFloat(values.GetValueOrDefault("Height")) ?? throw new InvalidOperationException("Capsule requires Height"),
                    Radius = TryParseFloat(values.GetValueOrDefault("Radius")) ?? throw new InvalidOperationException("Capsule requires Radius"),
                },

                ShapeType.Triangle => new TriangleShapeDef
                {
                    Name = name,
                    Flags = flags,
                    Material = material,
                    DamageMod = damageMod,
                    HitTagType = hitTagType,
                    Origin = origin,
                    Rotation = rotation,
                    Vertex0 = values.TryGetValue("Vertex0", out var vert1Str) ? ParseVector3(vert1Str) : throw new InvalidOperationException("Triangle requires Vertex0"),
                    Vertex1 = values.TryGetValue("Vertex1", out var vert2Str) ? ParseVector3(vert2Str) : throw new InvalidOperationException("Triangle requires Vertex1"),
                    Vertex2 = values.TryGetValue("Vertex2", out var vert3Str) ? ParseVector3(vert3Str) : throw new InvalidOperationException("Triangle requires Vertex2"),
                },

                ShapeType.Box => new BoxShapeDef
                {
                    Name = name,
                    Flags = flags,
                    Material = material,
                    DamageMod = damageMod,
                    HitTagType = hitTagType,
                    Origin = origin,
                    Rotation = rotation,
                    Extents = values.TryGetValue("Extents", out var vert3Str) ? ParseVector3(vert3Str) : throw new InvalidOperationException("Box requires Extents"),
                },

                ShapeType.HKX => new HKXShapeDef
                {
                    Name = name,
                    Flags = flags,
                    Material = material,
                    DamageMod = damageMod,
                    HitTagType = hitTagType,
                    Origin = origin,
                    Rotation = rotation,
                    Filename = TryParseFilename(values.GetValueOrDefault("Filename")?.Trim('"') ?? string.Empty),
                },

                _ => throw new InvalidOperationException($"Unknown type '{type}'")
            };

            shapes[name] = shape;
        }

        return shapes;
    }

    // "00213000\\00213550.HKX";
    public static string TryParseFilename(string input)
    {
        int lastIndex = input.LastIndexOf('\\');
        if (lastIndex >= 0)
        {
            return Path.GetFileNameWithoutExtension(input[(lastIndex + 1)..]);
        }

        return string.Empty;
    }

    public static ShapeFlags ParseFlags(string flagString)
    {
        var flags = flagString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new ShapeFlags();

        foreach (var flag in flags)
        {
            switch (flag.ToUpperInvariant())
            {
                case "HEADSHOT":
                    result.Headshot = true;
                    break;
                case "SHIELD":
                    result.Shield = true;
                    break;
                case "AREAONLY":
                    result.AreaOnly = true;
                    break;
                case "SHOOTOUT":
                    result.ShootOut = true;
                    break;
                case "SHOOTIN":
                    result.ShootIn = true;
                    break;
                case "SHOOTTHROUGH":
                    result.ShootThrough = true;
                    break;
                case "INTERACT_ONLY":
                    result.InteractOnly = true;
                    break;
                case "NON_INTERACTIVE":
                    result.NonInteractive = true;
                    break;
                case "TRANSPARENT":
                    result.Transparent = true;
                    break;
                case "BOUNCE_BULLETS":
                    result.BounceBullets = true;
                    break;
                case "REFRACT_BULLETS":
                    result.RefractBullets = true;
                    break;
                case "NPC_WARNING":
                    result.NpcWarning = true;
                    break;
                case "RAGDOLL_QUERY_ONLY":
                    result.RagdollQueryOnly = true;
                    break;
                case "NPC_QUERY_ONLY":
                    result.NpcQueryOnly = true;
                    break;
                case "IGNORE_PROJECTILES":
                    result.IgnoreProjectiles = true;
                    break;
            }
        }

        return result;
    }

    public class ShapeFlags
    {
        public bool Headshot { get; set; }
        public bool Shield { get; set; }
        public bool AreaOnly { get; set; }
        public bool ShootOut { get; set; }
        public bool ShootIn { get; set; }
        public bool ShootThrough { get; set; }
        public bool InteractOnly { get; set; }
        public bool NonInteractive { get; set; }
        public bool Transparent { get; set; }
        public bool BounceBullets { get; set; }
        public bool RefractBullets { get; set; }
        public bool NpcWarning { get; set; }
        public bool RagdollQueryOnly { get; set; }
        public bool NpcQueryOnly { get; set; }
        public bool IgnoreProjectiles { get; set; }
    }

    public abstract record ShapeDef
    {
        public required string Name = string.Empty;
        public required float DamageMod = -1f; // Unsure about the default value here
        public required string HitTagType = string.Empty;
        public required ShapeFlags Flags = new();
        public required int Material;

        // These are parsed by type but present on all objects (except for Sphere which doesn't have the rotation)
        public required Vector3 Origin = Vector3.Zero;
        public required Quaternion Rotation = Quaternion.Identity;
    }

    public record SphereShapeDef : ShapeDef
    {
        public required float Radius;
    }

    public record CylinderShapeDef : ShapeDef
    {
        public required float Height;
        public required float Radius;
    }

    public record CapsuleShapeDef : ShapeDef
    {
        public required float Height;
        public required float Radius;
    }

    public record TriangleShapeDef : ShapeDef
    {
        public required Vector3 Vertex0;
        public required Vector3 Vertex1;
        public required Vector3 Vertex2;
    }

    public record BoxShapeDef : ShapeDef
    {
        public required Vector3 Extents;
    }

    public record HKXShapeDef : ShapeDef
    {
        public string Filename = string.Empty;
    }
}