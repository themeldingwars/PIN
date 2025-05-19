namespace GameServer.Data.SDB.Records.dbitems;
public record class LevelItemAttributes
{
    public uint AttributeId { get; set; }
    public uint Level { get; set; }
    public float Value { get; set; }
}