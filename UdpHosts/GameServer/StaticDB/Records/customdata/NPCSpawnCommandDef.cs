namespace GameServer.StaticDB.Records.customdata;

public record NPCSpawnCommandDef : ICommandDef
{
    public uint Id { get; set; }

    // [AeroSdb("dbcharacter::Monster", "id")]
    public uint MonsterId { get; set; } = 0;
    public bool SetOwner { get; set; } = false;

    // Monster level (indexes dbcharacter::MonsterScaling). 0 = use the character default level.
    public byte Level { get; set; } = 0;

    // Server-only NPC: never scoped to any client. Used by the turret gunner spawns, which the original server
    // references through Turret::ObserverView.GunnerId but never sends a Character view for.
    public bool Hidden { get; set; } = false;
}