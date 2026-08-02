using Shared.Collision.Tagfile.Binary.Schema;
using Shared.Collision.Tagfile.Models;

namespace Shared.Collision.Tagfile.Binary;

public class TagfileDictionaryParser
{
    private static readonly Dictionary<string, HashSet<string>> _allSpecs = new()
    {
        { "hkpBoxShape", new() { "userData", "radius", "halfExtents" } },
        { "hkpSphereShape", new() { "userData", "radius" } },
        { "hkpCylinderShape", new() { "userData", "radius", "cylRadius", "cylBaseRadiusFactorForHeightFieldCollisions", "vertexA", "vertexB", "perpendicular1", "perpendicular2" } },
        { "hkpConvexTranslateShape", new() { "userData", "radius", "childShape", "translation" } },
        { "hkpCapsuleShape", new() { "userData", "radius", "vertexA", "vertexB" } },
        { "hkpMoppBvTreeShape", new() { "userData", "bvTreeType", "code", "child" } },
        { "hkpTransformShape", new() { "userData", "childShape", "rotation", "transform" } },
        { "hkpConvexTransformShape", new() { "userData", "radius", "childShape", "transform" } },
        { "hkpConvexVerticesShape", new() { "userData", "radius", "aabbHalfExtents", "aabbCenter", "rotatedVertices", "numVertices", "planeEquations", "connectivity" } },
        { "hkpListShape", new() { "userData", "disableWelding", "collectionType", "childInfo", "flags", "numDisabledChildren", "aabbHalfExtents", "aabbCenter", "enabledChildren" } },
        { "hkpExtendedMeshShape", new() { "userData", "disableWelding", "collectionType", "embeddedTrianglesSubpart", "aabbHalfExtents", "aabbCenter", "numBitsForSubpartIndex", "trianglesSubparts", "shapesSubparts", "weldingInfo", "weldingType", "defaultCollisionFilterInfo", "cachedNumChildShapes", "triangleRadius" } },
        { "hkRootLevelContainer", new() { "namedVariants" } },
        { "hkpRigidBody", new() { "collidable", "motion" } },
        { "hkpStorageExtendedMeshShape", new() { "userData", "disableWelding", "collectionType", "embeddedTrianglesSubpart", "aabbHalfExtents", "aabbCenter", "numBitsForSubpartIndex", "trianglesSubparts", "shapesSubparts", "weldingInfo", "weldingType", "defaultCollisionFilterInfo", "cachedNumChildShapes", "triangleRadius", "meshstorage", "shapestorage" } },
        { "hkpStorageExtendedMeshShapeMeshSubpartStorage", new() { "vertices", "indices8", "indices16", "indices32", "materialIndices" } },
        { "hkpSimpleMeshShape", new() { "userData", "disableWelding", "collectionType", "vertices", "triangles", "radius", "weldingType" } },
        { "hkpPhysicsData", new() { "worldCinfo", "systems" } },
        { "hkpPhysicsSystem", new() { "rigidBodies", "constraints", "actions", "phantoms", "name", "userData", "active" } },
        { "hkaRagdollInstance", new() { "rigidBodies", "constraints", "boneToRigidBodyMap", "skeleton" } },

        // Referenced types — added so they go to processed instead of raw
        { "hkAabb", new() { "min", "max" } },
        { "hkLocalFrame", new() },
        { "hkReferencedObject", new() },
        { "hkRootLevelContainerNamedVariant", new() { "name", "className", "variant" } },
        { "hkWorldMemoryAvailableWatchDog", new() },
        { "hkaBone", new() { "name", "lockTranslation" } },
        { "hkaSkeleton", new() { "name", "parentIndices", "bones", "referencePose", "referenceFloats", "floatSlots", "localFrames" } },
        { "hkaSkeletonLocalFrameOnBone", new() { "localFrame", "boneIndex" } },
        { "hkpAction", new() { "userData", "name" } },
        { "hkpCollisionFilter", new() { "prepad", "type", "postpad" } },
        { "hkpCompressedMeshShapeConvexPiece", new() { "offset", "vertices", "faceVertices", "faceOffsets", "reference", "transformIndex" } },
        { "hkpConstraintAtom", new() { "type" } },
        { "hkpConstraintData", new() { "userData" } },
        { "hkpConstraintInstance", new() { "data", "constraintModifiers", "entities", "priority", "wantRuntime", "destructionRemapInfo", "name", "userData" } },
        { "hkpConstraintInstanceSmallArraySerializeOverrideType", new() { "size" } },
        { "hkpConvexListFilter", new() },
        { "hkpConvexShape", new() { "userData", "radius" } },
        { "hkpConvexVerticesConnectivity", new() { "vertexIndices", "numVerticesPerFace" } },
        { "hkpConvexVerticesShapeFourVectors", new() { "x", "y", "z" } },
        { "hkpEntity", new() { "shape", "name", "userData", "material", "damageMultiplier", "storageIndex", "contactPointCallbackDelay", "autoRemoveLevel", "numShapeKeysInContactPointProperties", "responseModifierFlags", "uid", "spuCollisionCallback", "motion", "localFrame", "npData" } },
        { "hkpEntityExtendedListeners", new() },
        { "hkpEntitySmallArraySerializeOverrideType", new() { "size", "capacityAndFlags" } },
        { "hkpEntitySpuCollisionCallback", new() { "eventFilter", "userFilter" } },
        { "hkpExtendedMeshShapeShapesSubpart", new() { "type", "materialIndexStridingType", "materialIndexStriding", "numMaterials", "userData", "childShapes", "rotation", "translation" } },
        { "hkpExtendedMeshShapeTrianglesSubpart", new() { "type", "materialIndexStridingType", "materialIndexStriding", "numMaterials", "userData", "numTriangleShapes", "numVertices", "vertexStriding", "triangleOffset", "indexStriding", "stridingType", "flipAlternateTriangles", "extrusion", "transform" } },
        { "hkpListShapeChildInfo", new() { "shape", "collisionFilterInfo" } },
        { "hkpMaterial", new() { "responseType", "friction", "restitution" } },
        { "hkpMaxSizeMotion", new() },
        { "hkpModifierConstraintAtom", new() { "type", "modifierAtomSize", "childSize", "child", "pad" } },
        { "hkpNamedMeshMaterial", new() { "filterInfo", "name" } },
        { "hkpPhantom", new() { "shape", "name", "userData" } },
        { "hkpShape", new() { "userData" } },
        { "hkpSimpleMeshShapeTriangle", new() { "a", "b", "c", "weldingInfo" } },
        { "hkpSingleShapeContainer", new() { "childShape" } },
        { "hkpStorageExtendedMeshShapeMaterial", new() { "filterInfo", "restitution", "friction", "userData" } },
        { "hkpStorageExtendedMeshShapeShapeSubpartStorage", new() { "materialIndices", "materials", "materialIndices16" } },
        { "hkpWorldCinfo", new() { "gravity", "broadPhaseQuerySize", "contactRestingVelocity", "broadPhaseBorderBehaviour", "mtPostponeAndSortBroadPhaseBorderCallbacks", "broadPhaseWorldAabb", "useKdTree", "useMultipleTree", "treeUpdateType", "autoUpdateKdTree", "collisionTolerance", "collisionFilter", "convexListFilter", "expectedMaxLinearVelocity", "sizeOfToiEventQueue", "expectedMinPsiDeltaTime", "memoryWatchDog", "broadPhaseNumMarkers", "contactPointGeneration", "allowToSkipConfirmedCallbacks", "solverTau", "solverDamp", "solverIterations", "solverMicrosteps", "maxConstraintViolation", "forceCoherentConstraintOrderingInSolver", "snapCollisionToConvexEdgeThreshold", "snapCollisionToConcaveEdgeThreshold", "enableToiWeldRejection", "enableDeprecatedWelding", "iterativeLinearCastEarlyOutDistance", "iterativeLinearCastMaxIterations", "deactivationNumInactiveFramesSelectFlag0", "deactivationNumInactiveFramesSelectFlag1", "deactivationIntegrateCounter", "shouldActivateOnRigidBodyTransformChange", "deactivationReferenceDistance", "toiCollisionResponseRotateNormal", "maxSectorsPerCollideTask", "processToisMultithreaded", "maxEntriesPerToiCollideTask", "maxNumToiCollisionPairsSinglethreaded", "numToisTillAllowedPenetrationSimplifiedToi", "numToisTillAllowedPenetrationToi", "numToisTillAllowedPenetrationToiHigher", "numToisTillAllowedPenetrationToiForced", "enableDeactivation", "simulationType", "enableSimulationIslands", "minDesiredIslandSize", "processActionsInSingleThread", "allowIntegrationOfIslandsWithoutConstraintsInASeparateJob", "frameMarkerPsiSnap", "fireCollisionCallbacks" } },
    };

