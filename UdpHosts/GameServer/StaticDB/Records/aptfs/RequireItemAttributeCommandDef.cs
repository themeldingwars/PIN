namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireItemAttributeCommandDef : ICommandDef
{
    public uint AttributeId { get; set; }
    public uint Id { get; set; }
}
