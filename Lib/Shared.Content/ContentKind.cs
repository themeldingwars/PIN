namespace Shared.Content;

/// <summary>
///     A kind of content with its schema in <c>schemas/&lt;name&gt;.schema.json</c>. A zone kind lives in one file per zone,
///     <c>zones/&lt;zone id&gt;/&lt;name&gt;.json</c>, any other kind in files of its own folder, <c>&lt;name&gt;/*.json</c>.
/// </summary>
public abstract class ContentKind(string name, bool perZone)
{
    public string Name { get; } = name;

    public bool PerZone { get; } = perZone;

    public string FileName => $"{Name}.json";

    public string ListName => Name.Replace('-', '_');

    public string SchemaReference => PerZone ? $"../../schemas/{Name}.schema.json" : $"../schemas/{Name}.schema.json";
}