    public Task<DictionaryTagfile> ParseAsync(byte[] data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var binary = TagfileParser.ParseTagfile(reader);

        var processed = new List<TagfileDictObject>();
        var raw = new List<TagfileDictObject>();

        foreach (var kvp in binary.Objects)
        {
            var objData = kvp.Value;
            var tagName = TagfileParser.ObjectRefName(kvp.Key);

            var allData = ConvertMembers(objData, binary);

            if (_allSpecs.TryGetValue(objData.ClassName, out var specParams))
            {
                FillDefaults(objData.ClassName, allData);
                var specData = new Dictionary<string, object?>();
                foreach (var paramName in specParams)
                {
                    if (allData.TryGetValue(paramName, out var val))
                    {
                        specData[paramName] = val;
                    }
                }

                processed.Add(new TagfileDictObject(tagName, objData.ClassName, specData));
            }
            else
            {
                var rawParams = new List<Dictionary<string, object?>>();
                foreach (var md in allData)
                {
                    rawParams.Add(new Dictionary<string, object?>
                    {
                        { "@name", md.Key },
                        { "_", md.Value }
                    });
                }

                raw.Add(new TagfileDictObject(tagName, objData.ClassName, new Dictionary<string, object?> { { "raw", rawParams } }));
            }
        }

        return Task.FromResult(new DictionaryTagfile(processed, raw));
    }

