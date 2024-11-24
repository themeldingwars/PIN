using System;
using GameServer.Systems.Encounters;

namespace GameServer.Entities;

public class EncounterComponent
{
    [Flags]
    public enum Event : uint
    {
        Signal = 1 << 0,
        Interaction = 1 << 1,
        Donation = 1 << 2,
    }

    public ulong EncounterId { get; set; }
    public IEncounter Instance { get; set; }
    public Event Events { get; set; }
    public bool Handles(Event type) => Events.HasFlag(type);
    public void StartHandling(Event type) => Events |= type;
    public void StopHandling(Event type) => Events &= ~type;
}
