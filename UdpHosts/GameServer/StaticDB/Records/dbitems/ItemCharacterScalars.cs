namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemCharacterScalars
{
    public uint ItemId { get; set; }
    public float PerLevel { get; set; }
    public float Value { get; set; }
    public ushort AttributeCategory { get; set; }
}