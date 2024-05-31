using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static GameServer.Physics.ZoneLoader.ENWFData;

namespace GameServer.Physics.ZoneLoader;

public class TagfileObjectJsonConverter : JsonConverter<BaseTagfileObject>
{
    public override BaseTagfileObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;

            string className = root.GetProperty("class").GetString();
            JsonElement dataElement = root.GetProperty("data");

            BaseTagfileObject result;
            switch (className)
            {
                case "hkpListShape":
                    result = JsonSerializer.Deserialize<HkpListShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpMoppBvTreeShape":
                    result = JsonSerializer.Deserialize<HkpMoppBvTreeShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpConvexTranslateShape":
                    result = JsonSerializer.Deserialize<HkpConvexTranslateShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpBoxShape":
                    result = JsonSerializer.Deserialize<HkpBoxShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpSphereShape":
                    result = JsonSerializer.Deserialize<HkpSphereShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpCapsuleShape":
                    result = JsonSerializer.Deserialize<HkpCapsuleShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpCylinderShape":
                    result = JsonSerializer.Deserialize<HkpCylinderShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpTransformShape":
                    result = JsonSerializer.Deserialize<HkpTransformShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpConvexTransformShape":
                    result = JsonSerializer.Deserialize<HkpConvexTransformShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpConvexVerticesShape":
                    result = JsonSerializer.Deserialize<HkpConvexVerticesShapeObject>(dataElement.GetRawText(), options);
                    break;
                case "hkpExtendedMeshShape":
                    result = JsonSerializer.Deserialize<HkpExtendedMeshShapeObject>(dataElement.GetRawText(), options);
                    break;
                default:
                    throw new InvalidOperationException("Unknown class type: " + className);
            }

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