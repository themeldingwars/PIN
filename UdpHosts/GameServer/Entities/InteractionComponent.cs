using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Entities;

public class InteractionComponent
{
    public float Radius { get; set; }
    public float Height { get; set; }
    public uint CompletedAbilityId { get; set; }
    public uint StartedAbilityId { get; set; }
    public uint DurationMs { get; set; }
    public InteractionType Type { get; set; }
    public uint VendorId { get; set; }
}