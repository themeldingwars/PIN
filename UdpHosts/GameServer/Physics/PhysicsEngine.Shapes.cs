#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using GameServer.Entities.Character;
using GameServer.Physics.PoseLoader;
using static GameServer.Physics.PoseLoader.PoseData;

namespace GameServer.Physics;

public struct CompoundCacheEntry
{
    public TypedIndex ShapeIndex;
}

public struct ActivePoseShapeData
{
    public TypedIndex ShapeId;
    public ShapeFlags ShapeFlags = new();
    public int Material;
    public string Name = string.Empty;
    public float DamageMod = 1.0f;
    public string HitTagType = "Default";

    public ActivePoseShapeData()
    {
    }
}

/// <summary>
///    Creating physics shapes from posefiles
/// </summary>
public partial class PhysicsEngine
{
    /// <summary>
    ///     AssetId + Scale to Shape (TypedIndex) cache
    ///     This ensures we don't store duplicates of the same shapes, well, except for the actual primitives used... :thinking:
    /// </summary>
    private readonly Dictionary<AssetCompoundKey, CompoundCacheEntry> _compoundCache = [];

    /// <summary>
    ///     (Pose) AssetId to Compound based metadata (Same for all Scales)
    ///     This is so that we can look up the source Pose data. It's dependant on the Compound generation for the child index, but those will be the same for all Scales.
    /// </summary>
    private readonly Dictionary<uint, Dictionary<int, ActivePoseShapeData>> _assetIdToPoseCompoundData = [];

    /// <summary>
    ///     Compound/Shape (TypedIndex) to AssetId
    ///     When we have a hit and want to go look up the extra data, we need the asset id.
    /// </summary>
    private readonly Dictionary<TypedIndex, uint> _poseCompoundToAssetId = new();

    public (CompoundCacheEntry, Dictionary<int, ActivePoseShapeData>) CreateActivePose(PoseData poseDef, Vector3 offset, float scale = 1f)
    {
        var result = new Dictionary<int, ActivePoseShapeData>();
        var builder = new CompoundBuilder(BufferPool, Simulation.Shapes, poseDef.Shapes.Capacity);
        QuaternionEx.GetQuaternionBetweenNormalizedVectors(Vector3.UnitY, Vector3.UnitZ, out Quaternion rotateYToZ);
        int childIndex = 0; // We need this for lookups so we manually track it...
        foreach (var (name, shapeDef) in poseDef.Shapes)
        {
            if (name == "CatchAll" || name == "NPCWarning")
            {
                // TODO: CatchAll and NPCWarning should have some separate collision check but don't know how it should work yet
                continue;
            }

            TypedIndex shapeId;
            var pose = RigidPose.Identity;
            pose.Position = shapeDef.Origin;

            if (shapeDef is HKXShapeDef hkx)
            {
                _logger.Debug("Pose {PoseFileName} - {ShapeName} is HKX shape {Filename}", poseDef.Name, name, hkx.Filename);

                var assetId = hkx.Filename;
                var assetPath = Path.Combine(_assetsPath, $"{assetId}.pinasset.json");
                var statics = TagfileLoader.LoadRigidBody(Vector3.Zero, assetPath);
                for (int i = 0; i < statics.Length; i++)
                {
                    var stat = statics[i];
                    var statPose = stat.Pose;
                    statPose.Position += shapeDef.Origin * scale; // NOTE: Not sure if hkx shapes in .pose files can reference a rotation?
                    var statShapeId = stat.Shape;

                    builder.AddForKinematic(statShapeId, statPose, 1);
                    result.Add(childIndex++, new ActivePoseShapeData()
                    {
                        DamageMod = shapeDef.DamageMod,
                        HitTagType = shapeDef.HitTagType,
                        Material = shapeDef.Material,
                        Name = shapeDef.Name,
                        ShapeFlags = shapeDef.Flags,
                        ShapeId = statShapeId,
                    });
                }
            }
            else
            {
                switch (shapeDef)
                {
                    case CapsuleShapeDef capsule:
                        var capsuleRadius = capsule.Radius * scale;
                        var capsuleHeight = capsule.Height * scale;
                        shapeId = Simulation.Shapes.Add(new Capsule(capsuleRadius, capsuleHeight));
                        pose.Position = shapeDef.Origin * scale;
                        pose.Orientation = shapeDef.Rotation * rotateYToZ;
                        break;
                    case CylinderShapeDef cylinder:
                        var cylinderRadius = cylinder.Radius * scale;
                        var cylinderHeight = cylinder.Height * scale;
                        shapeId = Simulation.Shapes.Add(new Cylinder(cylinderRadius, cylinderHeight));
                        pose.Position = shapeDef.Origin * scale;
                        pose.Orientation = shapeDef.Rotation * rotateYToZ;
                        break;
                    case SphereShapeDef sphere:
                        shapeId = Simulation.Shapes.Add(new Sphere(sphere.Radius * scale));
                        pose.Position = shapeDef.Origin * scale;
                        break;
                    case TriangleShapeDef triangle:
                        shapeId = Simulation.Shapes.Add(new Triangle(triangle.Vertex0, triangle.Vertex1, triangle.Vertex2));
                        break;
                    case BoxShapeDef box:
                        shapeId = Simulation.Shapes.Add(new Box(box.Extents.X, box.Extents.Y, box.Extents.Z));
                        pose.Position = shapeDef.Origin * scale;
                        pose.Orientation = shapeDef.Rotation;
                        break;
                    default:
                        _logger.Debug("Unhandled shape", shapeDef);
                        shapeId = Simulation.Shapes.Add(new Sphere(1));
                        break;
                }

                builder.AddForKinematic(shapeId, pose, 1);

                // FIXME: The HKX route will add multiple children so the child index no longer aligns with the shape defs
                result.Add(childIndex++, new ActivePoseShapeData()
                {
                    DamageMod = shapeDef.DamageMod,
                    HitTagType = shapeDef.HitTagType,
                    Material = shapeDef.Material,
                    Name = shapeDef.Name,
                    ShapeFlags = shapeDef.Flags,
                    ShapeId = shapeId,
                });
            }
        }

        builder.BuildKinematicCompound(out var children, out Vector3 center);
        var compound = new Compound(children);

        // Origin at bottom
        Vector3 origin = new Vector3(0, 0, center.Z);
        for (int i = 0; i < childIndex; ++i)
        {
            ref var child = ref compound.Children[i];
            child.LocalPosition += origin + offset;
        }

        var entry = new CompoundCacheEntry
        {
            ShapeIndex = Simulation.Shapes.Add(compound)
        };

        return (entry, result);
    }

