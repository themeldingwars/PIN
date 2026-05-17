using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameServer.Physics.TagfileLoader;

public class TagfileObjectJsonConverter : JsonConverter<BaseTagfileObject>
{
    public override BaseTagfileObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;

            string className = root.GetProperty("class").GetString();
            JsonElement dataElement = root.GetProperty("data");
            string rawText = dataElement.GetRawText();
            BaseTagfileObject result = className switch
            {
                "hkpListShape" => JsonSerializer.Deserialize<HkpListShapeObject>(rawText, options),
                "hkpMoppBvTreeShape" => JsonSerializer.Deserialize<HkpMoppBvTreeShapeObject>(rawText, options),
                "hkpConvexTranslateShape" => JsonSerializer.Deserialize<HkpConvexTranslateShapeObject>(rawText, options),
                "hkpBoxShape" => JsonSerializer.Deserialize<HkpBoxShapeObject>(rawText, options),
                "hkpSphereShape" => JsonSerializer.Deserialize<HkpSphereShapeObject>(rawText, options),
                "hkpCapsuleShape" => JsonSerializer.Deserialize<HkpCapsuleShapeObject>(rawText, options),
                "hkpCylinderShape" => JsonSerializer.Deserialize<HkpCylinderShapeObject>(rawText, options),
                "hkpTransformShape" => JsonSerializer.Deserialize<HkpTransformShapeObject>(rawText, options),
                "hkpConvexTransformShape" => JsonSerializer.Deserialize<HkpConvexTransformShapeObject>(rawText, options),
                "hkpConvexVerticesShape" => JsonSerializer.Deserialize<HkpConvexVerticesShapeObject>(rawText, options),
                "hkpExtendedMeshShape" => JsonSerializer.Deserialize<HkpExtendedMeshShapeObject>(rawText, options),
                "hkRootLevelContainer" => JsonSerializer.Deserialize<HkRootLevelContainerObject>(rawText, options),
                "hkpRigidBody" => JsonSerializer.Deserialize<HkpRigidBody>(rawText, options),
                _ => throw new InvalidOperationException("Unknown class type: " + className),
            };
            result.Name = root.GetProperty("name").GetString();
            result.Class = className;

            return result;
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseTagfileObject value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Only reading is implemented.");
    }
}