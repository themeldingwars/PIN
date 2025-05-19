namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireInRangeCommandDef : ICommandDef
{
    public float Range { get; set; }
    public uint Id { get; set; }
    public byte Useoffset { get; set; }
    public byte Negate { get; set; }
}