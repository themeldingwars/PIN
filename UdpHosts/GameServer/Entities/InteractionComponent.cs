namespace GameServer.Entities;

public class InteractionComponent
{
    public float Radius { get; set; } = 1.0f;
    public float Height { get; set; } = 1.5f;
    public uint CompletedAbilityId { get; set; }
    public uint StartedAbilityId { get; set; }
    public uint DurationMs { get; set; }
    public InteractionType Type { get; set; }
    public uint VendorId { get; set; }
}