    public TypedIndex GetAssetShape(uint assetId, Vector3 offset, float scale = 1f)
    {
        var key = new AssetCompoundKey(assetId, offset, scale);
        return GetAssetShape(key);
    }

    public TypedIndex GetAssetShape(AssetCompoundKey key)
    {
        if (_compoundCache.TryGetValue(key, out var cacheEntry))
        {
            return cacheEntry.ShapeIndex;
        }

        var ok = PoseLoader.TryLoad(key.AssetId.ToString("D8"), out var poseDef);
        if (ok && poseDef != null)
        {
            var (entry, result) = CreateActivePose(poseDef, key.Offset, key.Scale);
            _compoundCache[key] = entry;
            _assetIdToPoseCompoundData.TryAdd(key.AssetId, result);
            _poseCompoundToAssetId.Add(entry.ShapeIndex, key.AssetId);
            return entry.ShapeIndex;
        }

        _logger.Debug("Returning fallback shape for assetId {assetId}", key.AssetId);
        return _fallbackShape;
    }

    public AssetCompoundKey GetCharacterPoseAsset(CharacterEntity character)
    {
        var mov = character.MovementStateContainer;
        var movestate = character.MovementStateContainer.Movestate;
        var info = character.Collision;

        var collisionId = info.PoseTypeRecord.StandingCollisionid;
        var offset = Vector3.Zero;
        var scale = character.Collision.Scale;

        if (info.AttachmentPoseId != 0)
        {
            collisionId = info.AttachmentPoseId;
            offset = info.AttachmentPoseOffset;
        }
        else if (info.PoseTypeRecord.PoseId == 0)
        {
            // PoseTypeRecord 0 provides no collision ids so let's look at the visual record instead
            if (info.HitboxCollisionId != 0)
            {
                collisionId = info.HitboxCollisionId;
            }
            else if (info.RagdollCollisionId != 0)
            {
                collisionId = info.RagdollCollisionId;
            }
            else
            {
                _logger.Warning("No suitable collisionId found during GetCharacterShape");
            }
        }
        else if (movestate == Movestate.Glider || movestate == Movestate.GliderThrusters || movestate == Movestate.GliderStalling)
        {
            collisionId = info.PoseTypeRecord.ProneCollisionid;
        }
        else if (movestate == Movestate.Falling)
        {
            collisionId = info.PoseTypeRecord.FallingCollisionid;
        }
        else if (movestate == Movestate.Knockdown || movestate == Movestate.KnockdownFalling)
        {
            collisionId = info.PoseTypeRecord.ProneCollisionid;
        }
        else if (mov.Crouch)
        {
            collisionId = info.PoseTypeRecord.CrouchedCollisionid;
        }
        else if (mov.Sprint)
        {
            collisionId = info.PoseTypeRecord.SprintingCollisionid;
        }
        else if (movestate == Movestate.Running)
        {
            collisionId = info.PoseTypeRecord.RunningCollisionid;
        }

        return new AssetCompoundKey(collisionId, offset, scale);
    }

    public TypedIndex GetCharacterShape(CharacterEntity character)
    {
        var info = character.Collision;
        if (info == null)
        {
            _logger.Debug("GetCharacterShape but no CollisionComponent");
            return _fallbackShape;
        }

        AssetCompoundKey key = GetCharacterPoseAsset(character);
        return GetAssetShape(key);
    }
}