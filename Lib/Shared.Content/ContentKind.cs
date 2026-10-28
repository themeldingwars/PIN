namespace Shared.Content;

/// <summary>
///     A kind of content that lives in one file per zone, <c>zones/&lt;zone id&gt;/&lt;name&gt;.json</c>, with its schema in
///     <c>schemas/&lt;name&gt;.schema.json</c>.
/// </summary>
public abstract class ContentKind(string name)
{
    public string Name { get; } = name;

    public string FileName => $"{Name}.json";
}
