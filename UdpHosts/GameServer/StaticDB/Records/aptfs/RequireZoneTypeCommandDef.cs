namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireZoneTypeCommandDef : ICommandDef
{
    public uint SpecificZoneId { get; set; }
    public uint Id { get; set; }
    public byte Holmgang { get; set; }
    public byte Adventure { get; set; }
    public byte Other { get; set; }
    public byte OpenWorld { get; set; }
    public byte Negate { get; set; }
}