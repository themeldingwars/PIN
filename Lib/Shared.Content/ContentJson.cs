using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Shared.Content.Json;

namespace Shared.Content;

/// <summary>
///     Reads and writes content files. Every tool writes through <see cref="Write{T}"/>, so the files stay in one
///     format and a pull request only shows what actually changed.
/// </summary>
public static class ContentJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,

        // A typo in a field name would otherwise just drop the value
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,

        // Names like Sertão stay readable instead of being escaped
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

        // An encounter's type doesn't have to be its first field, people add fields wherever
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new Vector3Converter(), new QuaternionConverter(), new NumberRangeConverter() },
    };

    private const string SchemaProperty = "$schema";

    /// <exception cref="ContentException">The file isn't valid JSON or doesn't match <paramref name="kind"/>.</exception>
    public static IReadOnlyList<T> Read<T>(ContentKind<T> kind, string json, string path)
        where T : ContentEntry
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ContentException(path, $"expected an object with a \"{kind.ListName}\" list");
            }

            IReadOnlyList<T> entries = [];
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Name == kind.ListName)
                {
                    entries = property.Value.Deserialize<List<T>>(Options) ?? [];
                }
                else if (property.Name != SchemaProperty)
                {
                    throw new ContentException(path, $"unknown property \"{property.Name}\", expected \"{kind.ListName}\"");
                }
            }

            return entries;
        }
        catch (Exception e) when (e is JsonException or NotSupportedException)
        {
            // NotSupportedException is what an encounter without a type ends up as
            throw new ContentException(path, e.Message, e);
        }
    }

    /// <exception cref="ContentException">The file isn't valid JSON or doesn't match <paramref name="kind"/>.</exception>
    public static IReadOnlyList<T> ReadFile<T>(ContentKind<T> kind, string path)
        where T : ContentEntry
    {
        return Read(kind, File.ReadAllText(path), path);
    }

    /// <summary>Reads a file that holds one object instead of a list of entries, e.g. a zone's <c>zone.json</c>.</summary>
    /// <exception cref="ContentException">The file isn't valid JSON or doesn't match <typeparamref name="T"/>.</exception>
    public static T ReadObject<T>(string json, string path)
        where T : class, new()
    {
        try
        {
            if (JsonNode.Parse(json) is not JsonObject root)
            {
                throw new ContentException(path, "expected an object");
            }

            root.Remove(SchemaProperty);
            return root.Deserialize<T>(Options) ?? new T();
        }
        catch (Exception e) when (e is JsonException or NotSupportedException)
        {
            throw new ContentException(path, e.Message, e);
        }
    }

    /// <summary>The canonical text of a file that holds one object, see <see cref="ReadObject{T}"/>.</summary>
    public static string WriteObject<T>(T value, string schemaReference)
        where T : class
    {
        var root = new JsonObject { [SchemaProperty] = schemaReference };
        foreach (var (name, node) in JsonSerializer.SerializeToNode(value, Options)!.AsObject().ToList())
        {
            root[name] = node?.DeepClone();
        }

        var text = new StringBuilder();
        WriteNode(text, root, 0);
        text.Append('\n');
        return text.ToString();
    }

    /// <summary>The canonical text of a content file: entries sorted by key, two space indent, vectors on one line.</summary>
    public static string Write<T>(ContentKind<T> kind, IEnumerable<T> entries)
        where T : ContentEntry
    {
        var root = new JsonObject
        {
            [SchemaProperty] = kind.SchemaReference,
            [kind.ListName] = JsonSerializer.SerializeToNode(kind.Sort(entries).ToList(), Options),
        };

        var text = new StringBuilder();
        WriteNode(text, root, 0);
        text.Append('\n');
        return text.ToString();
    }

    /// <summary>The entry as it's written, for telling whether two versions of an entry differ.</summary>
    public static string Serialize<T>(T entry)
        where T : ContentEntry
    {
        return JsonSerializer.Serialize(entry, Options);
    }

    private static void WriteNode(StringBuilder text, JsonNode? node, int indent)
    {
        switch (node)
        {
            case JsonObject obj when obj.Count == 0:
                text.Append("{}");
                break;
            case JsonObject obj:
                text.Append("{\n");
                var i = 0;
                foreach (var (name, value) in obj)
                {
                    text.Append(' ', indent + 2).Append(JsonSerializer.Serialize(name, Options)).Append(": ");
                    WriteNode(text, value, indent + 2);
                    text.Append(++i < obj.Count ? ",\n" : "\n");
                }

                text.Append(' ', indent).Append('}');
                break;
            case JsonArray array when array.Count == 0:
                text.Append("[]");
                break;

            // Vectors and quaternions, one line each
            case JsonArray array when array.All(item => item is JsonValue):
                text.Append('[').AppendJoin(", ", array.Select(item => item!.ToJsonString(Options))).Append(']');
                break;
            case JsonArray array:
                text.Append("[\n");
                for (var j = 0; j < array.Count; j++)
                {
                    text.Append(' ', indent + 2);
                    WriteNode(text, array[j], indent + 2);
                    text.Append(j + 1 < array.Count ? ",\n" : "\n");
                }

                text.Append(' ', indent).Append(']');
                break;
            default:
                text.Append(node?.ToJsonString(Options) ?? "null");
                break;
        }
    }
}
