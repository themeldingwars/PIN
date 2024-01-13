namespace GameServer.Data.SDB.Records.dbitems;
public record class AttributeDefinition
{
    public uint Id { get; set; }
    public uint LocalizedNameId { get; set; }
    public uint AttributeCategory { get; set; }
    public float ModuleEffect { get; set; }
    public uint DisplayOrder { get; set; }
    public uint FormatSpecifierId { get; set; }
    public uint LocalizedDescriptionId { get; set; }
    public string Name { get; set; }
    public byte IsModifier { get; set; }
    public byte DataType { get; set; }
    public byte Inverse { get; set; }
}