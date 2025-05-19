namespace GameServer.Entities;

public class InteractionComponent
{
    public float Radius { get; set; } = 1.0f;
    public float Height { get; set; } = 1.5f;
    public uint CompletedAbilityId { get; set; } = 0;
    public uint StartedAbilityId { get; set; } = 0;
    public uint DurationMs { get; set; } = 0;
    public InteractionType Type { get; set; } = 0;
    public uint VendorId { get; set; } = 0;
}