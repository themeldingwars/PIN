namespace GameServer.Data.SDB.Records.dbcharacter;

public record class MonsterVisualOption
{
    public int Parent { get; set; }
    public long Value { get; set; }
    public int Type { get; set; }
}