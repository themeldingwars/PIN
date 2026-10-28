namespace Shared.Content;

public sealed record ContentProblem(uint ZoneId, string Kind, string? Key, string Message)
{
    public override string ToString() => Key == null ? $"zone {ZoneId} {Kind}: {Message}" : $"zone {ZoneId} {Kind} {Key}: {Message}";
}
