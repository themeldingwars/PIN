namespace GameServer.Data.SDB.Records.dbitems;
public record class AttributeCategory
{
    public uint Id { get; set; }
    public uint LocalizedNameId { get; set; }
    public uint FormatSpecifierId { get; set; }
    public uint LocalizedDescriptionId { get; set; }
    public string Name { get; set; }
    public float ModuleEffectiveness { get; set; }
    public byte IsScalar { get; set; }
}