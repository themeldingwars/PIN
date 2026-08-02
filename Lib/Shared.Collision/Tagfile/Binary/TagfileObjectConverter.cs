#nullable enable
using System.Numerics;
using Shared.Collision.Tagfile.Models;

namespace Shared.Collision.Tagfile.Binary;

public static class TagfileObjectConverter
{
    public static Dictionary<string, BaseTagfileObject> Convert(DictionaryTagfile result, out List<TagfileDictObject> raw)
    {
        raw = result.Raw;
        var dict = new Dictionary<string, BaseTagfileObject>(result.Processed.Count);
        var rawDataMap = new Dictionary<string, Dictionary<string, object?>>();
        foreach (var obj in result.Processed)
        {
            rawDataMap[obj.Name] = obj.Data;
        }

        foreach (var obj in result.Processed)
        {
            dict[obj.Name] = ConvertObject(obj, dict, rawDataMap);
        }

        return dict;
    }

    public static Dictionary<string, BaseTagfileObject> ConvertObjects(IEnumerable<TagfileDictObject> objects)
    {
        var dict = new Dictionary<string, BaseTagfileObject>(objects.Count());
        var rawDataMap = new Dictionary<string, Dictionary<string, object?>>();
        foreach (var obj in objects)
        {
            rawDataMap[obj.Name] = obj.Data;
        }

        foreach (var obj in objects)
        {
            dict[obj.Name] = ConvertObject(obj, dict, rawDataMap);
        }

        return dict;
    }

    private static BaseTagfileObject ConvertObject(TagfileDictObject obj, Dictionary<string, BaseTagfileObject> allObjects, Dictionary<string, Dictionary<string, object?>> rawDataMap)
    {
        var data = obj.Data;

        return obj.Class switch
        {
            "hkpListShape" => ConvertHkpListShape(obj, data),
            "hkpMoppBvTreeShape" => ConvertHkpMoppBvTreeShape(obj, data),
            "hkpConvexTranslateShape" => ConvertHkpConvexTranslateShape(obj, data),
            "hkpBoxShape" => ConvertHkpBoxShape(obj, data),
            "hkpSphereShape" => ConvertHkpSphereShape(obj, data),
            "hkpCapsuleShape" => ConvertHkpCapsuleShape(obj, data),
            "hkpCylinderShape" => ConvertHkpCylinderShape(obj, data),
            "hkpTransformShape" => ConvertHkpTransformShape(obj, data),
            "hkpConvexTransformShape" => ConvertHkpConvexTransformShape(obj, data),
            "hkpConvexVerticesShape" => ConvertHkpConvexVerticesShape(obj, data),
            "hkpExtendedMeshShape" => ConvertHkpExtendedMeshShape(obj, data, allObjects, rawDataMap),
            "hkRootLevelContainer" => ConvertHkRootLevelContainer(obj, data),
            "hkpRigidBody" => ConvertHkpRigidBody(obj, data),
            "hkpStorageExtendedMeshShape" => ConvertHkpStorageExtendedMeshShape(obj, data, allObjects, rawDataMap),
            "hkpStorageExtendedMeshShapeMeshSubpartStorage" => ConvertHkpStorageExtendedMeshShapeMeshSubpartStorage(obj, data),
            "hkpSimpleMeshShape" => ConvertHkpSimpleMeshShape(obj, data),
            "hkaRagdollInstance" => ConvertHkaRagdollInstance(obj, data),
            _ => new BaseTagfileObject { Name = obj.Name, Class = obj.Class },
        };
    }

    #region Primitive Conversions

    private static object? GetRaw(Dictionary<string, object?> data, string key)
    {
        data.TryGetValue(key, out var val);
        return val;
    }

    private static long GetLong(Dictionary<string, object?> data, string key, long @default = 0)
    {
        if (data.TryGetValue(key, out var val) && val is long l)
        {
            return l;
        }

        return @default;
    }

