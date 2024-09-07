namespace GameServer.Data.SDB.Records.aptfs;
public record class RopePullCommandDef : ICommandDef
{
    public uint Skiprollback { get; set; }
    public float Range { get; set; }
    public float Speed { get; set; }
    public uint Beamfxid { get; set; }
    public uint Id { get; set; }
}
