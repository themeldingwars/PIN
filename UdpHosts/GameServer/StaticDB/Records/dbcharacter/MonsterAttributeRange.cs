namespace GameServer.Data.SDB.Records.dbcharacter;
public record class MonsterAttributeRange
{
    public uint MonsterId { get; set; }
    public ushort AttributeId { get; set; }
    public float ModuleMin { get; set; }
    public float ModuleMax { get; set; }
    public float Base { get; set; }
    public float PerLevel { get; set; }
    public float ModuleEffect { get; set; }
}
