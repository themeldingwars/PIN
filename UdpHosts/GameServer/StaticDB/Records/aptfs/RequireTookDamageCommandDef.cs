namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireTookDamageCommandDef : ICommandDef
{
    public int Timeoffset { get; set; }
    public uint Id { get; set; }
    public byte Inittime { get; set; }
    public byte Negate { get; set; }
}