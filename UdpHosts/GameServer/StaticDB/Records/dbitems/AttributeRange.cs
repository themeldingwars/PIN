namespace GameServer.Data.SDB.Records.dbitems;
public record class AttributeRange
{
    public uint ItemId { get; set; }
    public ushort AttributeId { get; set; }
    public float Base { get; set; }
    public float ModuleMin { get; set; }
    public float ModuleMax { get; set; }
    public float PerLevel { get; set; }
    public float ModuleEffect { get; set; }
}