    private static float GetFloat(Dictionary<string, object?> data, string key, float @default = 0f)
    {
        if (data.TryGetValue(key, out var val))
        {
            if (val is double d)
            {
                return (float)d;
            }

            if (val is long l)
            {
                return l;
            }
        }

        return @default;
    }

    private static byte GetByte(Dictionary<string, object?> data, string key, byte @default = 0)
    {
        if (data.TryGetValue(key, out var val))
        {
            if (val is byte b)
            {
                return b;
            }

            if (val is long l)
            {
                return (byte)l;
            }
        }

        return @default;
    }

    private static bool GetBool(Dictionary<string, object?> data, string key, bool @default = false)
    {
        if (data.TryGetValue(key, out var val) && val is bool b)
        {
            return b;
        }

        return @default;
    }

    private static string GetString(Dictionary<string, object?> data, string key, string @default = "")
    {
        if (data.TryGetValue(key, out var val) && val is string s)
        {
            return s;
        }

        return @default;
    }

    private static Vector4 ToVector4(object? val)
    {
        if (val is List<double> ld && ld.Count >= 3)
        {
            return new Vector4((float)ld[0], (float)ld[1], (float)ld[2], ld.Count > 3 ? (float)ld[3] : 0f);
        }

        return Vector4.Zero;
    }

    private static Vector4[] ToVector4Array(object? val)
    {
        if (val is List<List<double>> ll)
        {
            var result = new Vector4[ll.Count];
            for (int i = 0; i < ll.Count; i++)
            {
                result[i] = ToVector4(ll[i]);
            }

            return result;
        }

        return [];
    }

    private static uint[] ToUintArray(object? val)
    {
        if (val is List<long> ll)
        {
            var result = new uint[ll.Count];
            for (int i = 0; i < ll.Count; i++)
            {
                result[i] = (uint)ll[i];
            }

            return result;
        }

        return [];
    }

    private static string[] ToStringArray(object? val)
    {
        if (val is List<string> ls)
        {
            return [.. ls];
        }

        if (val is object[] arr)
        {
            var result = new string[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                result[i] = arr[i]?.ToString() ?? string.Empty;
            }

            return result;
        }

        return [];
    }

    #endregion

    #region Shape Converters