    private static Dictionary<string, object?> ConvertMembers(
        TagfileParser.ObjectData objData,
        TagfileParser.BinaryTagfile binary)
    {
        var result = new Dictionary<string, object?>();

        foreach (var kvp in objData.MembersData)
        {
            var memberName = kvp.Key;
            var rawValue = kvp.Value;
            result[memberName] = ConvertValue(rawValue, objData.ClassName, memberName, binary);
        }

        return result;
    }

    private static object? ConvertValue(object? rawValue, string className, string memberName, TagfileParser.BinaryTagfile binary)
    {
        if (rawValue == null)
        {
            return null;
        }

        var typeInfo = GetTypeInfo(className, memberName);

        if (rawValue is TagfileParser.ObjectData objData)
        {
            return ConvertObjectRefOrEmbedded(objData, binary);
        }

        if (typeInfo != null && typeInfo.Type == "TYPE_ENUM")
        {
            return ConvertEnum(rawValue, typeInfo.ResolvedEnumName);
        }

        if (rawValue is object[] arr && arr.Length > 0 && arr[0] is TagfileParser.ObjectData)
        {
            var result = new List<Dictionary<string, object?>>();
            foreach (var item in arr)
            {
                if (item is TagfileParser.ObjectData elem)
                {
                    var converted = ConvertObjectRefOrEmbedded(elem, binary);
                    if (converted is Dictionary<string, object?> dict)
                    {
                        result.Add(dict);
                    }
                }
            }

            return result;
        }

