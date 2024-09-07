namespace GameServer.Data.SDB.Records.aptfs;
public record class HasTargetsDurationCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public ushort MinCount { get; set; }
    public byte Negate { get; set; }
}
