namespace Shared.Content;

/// <summary>A problem in the content, <see cref="ZoneId"/> is null for content outside the zones, e.g. spawn tables.</summary>
public sealed record ContentProblem(uint? ZoneId, string Kind, string? Key, string Message)
{
    public override string ToString()
    {
        var where = ZoneId == null ? Kind : $"zone {ZoneId} {Kind}";
        return Key == null ? $"{where}: {Message}" : $"{where} {Key}: {Message}";
    }
}