        if (rawValue is object[] vecArr && vecArr.Length > 0 && (vecArr[0] is ValueTuple<float, float, float> || vecArr[0] is ValueTuple<float, float, float, float>))
        {
            var vecList = new List<object>();
            foreach (var item in vecArr)
            {
                vecList.Add(item);
            }

            return ConvertVecList(vecList);
        }

        if (rawValue is object[] objArr && objArr.Length > 0 && objArr[0] is string)
        {
            var refs = new List<string>();
            foreach (var item in objArr)
            {
                if (item is string s)
                {
                    refs.Add(s);
                }
            }

            return refs;
        }

        return ConvertPrimitive(rawValue, typeInfo);
    }

    private static object? ConvertObjectRefOrEmbedded(TagfileParser.ObjectData objData, TagfileParser.BinaryTagfile binary)
    {
        foreach (var kvp in binary.Objects)
        {
            if (ReferenceEquals(kvp.Value, objData))
            {
                return TagfileParser.ObjectRefName(kvp.Key);
            }
        }

        var allData = ConvertMembers(objData, binary);
        return allData;
    }

    private static object? ConvertEnum(object? value, string? enumName)
    {
        if (value == null || enumName == null)
        {
            return value;
        }

        var numVal = ExtractIntValue(value);

        foreach (var kvp in TagfileClassSchema.Classes)
        {
            foreach (var e in kvp.Value.Enums)
            {
                if (e.Name == enumName)
                {
                    foreach (var ev in e.Values)
                    {
                        if (ev.Value == numVal)
                        {
                            return ev.Name;
                        }
                    }
                }
            }
        }

        return numVal;
    }

    private static int ExtractIntValue(object? value)
    {
        if (value is ValueTuple<uint, bool> vt)
        {
            return (int)ConvertIntValue(vt, null);
        }

        if (value is uint u)
        {
            return (int)u;
        }

        if (value is long l)
        {
            return (int)l;
        }

        if (value is int i)
        {
            return i;
        }

        return 0;
    }

    private static object? ConvertPrimitive(object? value, TagfileMemberInfo? typeInfo)
    {
        if (value == null)
        {
            return null;
        }

        if (value is ValueTuple<uint, bool> vb)
        {
            return ConvertIntValue(vb, typeInfo);
        }

        if (value is float f)
        {
            return (double)f;
        }

        if (value is ValueTuple<float, float, float> v3)
        {
            return new List<double> { v3.Item1, v3.Item2, v3.Item3 };
        }

        if (value is ValueTuple<float, float, float, float> v4)
        {
            return new List<double> { v4.Item1, v4.Item2, v4.Item3, v4.Item4 };
        }

        if (value is List<object> objList)
        {
            return ConvertVecList(objList);
        }

        if (value is object[] arr)
        {
            return ConvertArray(arr, typeInfo);
        }

        if (value is byte b)
        {
            return b;
        }

        return value;
    }

    private static long ConvertIntValue((uint, bool) vb, TagfileMemberInfo? typeInfo)
    {
        long val = vb.Item2 ? -vb.Item1 : vb.Item1;

        if (typeInfo != null && typeInfo.Type.StartsWith("TYPE_U", StringComparison.InvariantCulture))
        {
            return (uint)(int)val;
        }

        return val;
    }

    private static object? ConvertVecList(List<object> list)
    {
        var result = new List<List<double>>();
        foreach (var item in list)
        {
            if (item is ValueTuple<float, float, float> v3)
            {
                result.Add([v3.Item1, v3.Item2, v3.Item3]);
            }
            else if (item is ValueTuple<float, float, float, float> v4)
            {
                result.Add([v4.Item1, v4.Item2, v4.Item3, v4.Item4]);
            }
        }

