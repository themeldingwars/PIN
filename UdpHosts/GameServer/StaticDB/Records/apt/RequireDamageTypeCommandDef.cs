namespace GameServer.Data.SDB.Records.apt;
public record class RequireDamageTypeCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Damagetype { get; set; }
    public byte Negate { get; set; }
}