    private static BaseTagfileObject ConvertHkpBoxShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkpBoxShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            Radius = GetFloat(data, "radius"),
            HalfExtents = ToVector4(GetRaw(data, "halfExtents")),
        };
    }

    private static BaseTagfileObject ConvertHkpSphereShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkpSphereShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            Radius = GetFloat(data, "radius"),
        };
    }

    private static BaseTagfileObject ConvertHkpCapsuleShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkpCapsuleShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            Radius = GetFloat(data, "radius"),
            VertexA = ToVector4(GetRaw(data, "vertexA")),
            VertexB = ToVector4(GetRaw(data, "vertexB")),
        };
    }

    private static BaseTagfileObject ConvertHkpCylinderShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkpCylinderShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            Radius = GetFloat(data, "radius"),
            CylRadius = GetFloat(data, "cylRadius"),
            CylBaseRadiusFactorForHeightFieldCollisions = GetFloat(data, "cylBaseRadiusFactorForHeightFieldCollisions"),
            VertexA = ToVector4(GetRaw(data, "vertexA")),
            VertexB = ToVector4(GetRaw(data, "vertexB")),
            Perpendicular1 = ToVector4(GetRaw(data, "perpendicular1")),
            Perpendicular2 = ToVector4(GetRaw(data, "perpendicular2")),
        };
    }

    private static BaseTagfileObject ConvertHkpConvexVerticesShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        var rv = GetRaw(data, "rotatedVertices");
        Vector4[][] rotatedVertices = [];

        if (rv is List<Dictionary<string, object?>> structList)
        {
            // hkpConvexVerticesShapeFourVectors: {x: Vector4, y: Vector4, z: Vector4}
            rotatedVertices = [.. structList.Select(d =>
                new[]
                {
                    ToVector4(GetRaw(d, "x")),
                    ToVector4(GetRaw(d, "y")),
                    ToVector4(GetRaw(d, "z")),
                })];
        }
        else if (rv is List<List<List<double>>> nested)
        {
            rotatedVertices = [.. nested.Select(row =>
            {
                var arr = new Vector4[row.Count];
                for (int i = 0; i < row.Count; i++)
                {
                    arr[i] = ToVector4(row[i]);
                }

                return arr;
            })];
        }

        return new HkpConvexVerticesShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            Radius = GetFloat(data, "radius"),
            NumVertices = (uint)GetLong(data, "numVertices"),
            RotatedVertices = rotatedVertices,
        };
    }

    #endregion

    #region Modifier Converters

    private static BaseTagfileObject ConvertHkpConvexTranslateShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkpConvexTranslateShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            Radius = GetFloat(data, "radius"),
            ChildShape = ResolveChildShapeRef(data),
            Translation = ToVector4(GetRaw(data, "translation")),
        };
    }

    private static BaseTagfileObject ConvertHkpTransformShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkpTransformShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            ChildShape = ResolveChildShapeRef(data),
            Rotation = ToVector4(GetRaw(data, "rotation")),
            Transform = ToVector4Array(GetRaw(data, "transform")),
        };
    }

    private static BaseTagfileObject ConvertHkpConvexTransformShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkpConvexTransformShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            Radius = GetFloat(data, "radius"),
            ChildShape = ResolveChildShapeRef(data),
            Transform = ToVector4Array(GetRaw(data, "transform")),
        };
    }

    #endregion

    #region Container Converters

    private static BaseTagfileObject ConvertHkpListShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        var ciRaw = GetRaw(data, "childInfo");
        var childInfo = ParseChildInfoArray(ciRaw);

        return new HkpListShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            DisableWelding = GetBool(data, "disableWelding"),
            CollectionType = GetString(data, "collectionType"),
            ChildInfo = childInfo,
            Flags = (uint)GetLong(data, "flags"),
            NumDisabledChildren = (uint)GetLong(data, "numDisabledChildren"),
            AabbHalfExtents = ToVector4(GetRaw(data, "aabbHalfExtents")),
            AabbCenter = ToVector4(GetRaw(data, "aabbCenter")),
            EnabledChildren = ToUintArray(GetRaw(data, "enabledChildren")),
        };
    }

    private static HkpListShapeObject.ChildInfoData[] ParseChildInfoArray(object? raw)
    {
        // Format from TYPE_ARRAY of TYPE_STRUCT: single dict with parallel arrays
        // { "shape": object[], "collisionFilterInfo": object[] }
        if (raw is Dictionary<string, object?> structDict)
        {
            var shapeRaw = structDict.GetValueOrDefault("shape");
            var filterRaw = structDict.GetValueOrDefault("collisionFilterInfo");

            var shapes = ResolveShapeArray(shapeRaw);
            var filterInfos = ResolveLongArray(filterRaw);

            int count = Math.Max(shapes.Length, filterInfos.Length);
            var result = new HkpListShapeObject.ChildInfoData[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new HkpListShapeObject.ChildInfoData
                {
                    Shape = i < shapes.Length ? shapes[i] : string.Empty,
                    CollisionFilterInfo = i < filterInfos.Length ? (uint)filterInfos[i] : 0u,
                };
            }

            return result;
        }

        if (raw is List<Dictionary<string, object?>> list)
        {
            var result = new HkpListShapeObject.ChildInfoData[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                result[i] = new HkpListShapeObject.ChildInfoData
                {
                    Shape = GetString(item, "shape"),
                    CollisionFilterInfo = (uint)GetLong(item, "collisionFilterInfo"),
                };
            }

            return result;
        }

        if (raw is List<List<object>> parallelArrays)
        {
            int count = 0;
            foreach (var arr in parallelArrays)
            {
                count = Math.Max(count, arr.Count);
            }

            var shapes = Array.Empty<string>();
            var filterInfos = Array.Empty<long>();

            for (int i = 0; i < parallelArrays.Count; i++)
            {
                var arr = parallelArrays[i];
                if (i == 0)
                {
                    shapes = [.. arr.Cast<string>()];
                }
                else
                {
                    filterInfos = [.. arr.OfType<long>()];
                }
            }

            var result = new HkpListShapeObject.ChildInfoData[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new HkpListShapeObject.ChildInfoData
                {
                    Shape = i < shapes.Length ? shapes[i] : string.Empty,
                    CollisionFilterInfo = i < filterInfos.Length ? (uint)filterInfos[i] : 0u,
                };
            }

            return result;
        }

        return [];
    }

    private static string[] ResolveShapeArray(object? raw)
    {
        if (raw is object[] arr)
        {
            var result = new string[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                result[i] = ConvertShapeRef(arr[i]);
            }

            return result;
        }

        if (raw is List<object> list)
        {
            var result = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = ConvertShapeRef(list[i]);
            }

            return result;
        }

        if (raw is List<string> strList)
        {
            return [.. strList];
        }

        return [];
    }

    private static string ConvertShapeRef(object? val)
    {
        if (val is string s)
        {
            return s;
        }

        if (val is TagfileParser.ObjectData od)
        {
            return od.ClassName;
        }

        if (val != null)
        {
            return val.ToString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static long[] ResolveLongArray(object? raw)
    {
        if (raw is object[] arr)
        {
            var result = new long[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                result[i] = ToLong(arr[i]);
            }

            return result;
        }

        if (raw is List<long> list)
        {
            return [.. list];
        }

        return [];
    }

    private static long ToLong(object? val)
    {
        if (val is long l)
        {
            return l;
        }

        if (val is int i)
        {
            return i;
        }

        if (val is uint u)
        {
            return u;
        }

        if (val is ValueTuple<uint, bool> vt)
        {
            long r = vt.Item2 ? -vt.Item1 : vt.Item1;
            return r;
        }

        if (val is byte b)
        {
            return b;
        }

        return 0;
    }

    private static BaseTagfileObject ConvertHkpMoppBvTreeShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        var childRaw = GetRaw(data, "child");
        string child;
        if (childRaw is string s)
        {
            child = s;
        }
        else if (childRaw is Dictionary<string, object?> d)
        {
            child = GetString(d, "childShape");
        }
        else
        {
            child = string.Empty;
        }

        return new HkpMoppBvTreeShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            BvTreeType = GetString(data, "bvTreeType"),
            Code = GetString(data, "code"),
            Child = child,
        };
    }

    private static string ResolveChildShapeRef(Dictionary<string, object?> data)
    {
        var raw = GetRaw(data, "childShape");
        if (raw is string s)
        {
            return s;
        }

        if (raw is Dictionary<string, object?> d)
        {
            var nested = d.GetValueOrDefault("childShape") ?? d.GetValueOrDefault("shape");
            if (nested is string ns)
            {
                return ns;
            }
        }

        return string.Empty;
    }

    private static BaseTagfileObject ConvertHkRootLevelContainer(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        var nvRaw = GetRaw(data, "namedVariants");

        string[] variantRefs;

        if (nvRaw is List<Dictionary<string, object?>> structList)
        {
            // hkRootLevelContainerNamedVariant: {name: string, className: string, variant: X####}
            variantRefs = [.. structList.Select(d => GetString(d, "variant"))];
        }
        else
        {
            variantRefs = ToStringArray(nvRaw);
        }

        var variants = new HkRootLevelContainerObject.NamedVariant[variantRefs.Length];
        for (int i = 0; i < variantRefs.Length; i++)
        {
            variants[i] = new HkRootLevelContainerObject.NamedVariant { Variant = variantRefs[i] };
        }

        return new HkRootLevelContainerObject
        {
            Name = obj.Name,
            Class = obj.Class,
            NamedVariants = variants,
        };
    }

    private static BaseTagfileObject ConvertHkpRigidBody(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        var collidableRaw = GetRaw(data, "collidable");
        var motionRaw = GetRaw(data, "motion");

        var collidable = new HkpRigidBody.CollidableData
        {
            Shape = string.Empty,
        };

        if (collidableRaw is string s)
        {
            collidable.Shape = s;
        }
        else if (collidableRaw is Dictionary<string, object?> cd)
        {
            collidable.Shape = GetString(cd, "shape");
        }

        var motion = new HkpRigidBody.MotionStateData
        {
            Transform = [],
        };

        if (motionRaw is Dictionary<string, object?> md)
        {
            var motionStateRaw = GetRaw(md, "motionState");
            if (motionStateRaw is Dictionary<string, object?> msd)
            {
                motion.Transform = ToVector4Array(GetRaw(msd, "transform"));
            }
            else
            {
                motion.Transform = ToVector4Array(GetRaw(md, "transform"));
            }
        }

        return new HkpRigidBody
        {
            Name = obj.Name,
            Class = obj.Class,
            Collidable = collidable,
            Motion = motion,
        };
    }

    #endregion

    #region Mesh Converters

    private static BaseTagfileObject ConvertHkpExtendedMeshShape(TagfileDictObject obj, Dictionary<string, object?> data, Dictionary<string, BaseTagfileObject> allObjects, Dictionary<string, Dictionary<string, object?>> rawDataMap)
    {
        return new HkpExtendedMeshShapeObject
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            DisableWelding = GetString(data, "disableWelding"),
            CollectionType = GetString(data, "collectionType"),
            EmbeddedTrianglesSubpart = ParseTrianglesSubpart(GetRaw(data, "embeddedTrianglesSubpart")),
            AabbHalfExtents = ToVector4(GetRaw(data, "aabbHalfExtents")),
            AabbCenter = ToVector4(GetRaw(data, "aabbCenter")),
            NumBitsForSubpartIndex = GetByte(data, "numBitsForSubpartIndex"),
            TrianglesSubparts = ParseTrianglesSubpartArray(GetRaw(data, "trianglesSubparts")),
            ShapesSubparts = ParseShapesSubpartArray(GetRaw(data, "shapesSubparts"), rawDataMap),
            WeldingInfo = ToUintArray(GetRaw(data, "weldingInfo")),
            WeldingType = GetString(data, "weldingType"),
            DefaultCollisionFilterInfo = (uint)GetLong(data, "defaultCollisionFilterInfo"),
            CachedNumChildShapes = (uint)GetLong(data, "cachedNumChildShapes"),
            TriangleRadius = GetFloat(data, "triangleRadius"),
        };
    }

    private static HkpExtendedMeshShapeObject.TrianglesSubpart ParseTrianglesSubpart(object? raw)
    {
        if (raw is not Dictionary<string, object?> d)
        {
            return default;
        }

        return new HkpExtendedMeshShapeObject.TrianglesSubpart
        {
            Type = GetString(d, "type"),
            MaterialIndexStridingType = GetString(d, "materialIndexStridingType"),
            MaterialIndexStriding = GetString(d, "materialIndexStriding"),
            NumMaterials = (uint)GetLong(d, "numMaterials"),
            UserData = (uint)GetLong(d, "userData"),
            NumTriangleShapes = (uint)GetLong(d, "numTriangleShapes"),
            NumVertices = (uint)GetLong(d, "numVertices"),
            VertexStriding = (uint)GetLong(d, "vertexStriding"),
            TriangleOffset = (uint)GetLong(d, "triangleOffset"),
            IndexStriding = (uint)GetLong(d, "indexStriding"),
            StridingType = GetString(d, "stridingType"),
            FlipAlternateTriangles = (uint)GetLong(d, "flipAlternateTriangles"),
            Extrusion = ToVector4(GetRaw(d, "extrusion")),
            Transform = ToVector4Array(GetRaw(d, "transform")),
        };
    }

    private static HkpExtendedMeshShapeObject.TrianglesSubpart[] ParseTrianglesSubpartArray(object? raw)
    {
        if (raw is List<Dictionary<string, object?>> list)
        {
            return [.. list.Select(ParseTrianglesSubpart)];
        }

        return [];
    }

    private static string[] ResolveChildShapes(object? raw)
    {
        if (raw is string s)
        {
            return [s];
        }

        if (raw is List<string> ls)
        {
            return [.. ls];
        }

        if (raw is object[] arr)
        {
            var result = new string[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                result[i] = arr[i]?.ToString() ?? string.Empty;
            }

            return result;
        }

        if (raw is Dictionary<string, object?> d)
        {
            var nested = d.GetValueOrDefault("childShapes") ?? d.GetValueOrDefault("shape");
            return ResolveChildShapes(nested);
        }

        return [];
    }

    private static HkpExtendedMeshShapeObject.ShapesSubpart[] ParseShapesSubpartArray(object? raw, Dictionary<string, Dictionary<string, object?>> rawDataMap)
    {
        if (raw is List<Dictionary<string, object?>> list)
        {
            // Inline struct array - parse directly from dict data
            return [.. list.Select(d => new HkpExtendedMeshShapeObject.ShapesSubpart
            {
                Type = GetString(d, "type"),
                MaterialIndexStridingType = GetString(d, "materialIndexStridingType"),
                MaterialIndexStriding = GetString(d, "materialIndexStriding"),
                NumMaterials = (uint)GetLong(d, "numMaterials"),
                UserData = (uint)GetLong(d, "userData"),
                ChildShapes = ResolveChildShapes(GetRaw(d, "childShapes")),
                Rotation = ToVector4(GetRaw(d, "rotation")),
                Translation = ToVector4(GetRaw(d, "translation")),
            })];
        }

        if (raw is List<string> refs)
        {
            // Object pointer array - each reference is an hkpExtendedMeshShapeShapesSubpart object
            // Resolve each subpart ref to get its childShapes array
            var result = new HkpExtendedMeshShapeObject.ShapesSubpart[refs.Count];
            for (int i = 0; i < refs.Count; i++)
            {
                if (rawDataMap.TryGetValue(refs[i], out var subpartData))
                {
                    result[i] = new HkpExtendedMeshShapeObject.ShapesSubpart
                    {
                        ChildShapes = ResolveChildShapes(GetRaw(subpartData, "childShapes")),
                        Rotation = ToVector4(GetRaw(subpartData, "rotation")),
                        Translation = ToVector4(GetRaw(subpartData, "translation")),
                    };
                }
                else
                {
                    result[i] = default;
                }
            }

            return result;
        }

        return [];
    }

    private static BaseTagfileObject ConvertHkpStorageExtendedMeshShape(TagfileDictObject obj, Dictionary<string, object?> data, Dictionary<string, BaseTagfileObject> allObjects, Dictionary<string, Dictionary<string, object?>> rawDataMap)
    {
        var baseShape = (HkpExtendedMeshShapeObject)ConvertHkpExtendedMeshShape(obj, data, allObjects, rawDataMap);
        return new HkpStorageExtendedMeshShape
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = baseShape.UserData,
            DisableWelding = baseShape.DisableWelding,
            CollectionType = baseShape.CollectionType,
            EmbeddedTrianglesSubpart = baseShape.EmbeddedTrianglesSubpart,
            AabbHalfExtents = baseShape.AabbHalfExtents,
            AabbCenter = baseShape.AabbCenter,
            NumBitsForSubpartIndex = baseShape.NumBitsForSubpartIndex,
            TrianglesSubparts = baseShape.TrianglesSubparts,
            ShapesSubparts = baseShape.ShapesSubparts,
            WeldingInfo = baseShape.WeldingInfo,
            WeldingType = baseShape.WeldingType,
            DefaultCollisionFilterInfo = baseShape.DefaultCollisionFilterInfo,
            CachedNumChildShapes = baseShape.CachedNumChildShapes,
            TriangleRadius = baseShape.TriangleRadius,
            Meshstorage = ToStringArray(GetRaw(data, "meshstorage")),
            Shapestorage = ToStringArray(GetRaw(data, "shapestorage")),
        };
    }

    private static BaseTagfileObject ConvertHkpStorageExtendedMeshShapeMeshSubpartStorage(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkpStorageExtendedMeshShapeMeshSubpartStorage
        {
            Name = obj.Name,
            Class = obj.Class,
            Vertices = ToVector4Array(GetRaw(data, "vertices")),
            Indices8 = ToUintArray(GetRaw(data, "indices8")),
            Indices16 = ToUintArray(GetRaw(data, "indices16")),
            Indices32 = ToUintArray(GetRaw(data, "indices32")),
            MaterialIndices = ToUintArray(GetRaw(data, "materialIndices")),
        };
    }

    private static BaseTagfileObject ConvertHkpSimpleMeshShape(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        var triRaw = GetRaw(data, "triangles");
        var triangles = ParseTriangleDataArray(triRaw);

        return new HkpSimpleMeshShape
        {
            Name = obj.Name,
            Class = obj.Class,
            UserData = (uint)GetLong(data, "userData"),
            DisableWelding = GetString(data, "disableWelding"),
            CollectionType = GetString(data, "collectionType"),
            WeldingType = GetString(data, "weldingType"),
            Radius = GetFloat(data, "radius"),
            MaterialIndices = ToUintArray(GetRaw(data, "materialIndices")),
            Vertices = ToVector4Array(GetRaw(data, "vertices")),
            Triangles = triangles,
        };
    }

    private static HkpSimpleMeshShape.TriangleData[] ParseTriangleDataArray(object? raw)
    {
        if (raw is List<Dictionary<string, object?>> list)
        {
            var result = new HkpSimpleMeshShape.TriangleData[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var d = list[i];
                result[i] = new HkpSimpleMeshShape.TriangleData
                {
                    A = (uint)GetLong(d, "a"),
                    B = (uint)GetLong(d, "b"),
                    C = (uint)GetLong(d, "c"),
                    WeldingInfo = (uint)GetLong(d, "weldingInfo"),
                };
            }

            return result;
        }

        if (raw is List<List<long>> listOfLists)
        {
            var result = new HkpSimpleMeshShape.TriangleData[listOfLists.Count];
            for (int i = 0; i < listOfLists.Count; i++)
            {
                var row = listOfLists[i];
                result[i] = new HkpSimpleMeshShape.TriangleData
                {
                    A = row.Count > 0 ? (uint)row[0] : 0u,
                    B = row.Count > 1 ? (uint)row[1] : 0u,
                    C = row.Count > 2 ? (uint)row[2] : 0u,
                    WeldingInfo = row.Count > 3 ? (uint)row[3] : 0u,
                };
            }

            return result;
        }

        return [];
    }

    private static BaseTagfileObject ConvertHkaRagdollInstance(TagfileDictObject obj, Dictionary<string, object?> data)
    {
        return new HkaRagdollInstance
        {
            Name = obj.Name,
            Class = obj.Class,
            RigidBodies = ToStringArray(GetRaw(data, "rigidBodies")),
            Constraints = ToStringArray(GetRaw(data, "constraints")),
            BoneToRigidBodyMap = ToUintArray(GetRaw(data, "boneToRigidBodyMap")),
            Skeleton = GetString(data, "skeleton"),
        };
    }

    #endregion
}