        return result;
    }

    private static object? ConvertArray(object[] arr, TagfileMemberInfo? typeInfo)
    {
        var result = new List<long>();
        foreach (var item in arr)
        {
            if (item is ValueTuple<uint, bool> vb)
            {
                result.Add(ConvertIntValue(vb, typeInfo));
            }
            else if (item is float f)
            {
                result.Add((long)f);
            }
            else if (item is byte b)
            {
                result.Add(b);
            }
            else if (item is int i)
            {
                result.Add(i);
            }
        }

        return result;
    }

    private static TagfileMemberInfo? GetTypeInfo(string className, string memberName)
    {
        return TagfileClassSchema.TryGetMember(className, memberName, out var info)
            ? info
            : null;
    }

    private static void FillDefaults(string className, Dictionary<string, object?> data)
    {
        if (!_allSpecs.TryGetValue(className, out var specParams))
        {
            return;
        }

        foreach (var paramName in specParams)
        {
            if (data.ContainsKey(paramName))
            {
                continue;
            }

            if (!TagfileClassSchema.TryGetMember(className, paramName, out var info))
            {
                continue;
            }

            var @default = GetDefaultForType(info);
            if (@default != null)
            {
                data[paramName] = @default;
            }
        }
    }

    private static object? GetDefaultForType(TagfileMemberInfo info)
    {
        var type = info.Type;

        if (type.StartsWith("TYPE_U", StringComparison.InvariantCulture) || type.StartsWith("TYPE_I", StringComparison.InvariantCulture))
        {
            return 0L;
        }

        if (type == "TYPE_BOOL")
        {
            return false;
        }

        if (type == "TYPE_ENUM" && info.ResolvedEnumName != null)
        {
            return GetFirstEnumValue(info.ResolvedEnumName);
        }

        if (type == "TYPE_REAL" || type == "TYPE_HALF")
        {
            return 0.0;
        }

        if (type == "TYPE_VECTOR4" || type == "TYPE_VEC_4")
        {
            return new List<double> { 0, 0, 0, 0 };
        }

        if (type == "TYPE_VEC_8")
        {
            return new List<List<double>> { new() { 0, 0, 0, 0 }, new() { 0, 0, 0, 0 } };
        }

        if (type == "TYPE_VEC_12")
        {
            return new List<List<double>> { new() { 0, 0, 0, 0 }, new() { 0, 0, 0, 0 }, new() { 0, 0, 0, 0 } };
        }

        if (type == "TYPE_VEC_16")
        {
            return new List<List<double>> { new() { 0, 0, 0, 0 }, new() { 0, 0, 0, 0 }, new() { 0, 0, 0, 0 }, new() { 0, 0, 0, 0 } };
        }

        if (type == "TYPE_QUATERNION")
        {
            return new List<double> { 0, 0, 0, 1 };
        }

        if (type == "TYPE_TRANSFORM")
        {
            return new List<List<double>>
            {
                new() { 1, 0, 0, 0 }, new() { 0, 1, 0, 0 }, new() { 0, 0, 1, 0 }, new() { 0, 0, 0, 0 }
            };
        }

        if (type == "TYPE_QSTRANSFORM")
        {
            return new Dictionary<string, List<double>>
            {
                { "translation", new List<double> { 0, 0, 0, 0 } },
                { "rotation", new List<double> { 0, 0, 0, 1 } },
                { "scale", new List<double> { 1, 1, 1, 1 } }
            };
        }

        return null;
    }

    private static string? GetFirstEnumValue(string enumName)
    {
        foreach (var kvp in TagfileClassSchema.Classes)
        {
            foreach (var e in kvp.Value.Enums)
            {
                if (e.Name == enumName)
                {
                    foreach (var ev in e.Values)
                    {
                        if (ev.Value == 0)
                        {
                            return ev.Name;
                        }
                    }
                }
            }
        }

        return null;
    }
}
