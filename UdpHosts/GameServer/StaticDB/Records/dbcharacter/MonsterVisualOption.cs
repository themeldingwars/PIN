namespace GameServer.StaticDB.Records.dbcharacter;

public record class MonsterVisualOption
{
    public uint Parent { get; set; }
    public uint Value { get; set; }
    public byte Type { get; set; }
}