namespace GameServer.Data.SDB.Records.aptfs;
public record class ShootingDurationCommandDef
{
    public int Timeoffset { get; set; }
    public uint Id { get; set; }
    public byte Inittime { get; set; }
    public byte Ended { get; set; }
    public byte Frominput { get; set; }
    public byte Negate { get; set; }
}
