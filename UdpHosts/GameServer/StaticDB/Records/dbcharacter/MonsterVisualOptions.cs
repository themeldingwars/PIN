namespace GameServer.StaticDB.Records.dbcharacter;

public record class MonsterVisualOptions
{
    public uint Id { get; set; }
    public byte Female { get; set; }
    public byte Male { get; set; }
}