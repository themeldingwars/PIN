using GameServer.Systems.Encounters;

namespace GameServer.Entities;

public class EncounterComponent
{
    public ulong EncounterId { get; set; }
    public IEncounter Instance { get; set; }
}