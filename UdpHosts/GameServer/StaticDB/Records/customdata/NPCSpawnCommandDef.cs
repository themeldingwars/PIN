namespace GameServer.Data.SDB.Records.customdata;

public record NPCSpawnCommandDef : ICommandDef
{
    public uint Id { get; set; }

    // [AeroSdb("dbcharacter::Monster", "id")]
    public uint MonsterId { get; set; } = 0;
    public bool SetOwner { get; set; } = false;
}